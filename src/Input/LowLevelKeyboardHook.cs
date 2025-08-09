using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace WinKeysRemapper.Input
{
    public class LowLevelKeyboardHook
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;
        private const int HC_ACTION = 0;
        
        private const uint LEFT_ARROW_SCAN = 0x4B;
        private const uint UP_ARROW_SCAN = 0x48;  
        private const uint RIGHT_ARROW_SCAN = 0x4D;
        private const uint DOWN_ARROW_SCAN = 0x50;
        
        private const uint KEYBOARDMANAGER_INJECTED_FLAG = 0xFFFFFFF0;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        
        private readonly LowLevelKeyboardProc _proc = null!;
        private IntPtr _hookID = IntPtr.Zero;
        private static LowLevelKeyboardHook? _instance;
        
        private readonly Dictionary<int, int> _keyMappings;
        private readonly string _targetApplication;
        
        private uint _lastProcessId = 0;
        private bool _lastResult = false;
        
        private readonly HashSet<int> _pressedKeys = new HashSet<int>();
        private readonly object _keyStateLock = new object();
        
        private volatile bool _hookInstalled = false;
        private readonly object _hookLock = new object();

        private static readonly string DebugLogPath = Path.Combine(Path.GetTempPath(), "WinKeysRemapper_Debug.log");
        private static readonly object LogLock = new object();

        private static void LogDebug(string message)
        {
            try
            {
                lock (LogLock)
                {
                    File.AppendAllText(DebugLogPath, $"{DateTime.Now:HH:mm:ss.fff} - {message}\n");
                }
            }
            catch
            {
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        public LowLevelKeyboardHook(Dictionary<int, int> keyMappings, string targetApplication)
        {
            _keyMappings = keyMappings ?? new Dictionary<int, int>();
            _targetApplication = targetApplication ?? throw new ArgumentNullException(nameof(targetApplication));
            
            _proc = HookCallback;
            _instance = this;
        }

        private bool IsTargetApplicationActive()
        {
            try
            {
                var hwnd = GetForegroundWindow();
                if (hwnd == IntPtr.Zero) return false;
                
                GetWindowThreadProcessId(hwnd, out uint processId);
                
                if (processId == _lastProcessId)
                {
                    return _lastResult;
                }
                
                using var process = System.Diagnostics.Process.GetProcessById((int)processId);
                var processName = process.ProcessName.ToLowerInvariant();
                var result = processName.Contains(_targetApplication.ToLowerInvariant());
                
                _lastProcessId = processId;
                _lastResult = result;
                
                return result;
            }
            catch
            {
                _lastProcessId = 0;
                _lastResult = false;
                return false;
            }
        }

        public void InstallHook()
        {
            lock (_hookLock)
            {
                if (!_hookInstalled && _hookID == IntPtr.Zero)
                {
                    _hookID = SetHook(_proc);
                    if (_hookID != IntPtr.Zero)
                    {
                        _hookInstalled = true;
                    }
                    else
                    {
                        var error = Marshal.GetLastWin32Error();
                        throw new InvalidOperationException($"Failed to install keyboard hook, error: {error}");
                    }
                }
            }
        }

        public void UninstallHook()
        {
            lock (_hookLock)
            {
                if (_hookInstalled && _hookID != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(_hookID);
                    _hookID = IntPtr.Zero;
                    _hookInstalled = false;
                    
                    lock (_keyStateLock)
                    {
                        _pressedKeys.Clear();
                    }
                }
            }
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule!)
            {
                var moduleHandle = GetModuleHandle(curModule.ModuleName);
                var hookId = SetWindowsHookEx(WH_KEYBOARD_LL, proc, moduleHandle, 0);
                return hookId;
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode != HC_ACTION)
            {
                return CallNextHookEx(_instance?._hookID ?? IntPtr.Zero, nCode, wParam, lParam);
            }

            if (_instance == null)
            {
                return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
            }

            var hookStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);

            if (hookStruct.dwExtraInfo == (UIntPtr)KEYBOARDMANAGER_INJECTED_FLAG)
            {
                return CallNextHookEx(_instance._hookID, nCode, wParam, lParam);
            }

            var wParamInt = (int)wParam;
            bool isKeyDown = (wParamInt == WM_KEYDOWN || wParamInt == WM_SYSKEYDOWN);
            bool isKeyUp = (wParamInt == WM_KEYUP || wParamInt == WM_SYSKEYUP);
            
            if (!isKeyDown && !isKeyUp)
            {
                return CallNextHookEx(_instance._hookID, nCode, wParam, lParam);
            }

            var vkCode = (int)hookStruct.vkCode;
            
            if (_instance._keyMappings.TryGetValue(vkCode, out int mappedKey))
            {
                lock (_instance._keyStateLock)
                {
                    if (isKeyDown)
                    {
                        if (!_instance._pressedKeys.Contains(vkCode))
                        {
                            _instance._pressedKeys.Add(vkCode);
                            
                            if (mappedKey >= 0x25 && mappedKey <= 0x28)
                            {
                                _instance.SendArrowKey(mappedKey, true);
                            }
                            else
                            {
                                _instance.SendRegularKey(mappedKey, true);
                            }
                        }
                    }
                    else if (isKeyUp)
                    {
                        if (_instance._pressedKeys.Contains(vkCode))
                        {
                            _instance._pressedKeys.Remove(vkCode);
                            
                            if (mappedKey >= 0x25 && mappedKey <= 0x28)
                            {
                                _instance.SendArrowKey(mappedKey, false);
                            }
                            else
                            {
                                _instance.SendRegularKey(mappedKey, false);
                            }
                        }
                    }
                }
                
                return new IntPtr(1);
            }

            return CallNextHookEx(_instance._hookID, nCode, wParam, lParam);
        }

        private void SendArrowKey(int vkCode, bool isKeyDown)
        {
            uint scanCode = vkCode switch
            {
                0x25 => LEFT_ARROW_SCAN,
                0x26 => UP_ARROW_SCAN,
                0x27 => RIGHT_ARROW_SCAN,
                0x28 => DOWN_ARROW_SCAN,
                _ => 0
            };

            if (scanCode == 0) return;

            try
            {
                IntPtr targetWindow = FindWindow(null!, "Hearts of Iron IV");
                if (targetWindow == IntPtr.Zero)
                {
                    targetWindow = GetForegroundWindow();
                }

                if (targetWindow != IntPtr.Zero)
                {
                    uint message = isKeyDown ? (uint)WM_KEYDOWN : (uint)WM_KEYUP;
                    uint lParam;
                    
                    if (isKeyDown)
                    {
                        lParam = 1 | (scanCode << 16) | (1u << 24);
                    }
                    else
                    {
                        lParam = 1 | (scanCode << 16) | (1u << 24) | (1u << 30) | (1u << 31);
                    }
                    
                    SendMessage(targetWindow, message, new IntPtr(vkCode), new IntPtr((int)lParam));
                }
            }
            catch
            {
                SendRegularKey(vkCode, isKeyDown);
            }
        }

        private void SendArrowKeyDown(int vkCode) => SendArrowKey(vkCode, true);
        private void SendArrowKeyUp(int vkCode) => SendArrowKey(vkCode, false);

        private void SendRegularKey(int vkCode, bool isKeyDown)
        {
            try
            {
                uint scanCode = MapVirtualKey((uint)vkCode, 0);

                var input = new INPUT
                {
                    type = 1,
                    u = new InputUnion
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = (ushort)vkCode,
                            wScan = (ushort)scanCode,
                            dwFlags = isKeyDown ? 0u : 2u,
                            time = 0,
                            dwExtraInfo = (UIntPtr)KEYBOARDMANAGER_INJECTED_FLAG
                        }
                    }
                };

                uint result = SendInput(1, new INPUT[] { input }, Marshal.SizeOf(typeof(INPUT)));
                if (result == 0)
                {
                    LogDebug($"SendInput failed for vkCode: {vkCode}");
                }
            }
            catch (Exception ex)
            {
                LogDebug($"SendRegularKey error: {ex.Message}");
            }
        }

        public void Dispose()
        {
            UninstallHook();
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
