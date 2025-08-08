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
            reloadMenuItem.Image = CreateMenuIcon("🔄"); // Refresh icon
            
            var openConfigMenuItem = new ToolStripMenuItem("Open Config");
            openConfigMenuItem.Click += OnOpenConfig;
            openConfigMenuItem.Image = CreateMenuIcon("📝"); // Edit/document icon
            
            var exitMenuItem = new ToolStripMenuItem("Exit");
            exitMenuItem.Click += OnExit;
            exitMenuItem.Image = CreateMenuIcon("❌"); // Exit icon
            
            _contextMenu.Items.Add(reloadMenuItem);
            _contextMenu.Items.Add(openConfigMenuItem);
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
            // Create a modern-looking keyboard icon
            var bitmap = new Bitmap(32, 32);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                
                // Create a modern keyboard icon with gradient
                var rect = new Rectangle(4, 8, 24, 16);
                var keyboardBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    rect, Color.FromArgb(70, 130, 180), Color.FromArgb(25, 25, 112), 45f);
                
                // Main keyboard body
                graphics.FillRoundedRectangle(keyboardBrush, rect, 3);
                graphics.DrawRoundedRectangle(new Pen(Color.FromArgb(50, 50, 50), 1), rect, 3);
                
                // Draw some key indicators
                var keyBrush = new SolidBrush(Color.FromArgb(220, 220, 220));
                graphics.FillRectangle(keyBrush, 7, 11, 3, 2);  // Key 1
                graphics.FillRectangle(keyBrush, 11, 11, 3, 2); // Key 2
                graphics.FillRectangle(keyBrush, 15, 11, 3, 2); // Key 3
                graphics.FillRectangle(keyBrush, 19, 11, 3, 2); // Key 4
                
                graphics.FillRectangle(keyBrush, 7, 14, 4, 2);  // Space key simulation
                graphics.FillRectangle(keyBrush, 12, 14, 4, 2); // Space key simulation
                graphics.FillRectangle(keyBrush, 17, 14, 4, 2); // Space key simulation
                
                // Add a small "remap" indicator arrow
                var arrowPen = new Pen(Color.FromArgb(255, 215, 0), 2);
                graphics.DrawLine(arrowPen, 24, 4, 28, 4);
                graphics.DrawLine(arrowPen, 26, 2, 28, 4);
                graphics.DrawLine(arrowPen, 26, 6, 28, 4);
                
                keyboardBrush.Dispose();
                keyBrush.Dispose();
                arrowPen.Dispose();
            }
            
            return Icon.FromHandle(bitmap.GetHicon());
        }

        private Bitmap CreateMenuIcon(string emoji)
        {
            var bitmap = new Bitmap(16, 16);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                
                // Try to use emoji first, fallback to simple graphics if needed
                try
                {
                    using (var font = new Font("Segoe UI Emoji", 10, FontStyle.Regular))
                    {
                        var textSize = graphics.MeasureString(emoji, font);
                        var x = (16 - textSize.Width) / 2;
                        var y = (16 - textSize.Height) / 2;
                        graphics.DrawString(emoji, font, Brushes.Black, x, y);
                    }
                }
                catch
                {
                    // Fallback to simple geometric shapes
                    switch (emoji)
                    {
                        case "🔄": // Reload
                            graphics.DrawEllipse(new Pen(Color.Blue, 2), 2, 2, 12, 12);
                            graphics.DrawLine(new Pen(Color.Blue, 2), 8, 2, 10, 4);
                            graphics.DrawLine(new Pen(Color.Blue, 2), 10, 4, 8, 6);
                            break;
                        case "📝": // Edit
                            graphics.FillRectangle(Brushes.White, 3, 2, 8, 11);
                            graphics.DrawRectangle(Pens.Black, 3, 2, 8, 11);
                            graphics.DrawLine(new Pen(Color.Blue, 1), 5, 5, 9, 5);
                            graphics.DrawLine(new Pen(Color.Blue, 1), 5, 7, 9, 7);
                            graphics.DrawLine(new Pen(Color.Blue, 1), 5, 9, 7, 9);
                            break;
                        case "❌": // Exit
                            graphics.DrawLine(new Pen(Color.Red, 2), 4, 4, 12, 12);
                            graphics.DrawLine(new Pen(Color.Red, 2), 12, 4, 4, 12);
                            break;
                    }
                }
            }
            return bitmap;
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

        private void OnOpenConfig(object? sender, EventArgs e)
        {
            try
            {
                var configPath = _configManager.GetConfigPath();
                
                if (System.IO.File.Exists(configPath))
                {
                    // Use Windows shell to open the file with the default application
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = configPath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    ShowBalloonTip("Config Not Found", 
                        "Configuration file not found. It will be created when the application starts.", 
                        ToolTipIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                ShowBalloonTip("Open Failed", 
                    $"Failed to open config file: {ex.Message}", 
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

    // Extension methods for modern graphics
    public static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle rectangle, int radius)
        {
            using (var path = CreateRoundedRectanglePath(rectangle, radius))
            {
                graphics.FillPath(brush, path);
            }
        }

        public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle rectangle, int radius)
        {
            using (var path = CreateRoundedRectanglePath(rectangle, radius))
            {
                graphics.DrawPath(pen, path);
            }
        }

        private static System.Drawing.Drawing2D.GraphicsPath CreateRoundedRectanglePath(Rectangle rectangle, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            int diameter = radius * 2;

            path.AddArc(rectangle.X, rectangle.Y, diameter, diameter, 180, 90);
            path.AddArc(rectangle.Right - diameter, rectangle.Y, diameter, diameter, 270, 90);
            path.AddArc(rectangle.Right - diameter, rectangle.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rectangle.X, rectangle.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
