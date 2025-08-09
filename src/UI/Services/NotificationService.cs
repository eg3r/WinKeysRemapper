using System;
using System.Windows.Forms;

namespace WinKeysRemapper.UI.Services
{
    public class NotificationService
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly bool _isStartupMode;

        public NotificationService(NotifyIcon notifyIcon, bool isStartupMode = false)
        {
            _notifyIcon = notifyIcon ?? throw new ArgumentNullException(nameof(notifyIcon));
            _isStartupMode = isStartupMode;
        }

        public void ShowNotification(string title, string text, ToolTipIcon icon)
        {
            if (_isStartupMode && icon == ToolTipIcon.Info)
            {
                return;
            }

            try
            {
                _notifyIcon.ShowBalloonTip(3000, title, text, icon);
            }
            catch
            {
            }
        }

        public void ShowHookActivated(string targetApplication)
        {
        }

        public void ShowHookDeactivated()
        {
        }

        public void ShowHookError(string message)
        {
            ShowNotification("Hook Error", message, ToolTipIcon.Error);
        }

        public void ShowConfigurationLoaded(string targetApplication, int successfulMappings, int totalMappings)
        {
            if (!_isStartupMode)
            {
                ShowNotification("WinKeysRemapper Started", 
                    $"Monitoring: {targetApplication}\n" +
                    $"Mappings: {successfulMappings}/{totalMappings} keys loaded\n" +
                    $"Hook will activate when target app is detected", 
                    ToolTipIcon.Info);
            }
        }

        public void ShowConfigurationError(string message)
        {
            ShowNotification("Configuration Error", message, ToolTipIcon.Error);
        }

        public void ShowConfigurationReloaded(string targetApplication, int successfulMappings, int totalMappings)
        {
            ShowNotification("Config Reloaded", 
                $"Monitoring: {targetApplication}\n" +
                $"Mappings: {successfulMappings}/{totalMappings} keys parsed successfully", 
                ToolTipIcon.Info);
        }

        public void ShowReloadError(string message)
        {
            ShowNotification("Reload Failed", message, ToolTipIcon.Error);
        }

        public void ShowConfigFileError(string message)
        {
            ShowNotification("Config File Error", message, ToolTipIcon.Error);
        }

        public void ShowStartupToggleError(string message)
        {
            ShowNotification("Startup Toggle Error", message, ToolTipIcon.Error);
        }
    }
}
