using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace WinKeysRemapper.UI.Services
{
    public class ApplicationMonitor : IDisposable
    {
        private System.Threading.Timer? _monitorTimer;
        private volatile bool _isTargetActive = false;
        private string? _targetApplication;

        // Windows API for process monitoring
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        public event Action<string>? TargetApplicationActivated;
        public event Action<string>? TargetApplicationDeactivated;

        public bool IsTargetActive => _isTargetActive;
        public string? TargetApplication => _targetApplication;

        public void StartMonitoring(string targetApplication, TimeSpan interval)
        {
            _targetApplication = targetApplication?.ToLowerInvariant();
            
            _monitorTimer?.Dispose();
            _monitorTimer = new System.Threading.Timer(MonitorForegroundApplication, null, 
                TimeSpan.Zero, interval);
        }

        public void StopMonitoring()
        {
            _monitorTimer?.Dispose();
            _monitorTimer = null;
        }

        private void MonitorForegroundApplication(object? state)
        {
            try
            {
                var hwnd = GetForegroundWindow();
                if (hwnd == IntPtr.Zero) return;

                GetWindowThreadProcessId(hwnd, out uint processId);
                var process = Process.GetProcessById((int)processId);
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
                // Ignore errors in background monitoring
                if (_isTargetActive)
                {
                    _isTargetActive = false;
                    TargetApplicationDeactivated?.Invoke("unknown");
                }
            }
        }

        public void Dispose()
        {
            _monitorTimer?.Dispose();
        }
    }
}
