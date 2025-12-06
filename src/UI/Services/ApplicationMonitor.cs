using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinKeysRemapper.UI.Services
{
    public class ApplicationMonitor : IDisposable
    {
        private IntPtr _hookHandle = IntPtr.Zero;
        private WinEventDelegate? _winEventDelegate;
        private volatile bool _isTargetActive = false;
        private string? _targetApplication;

        // Windows API
        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 0x0003;

        private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        public event Action<string>? TargetApplicationActivated;
        public event Action<string>? TargetApplicationDeactivated;

        public bool IsTargetActive => _isTargetActive;
        public string? TargetApplication => _targetApplication;

        public void StartMonitoring(string targetApplication)
        {
            _targetApplication = targetApplication?.ToLowerInvariant();
            
            StopMonitoring();

            _winEventDelegate = new WinEventDelegate(WinEventProc);
            _hookHandle = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _winEventDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);

            // Check immediately once
            CheckForegroundWindow(GetForegroundWindow());
        }

        public void StopMonitoring()
        {
            if (_hookHandle != IntPtr.Zero)
            {
                UnhookWinEvent(_hookHandle);
                _hookHandle = IntPtr.Zero;
            }
            _winEventDelegate = null;
        }

        private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (eventType == EVENT_SYSTEM_FOREGROUND)
            {
                CheckForegroundWindow(hwnd);
            }
        }

        private void CheckForegroundWindow(IntPtr hwnd)
        {
            try
            {
                if (hwnd == IntPtr.Zero) return;

                GetWindowThreadProcessId(hwnd, out uint processId);
                using var process = Process.GetProcessById((int)processId);
                var processName = process.ProcessName.ToLowerInvariant();

                bool isCurrentlyActive = !string.IsNullOrEmpty(_targetApplication) && 
                                       processName.Contains(_targetApplication);

                // Check for state changes
                if (isCurrentlyActive && !_isTargetActive)
                {
                    _isTargetActive = true;
                    TargetApplicationActivated?.Invoke(processName);
                }
                else if (!isCurrentlyActive && _isTargetActive)
                {
                    _isTargetActive = false;
                    TargetApplicationDeactivated?.Invoke(processName);
                }
            }
            catch
            {
                // Ignore errors (e.g. process accessing issues)
                if (_isTargetActive)
                {
                    _isTargetActive = false;
                    TargetApplicationDeactivated?.Invoke("unknown");
                }
            }
        }

        public void Dispose()
        {
            StopMonitoring();
        }
    }
}
