using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinKeysRemapper.Configuration;
using WinKeysRemapper.Input;
using WinKeysRemapper.UI.Services;

namespace WinKeysRemapper.UI
{
    public class TrayManager : Form
    {
        private readonly ConfigurationManager _configManager;
        private readonly ApplicationMonitor _applicationMonitor;
        private readonly HookManager _hookManager;
        private readonly TrayIconManager _trayIconManager;
        private readonly NotificationService _notificationService;
        private readonly bool _isStartupMode;

        private KeyMappingConfig? _config;
        private Dictionary<int, int>? _keyMappings;
        private string? _targetApplication;

        public TrayManager(bool isStartupMode = false)
        {
            // Initialize as a hidden form for proper Windows Forms context
            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
            Visible = false;
            
            // Force handle creation so InvokeRequired works properly
            var handle = this.Handle;
            
            _isStartupMode = isStartupMode;
            _configManager = new ConfigurationManager();
            
            var startupManager = new StartupManager();
            
            // Initialize services
            _applicationMonitor = new ApplicationMonitor();
            _hookManager = new HookManager(this);
            _trayIconManager = new TrayIconManager(startupManager);
            
            // Initialize tray icon first to get NotifyIcon for notifications
            _trayIconManager.Initialize();
            _notificationService = new NotificationService(_trayIconManager.NotifyIcon, _isStartupMode);
            
            // Wire up events
            WireUpEvents();
            
            LoadConfigAndStartMonitoring();
        }

        private void WireUpEvents()
        {
            // Application monitor events
            _applicationMonitor.TargetApplicationActivated += OnTargetApplicationActivated;
            _applicationMonitor.TargetApplicationDeactivated += OnTargetApplicationDeactivated;
            
            // Hook manager events
            _hookManager.HookActivated += OnHookActivated;
            _hookManager.HookDeactivated += OnHookDeactivated;
            _hookManager.HookError += OnHookError;
            
            // Tray icon events
            _trayIconManager.ReloadConfigRequested += OnReloadConfig;
            _trayIconManager.OpenConfigRequested += OnOpenConfig;
            _trayIconManager.ToggleStartupRequested += OnToggleStartup;
            _trayIconManager.ExitRequested += OnExit;
        }

        private void OnTargetApplicationActivated(string processName)
        {
            if (_keyMappings != null && _targetApplication != null)
            {
                _hookManager.CreateHook(_keyMappings, _targetApplication);
            }
        }

        private void OnTargetApplicationDeactivated(string processName)
        {
            _hookManager.DestroyHook();
        }

        private void OnHookActivated(string targetApplication)
        {
            _notificationService.ShowHookActivated(targetApplication);
        }

        private void OnHookDeactivated()
        {
            _notificationService.ShowHookDeactivated();
        }

        private void OnHookError(string message)
        {
            _notificationService.ShowHookError(message);
        }

        private void LoadConfigAndStartMonitoring()
        {
            try
            {
                _config = _configManager.LoadConfig();
                
                // Parse key mappings once and store in memory
                _keyMappings = new Dictionary<int, int>();
                _targetApplication = _config.TargetApplication;
                
                var successfulMappings = 0;
                foreach (var mapping in _config.KeyMappings)
                {
                    if (VirtualKeyParser.TryParseVirtualKey(mapping.Key, out int fromKey) && 
                        VirtualKeyParser.TryParseVirtualKey(mapping.Value, out int toKey))
                    {
                        _keyMappings[fromKey] = toKey;
                        successfulMappings++;
                    }
                }
                
                // Start application monitoring
                _applicationMonitor.StartMonitoring(_targetApplication, TimeSpan.FromSeconds(2));
                
                _notificationService.ShowConfigurationLoaded(_targetApplication, successfulMappings, _config.KeyMappings.Count);
            }
            catch (Exception ex)
            {
                _notificationService.ShowConfigurationError($"Failed to load config: {ex.Message}");
            }
        }
        
        private void ReloadConfig()
        {
            try
            {
                _config = _configManager.LoadConfig();
                
                var newKeyMappings = new Dictionary<int, int>();
                var newTargetApplication = _config.TargetApplication;
                
                var successfulMappings = 0;
                foreach (var mapping in _config.KeyMappings)
                {
                    if (VirtualKeyParser.TryParseVirtualKey(mapping.Key, out int fromKey) && 
                        VirtualKeyParser.TryParseVirtualKey(mapping.Value, out int toKey))
                    {
                        newKeyMappings[fromKey] = toKey;
                        successfulMappings++;
                    }
                }
                
                // Update stored configuration
                _keyMappings = newKeyMappings;
                _targetApplication = newTargetApplication;
                
                // If target app changed, restart monitoring
                _hookManager.DestroyHook();
                _applicationMonitor.StartMonitoring(_targetApplication, TimeSpan.FromSeconds(2));
                
                _notificationService.ShowConfigurationReloaded(_targetApplication, successfulMappings, _config.KeyMappings.Count);
            }
            catch (Exception ex)
            {
                _notificationService.ShowReloadError($"Failed to reload config: {ex.Message}");
            }
        }

        private void OnReloadConfig(object? sender, EventArgs e)
        {
            try
            {
                ReloadConfig();
            }
            catch (Exception ex)
            {
                _notificationService.ShowReloadError($"Failed to reload config: {ex.Message}");
            }
        }

        private void OnOpenConfig(object? sender, EventArgs e)
        {
            try
            {
                var configPath = _configManager.GetConfigPath();
                
                if (File.Exists(configPath))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = configPath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    _notificationService.ShowConfigFileError("Config file not found.");
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowConfigFileError($"Failed to open config file: {ex.Message}");
            }
        }

        private async void OnToggleStartup(object? sender, EventArgs e)
        {
            try
            {
                // Run the startup toggle operation on a background thread
                await Task.Run(() =>
                {
                    var startupManager = new StartupManager();
                    if (startupManager.IsStartupEnabled())
                    {
                        startupManager.DisableStartup();
                    }
                    else
                    {
                        startupManager.EnableStartup();
                    }
                });

                // No need to update checkbox - it was already toggled for immediate feedback
            }
            catch (Exception ex)
            {
                // If operation failed, revert the checkbox state
                _trayIconManager.UpdateStartupCheckboxState();
                _notificationService.ShowStartupToggleError($"Failed to toggle startup: {ex.Message}");
            }
        }

        private void OnExit(object? sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _applicationMonitor?.Dispose();
                _hookManager?.Dispose();
                _trayIconManager?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
