using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        
        // Flag to prevent infinite loops - same as PowerToys uses
        private const uint KEYBOARDMANAGER_INJECTED_FLAG = 0xFFFFFFF0;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        
        private readonly LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;
        private static LowLevelKeyboardHook? _instance;
        
        // Configuration
        private readonly Dictionary<int, int> _keyMappings;
        private readonly HashSet<string> _targetApplications;
        
        // Process checking optimization - background thread updates
        private volatile bool _isTargetApplication = false;
        private volatile bool _loadingSettings = false;
        private readonly Timer _processUpdateTimer;
        private readonly object _processLock = new object();

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

        public LowLevelKeyboardHook(Dictionary<int, int> keyMappings, HashSet<string> targetApplications)
        {
            _proc = HookCallback;
            _keyMappings = keyMappings ?? new Dictionary<int, int>();
            _targetApplications = targetApplications ?? new HashSet<string>();

            // Start background process checking timer - updates every 2 seconds instead of every keystroke
            _processUpdateTimer = new Timer(UpdateTargetApplicationStatus, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
        }

        public static LowLevelKeyboardHook CreateInstance(Dictionary<int, int> keyMappings, HashSet<string> targetApplications)
        {
            _instance?.Dispose();
            _instance = new LowLevelKeyboardHook(keyMappings, targetApplications);
            return _instance;
        }

        private void UpdateTargetApplicationStatus(object? state)
        {
            try
            {
                lock (_processLock)
                {
                    var hwnd = GetForegroundWindow();
                    if (hwnd == IntPtr.Zero)
                    {
                        _isTargetApplication = false;
                        return;
                    }

                    GetWindowThreadProcessId(hwnd, out uint processId);
                    var process = Process.GetProcessById((int)processId);
                    var processName = process.ProcessName.ToLowerInvariant();
                    
                    _isTargetApplication = _targetApplications.Any(app => 
                        processName.Contains(app.ToLowerInvariant()));
                }
            }
            catch
            {
                // Ignore errors in background thread
                _isTargetApplication = false;
            }
        }

        public void InstallHook()
        {
            if (_hookID == IntPtr.Zero)
            {
                _hookID = SetHook(_proc);
                if (_hookID == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Failed to install keyboard hook");
                }
            }
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule!)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            // PowerToys pattern: Early returns for performance
            if (nCode != HC_ACTION)
            {
                return CallNextHookEx(_instance?._hookID ?? IntPtr.Zero, nCode, wParam, lParam);
            }

            if (_instance == null)
            {
                return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
            }

            // Early return if loading settings (PowerToys pattern)
            if (_instance._loadingSettings)
            {
                return CallNextHookEx(_instance._hookID, nCode, wParam, lParam);
            }

            // Parse the keyboard event
            var hookStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);

            // Check if this is our own injected event to prevent infinite loops
            if (hookStruct.dwExtraInfo == (UIntPtr)KEYBOARDMANAGER_INJECTED_FLAG)
            {
                // Let our own events pass through to the system
                return CallNextHookEx(_instance._hookID, nCode, wParam, lParam);
            }

            // Early return if not a target application (checked by background thread)
            if (!_instance._isTargetApplication)
            {
                return CallNextHookEx(_instance._hookID, nCode, wParam, lParam);
            }

            // Only process key down events for remapping
            if ((int)wParam != WM_KEYDOWN && (int)wParam != WM_SYSKEYDOWN)
            {
                return CallNextHookEx(_instance._hookID, nCode, wParam, lParam);
            }

            // Check if this key should be remapped
            var vkCode = (int)hookStruct.vkCode;
            if (_instance._keyMappings.TryGetValue(vkCode, out int mappedKey))
            {
                // Send the mapped key
                _instance.SendKey(mappedKey);
                
                // Suppress the original key
                return new IntPtr(1);
            }

            return CallNextHookEx(_instance._hookID, nCode, wParam, lParam);
        }

        private void SendKey(int vkCode)
        {
            try
            {
                var inputs = new INPUT[2];

                // Key down
                inputs[0] = new INPUT
                {
                    type = 1, // INPUT_KEYBOARD
                    u = new InputUnion
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = (ushort)vkCode,
                            wScan = 0,
                            dwFlags = 0,
                            time = 0,
                            dwExtraInfo = (UIntPtr)KEYBOARDMANAGER_INJECTED_FLAG
                        }
                    }
                };

                // Key up
                inputs[1] = new INPUT
                {
                    type = 1, // INPUT_KEYBOARD
                    u = new InputUnion
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = (ushort)vkCode,
                            wScan = 0,
                            dwFlags = 2, // KEYEVENTF_KEYUP
                            time = 0,
                            dwExtraInfo = (UIntPtr)KEYBOARDMANAGER_INJECTED_FLAG
                        }
                    }
                };

                SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
            }
            catch
            {
                // Silently ignore errors
            }
        }

        public void Dispose()
        {
            _processUpdateTimer?.Dispose();
            UninstallHook();
        }

        public void UninstallHook()
        {
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }
        }
    }
}
