using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Reflection;
using WinKeysRemapper.Configuration;
using WinKeysRemapper.Input;

namespace WinKeysRemapper.UI
{
    public class TrayManager : IDisposable
    {
        private NotifyIcon? _notifyIcon;
        private ContextMenuStrip? _contextMenu;
        private ConfigurationManager _configManager;
        private StartupManager _startupManager;
        private LowLevelKeyboardHook? _keyboardHook;
        private KeyMappingConfig? _config;
        private readonly bool _isStartupMode;

        public TrayManager(bool isStartupMode = false)
        {
            _isStartupMode = isStartupMode;
            _configManager = new ConfigurationManager();
            _startupManager = new StartupManager();
            InitializeTrayIcon();
            LoadConfigAndStartHook();
        }

        private void InitializeTrayIcon()
        {
            _contextMenu = new ContextMenuStrip();
            
            var reloadMenuItem = new ToolStripMenuItem("Reload Config");
            reloadMenuItem.Click += OnReloadConfig;
            reloadMenuItem.Image = CreateMenuIcon("🔄");
            
            var openConfigMenuItem = new ToolStripMenuItem("Open Config");
            openConfigMenuItem.Click += OnOpenConfig;
            openConfigMenuItem.Image = CreateMenuIcon("📝");
            
            var startupMenuItem = new ToolStripMenuItem("Start with Windows");
            startupMenuItem.Click += OnToggleStartup;
            startupMenuItem.CheckOnClick = true;
            startupMenuItem.Checked = _startupManager.IsStartupEnabled();
            startupMenuItem.Image = CreateMenuIcon("🚀");
            
            var exitMenuItem = new ToolStripMenuItem("Exit");
            exitMenuItem.Click += OnExit;
            exitMenuItem.Image = CreateMenuIcon("❌");
            
            _contextMenu.Items.Add(reloadMenuItem);
            _contextMenu.Items.Add(openConfigMenuItem);
            _contextMenu.Items.Add(startupMenuItem);
            _contextMenu.Items.Add(new ToolStripSeparator());
            _contextMenu.Items.Add(exitMenuItem);

            _notifyIcon = new NotifyIcon
            {
                Icon = CreateIcon(),
                Text = "WinKeysRemapper",
                ContextMenuStrip = _contextMenu,
                Visible = true
            };

            _notifyIcon.DoubleClick += OnDoubleClick;
        }

        private Icon CreateIcon()
        {
            var bitmap = new Bitmap(32, 32);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                
                var rect = new Rectangle(4, 8, 24, 16);
                var keyboardBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    rect, Color.FromArgb(70, 130, 180), Color.FromArgb(25, 25, 112), 45f);
                
                graphics.FillRoundedRectangle(keyboardBrush, rect, 3);
                graphics.DrawRoundedRectangle(new Pen(Color.FromArgb(50, 50, 50), 1), rect, 3);
                
                var keyBrush = new SolidBrush(Color.FromArgb(220, 220, 220));
                graphics.FillRectangle(keyBrush, 7, 11, 3, 2);
                graphics.FillRectangle(keyBrush, 11, 11, 3, 2);
                graphics.FillRectangle(keyBrush, 15, 11, 3, 2);
                graphics.FillRectangle(keyBrush, 19, 11, 3, 2);
                
                graphics.FillRectangle(keyBrush, 7, 14, 4, 2);
                graphics.FillRectangle(keyBrush, 12, 14, 4, 2);
                graphics.FillRectangle(keyBrush, 17, 14, 4, 2);
                
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
                    switch (emoji)
                    {
                        case "🔄":
                            graphics.DrawEllipse(new Pen(Color.Blue, 2), 2, 2, 12, 12);
                            graphics.DrawLine(new Pen(Color.Blue, 2), 8, 2, 10, 4);
                            graphics.DrawLine(new Pen(Color.Blue, 2), 10, 4, 8, 6);
                            break;
                        case "📝":
                            graphics.FillRectangle(Brushes.White, 3, 2, 8, 11);
                            graphics.DrawRectangle(Pens.Black, 3, 2, 8, 11);
                            graphics.DrawLine(new Pen(Color.Blue, 1), 5, 5, 9, 5);
                            graphics.DrawLine(new Pen(Color.Blue, 1), 5, 7, 9, 7);
                            graphics.DrawLine(new Pen(Color.Blue, 1), 5, 9, 7, 9);
                            break;
                        case "❌":
                            graphics.DrawLine(new Pen(Color.Red, 2), 4, 4, 12, 12);
                            graphics.DrawLine(new Pen(Color.Red, 2), 12, 4, 4, 12);
                            break;
                        case "🚀":
                            var points = new Point[] {
                                new Point(8, 2), new Point(10, 6), new Point(9, 10),
                                new Point(8, 12), new Point(7, 10), new Point(6, 6)
                            };
                            graphics.FillPolygon(Brushes.Orange, points);
                            graphics.DrawPolygon(new Pen(Color.DarkOrange, 1), points);
                            graphics.FillEllipse(Brushes.Red, 7, 12, 2, 3);
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
                
                _keyboardHook?.Dispose();
                
                var keyMappings = new Dictionary<int, int>();
                var targetApplications = new HashSet<string> { _config.TargetApplication };
                
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
                
                _keyboardHook = LowLevelKeyboardHook.CreateInstance(keyMappings, targetApplications);
                _keyboardHook.InstallHook();
                
                if (!_isStartupMode)
                {
                    ShowBalloonTip("WinKeysRemapper Started", 
                        $"Monitoring: {_config.TargetApplication}\n" +
                        $"Mappings: {successfulMappings}/{_config.KeyMappings.Count} keys parsed successfully", 
                        ToolTipIcon.Info);
                }
            }
            catch (Exception ex)
            {
                if (_isStartupMode)
                {
                    ShowBalloonTip("WinKeysRemapper Error", 
                        "Failed to load configuration", 
                        ToolTipIcon.Error);
                }
                else
                {
                    ShowBalloonTip("Configuration Error", 
                        $"Failed to load config: {ex.Message}", 
                        ToolTipIcon.Error);
                }
            }
        }

        private void OnReloadConfig(object? sender, EventArgs e)
        {
            try
            {
                LoadConfigAndStartHook();
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
                
                if (File.Exists(configPath))
                {
                    Process.Start(new ProcessStartInfo
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

        private void OnToggleStartup(object? sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            if (menuItem == null) return;

            try
            {
                if (menuItem.Checked)
                {
                    _startupManager.EnableStartup();
                }
                else
                {
                    _startupManager.DisableStartup();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to toggle startup: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                menuItem.Checked = !menuItem.Checked;
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
