using System;
using System.Collections.Generic;
using System.Windows.Forms;
using WinKeysRemapper.Input;

namespace WinKeysRemapper.UI.Services
{
    public class HookManager : IDisposable
    {
        private LowLevelKeyboardHook? _keyboardHook;
        private readonly Control _invokeControl;

        public event Action<string>? HookActivated;
        public event Action? HookDeactivated;
        public event Action<string>? HookError;

        public bool IsHookActive => _keyboardHook != null;

        public HookManager(Control invokeControl)
        {
            _invokeControl = invokeControl ?? throw new ArgumentNullException(nameof(invokeControl));
        }

        public void CreateHook(Dictionary<int, int> keyMappings, string targetApplication)
        {
            if (_keyboardHook != null || keyMappings == null || string.IsNullOrEmpty(targetApplication))
            {
                return;
            }

            try
            {
                // Ensure hook creation happens on UI thread
                if (_invokeControl.InvokeRequired)
                {
                    _invokeControl.Invoke(() => CreateHookDirect(keyMappings, targetApplication));
                }
                else
                {
                    CreateHookDirect(keyMappings, targetApplication);
                }
            }
            catch (Exception ex)
            {
                HookError?.Invoke($"Failed to create keyboard hook: {ex.Message}");
            }
        }

        private void CreateHookDirect(Dictionary<int, int> keyMappings, string targetApplication)
        {
            try
            {
                _keyboardHook = new LowLevelKeyboardHook(keyMappings, targetApplication);
                _keyboardHook.InstallHook();
                
                HookActivated?.Invoke(targetApplication);
            }
            catch (Exception ex)
            {
                _keyboardHook?.Dispose();
                _keyboardHook = null;
                HookError?.Invoke($"Failed to install keyboard hook: {ex.Message}");
            }
        }

        public void DestroyHook()
        {
            if (_keyboardHook == null) return;

            try
            {
                _keyboardHook.Dispose();
                _keyboardHook = null;
                HookDeactivated?.Invoke();
            }
            catch (Exception ex)
            {
                HookError?.Invoke($"Failed to cleanup keyboard hook: {ex.Message}");
            }
        }

        public void Dispose()
        {
            DestroyHook();
        }
    }
}
