using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WinKeysRemapper.Configuration;
using WinKeysRemapper.Input;

namespace WinKeysRemapper.UI
{
    public class TrayManager : IDisposable
    {
        private NotifyIcon? _notifyIcon;
        private ContextMenuStrip? _contextMenu;
        private ConfigurationManager _configManager;
        private LowLevelKeyboardHook? _keyboardHook;
        private KeyMappingConfig? _config;

        public TrayManager()
        {
            _configManager = new ConfigurationManager();
            InitializeTrayIcon();
            LoadConfigAndStartHook();
        }

        private void InitializeTrayIcon()
        {
            // Create context menu
            _contextMenu = new ContextMenuStrip();
            
            var reloadMenuItem = new ToolStripMenuItem("Reload Config");
            reloadMenuItem.Click += OnReloadConfig;
            
            var exitMenuItem = new ToolStripMenuItem("Exit");
            exitMenuItem.Click += OnExit;
            
            _contextMenu.Items.Add(reloadMenuItem);
            _contextMenu.Items.Add(new ToolStripSeparator());
            _contextMenu.Items.Add(exitMenuItem);

            // Create notify icon
            _notifyIcon = new NotifyIcon
            {
                Icon = CreateIcon(),
                Text = "WinKeysRemapper",
                ContextMenuStrip = _contextMenu,
                Visible = true
            };

            // Double-click to show status
            _notifyIcon.DoubleClick += OnDoubleClick;
        }

        private Icon CreateIcon()
        {
            // Create a simple icon programmatically
            var bitmap = new Bitmap(16, 16);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);
                graphics.FillEllipse(Brushes.Blue, 2, 2, 12, 12);
                graphics.DrawString("K", new Font("Arial", 8, FontStyle.Bold), Brushes.White, 3, 1);
            }
            
            return Icon.FromHandle(bitmap.GetHicon());
        }

        private void LoadConfigAndStartHook()
        {
            try
            {
                _config = _configManager.LoadConfig();
                
                // Stop existing hook if any
                _keyboardHook?.Dispose();
                
                // Convert configuration to the format expected by the hook
                var keyMappings = new Dictionary<int, int>();
                var targetApplications = new HashSet<string> { _config.TargetApplication };
                
                // Convert string key names to virtual key codes
                var successfulMappings = 0;
                foreach (var mapping in _config.KeyMappings)
                {
                    if (VirtualKeyParser.TryParseVirtualKey(mapping.Key, out int fromKey) && 
                        VirtualKeyParser.TryParseVirtualKey(mapping.Value, out int toKey))
                    {
                        keyMappings[fromKey] = toKey;
                        successfulMappings++;
                    }
                }
                
                // Create and start new hook
                _keyboardHook = LowLevelKeyboardHook.CreateInstance(keyMappings, targetApplications);
                _keyboardHook.InstallHook();
                
                ShowBalloonTip("WinKeysRemapper Started", 
                    $"Monitoring: {_config.TargetApplication}\n" +
                    $"Mappings: {successfulMappings}/{_config.KeyMappings.Count} keys parsed successfully", 
                    ToolTipIcon.Info);
            }
            catch (Exception ex)
            {
                ShowBalloonTip("Configuration Error", 
                    $"Failed to load config: {ex.Message}", 
                    ToolTipIcon.Error);
            }
        }

        private void OnReloadConfig(object? sender, EventArgs e)
        {
            try
            {
                LoadConfigAndStartHook();
                // ShowBalloonTip("Config Reloaded", 
                //     "Configuration has been successfully reloaded.", 
                //     ToolTipIcon.Info);
            }
            catch (Exception ex)
            {
                ShowBalloonTip("Reload Failed", 
                    $"Failed to reload config: {ex.Message}", 
                    ToolTipIcon.Error);
            }
        }

        private void OnExit(object? sender, EventArgs e)
        {
            Application.Exit();
        }

        private void OnDoubleClick(object? sender, EventArgs e)
        {
            var status = _config != null 
                ? $"Status: Running\nTarget: {_config.TargetApplication}\nMappings: {_config.KeyMappings.Count}"
                : "Status: Configuration not loaded";
                
            ShowBalloonTip("WinKeysRemapper Status", status, ToolTipIcon.Info);
        }

        private void ShowBalloonTip(string title, string text, ToolTipIcon icon)
        {
            _notifyIcon?.ShowBalloonTip(3000, title, text, icon);
        }

        public void Dispose()
        {
            _keyboardHook?.Dispose();
            _notifyIcon?.Dispose();
            _contextMenu?.Dispose();
        }
    }
}
