using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WinKeysRemapper.Configuration;

namespace WinKeysRemapper.UI.Services
{
    public class TrayIconManager : IDisposable
    {
        private NotifyIcon? _notifyIcon;
        private ContextMenuStrip? _contextMenu;
        private ToolStripMenuItem? _startupMenuItem;
        private readonly StartupManager _startupManager;

        public NotifyIcon NotifyIcon => _notifyIcon ?? throw new InvalidOperationException("Tray icon not initialized");

        public event EventHandler? ReloadConfigRequested;
        public event EventHandler? OpenConfigRequested;
        public event EventHandler? ToggleStartupRequested;
        public event EventHandler? ExitRequested;

        public TrayIconManager(StartupManager startupManager)
        {
            _startupManager = startupManager ?? throw new ArgumentNullException(nameof(startupManager));
        }

        public void Initialize()
        {
            CreateContextMenu();
            CreateNotifyIcon();
        }

        private void CreateContextMenu()
        {
            _contextMenu = new ContextMenuStrip();

            var reloadMenuItem = new ToolStripMenuItem("Reload Config");
            reloadMenuItem.Click += (s, e) => ReloadConfigRequested?.Invoke(s, e);
            reloadMenuItem.Image = CreateMenuIcon("🔄");

            var openConfigMenuItem = new ToolStripMenuItem("Open Config");
            openConfigMenuItem.Click += (s, e) => OpenConfigRequested?.Invoke(s, e);
            openConfigMenuItem.Image = CreateMenuIcon("📝");

            var startupMenuItem = new ToolStripMenuItem("Start with Windows");
            startupMenuItem.CheckOnClick = false; // Disable automatic toggling
            startupMenuItem.Checked = _startupManager.IsStartupEnabled();
            startupMenuItem.Image = CreateMenuIcon("🚀");
            startupMenuItem.Click += (s, e) =>
            {
                // Immediately toggle the checkbox for visual feedback
                startupMenuItem.Checked = !startupMenuItem.Checked;
                ToggleStartupRequested?.Invoke(s, e);
            };

            // Store reference for later updates
            _startupMenuItem = startupMenuItem;

            var exitMenuItem = new ToolStripMenuItem("Exit");
            exitMenuItem.Click += (s, e) => ExitRequested?.Invoke(s, e);
            exitMenuItem.Image = CreateMenuIcon("❌");

            _contextMenu.Items.AddRange(new ToolStripItem[] {
                reloadMenuItem,
                openConfigMenuItem,
                new ToolStripSeparator(),
                startupMenuItem,
                new ToolStripSeparator(),
                exitMenuItem
            });

            // Prevent menu from closing when startup checkbox is clicked
            _contextMenu.Closing += (s, e) =>
            {
                if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
                {
                    var clickedItem = _contextMenu.GetItemAt(_contextMenu.PointToClient(Cursor.Position));
                    if (clickedItem == startupMenuItem)
                    {
                        e.Cancel = true;
                    }
                }
            };
        }

        private void CreateNotifyIcon()
        {
            _notifyIcon = new NotifyIcon()
            {
                Icon = CreateMainIcon(),
                ContextMenuStrip = _contextMenu,
                Text = "WinKeysRemapper",
                Visible = true
            };
        }

        private Icon CreateMainIcon()
        {
            // Try some common keyboard/input-related icon indexes

            try
            {
                IntPtr hIcon = ExtractIcon(IntPtr.Zero, "shell32.dll", 96);
                if (hIcon != IntPtr.Zero)
                {
                    var icon = Icon.FromHandle(hIcon);
                    return new Icon(icon, 16, 16);
                }
            }
            catch
            {
            }

            // Fallback: Create custom "KM" icon
            var bitmap = new Bitmap(16, 16);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // Draw rounded background box
                using (var backgroundBrush = new SolidBrush(Color.FromArgb(30, 144, 255))) // Dodger blue
                using (var outlinePen = new Pen(Color.FromArgb(0, 100, 200), 1)) // Darker blue outline
                {
                    var boxRect = new Rectangle(1, 1, 14, 14);
                    graphics.FillRoundedRectangle(backgroundBrush, boxRect, 3);
                    graphics.DrawRoundedRectangle(outlinePen, boxRect, 3);
                }

                // Draw "KM" text
                using var font = new Font("Arial", 7, FontStyle.Bold);
                using var textBrush = new SolidBrush(Color.White);
                var text = "KM";
                var textSize = graphics.MeasureString(text, font);
                var x = (16 - textSize.Width) / 2;
                var y = (16 - textSize.Height) / 2;

                graphics.DrawString(text, font, textBrush, x, y);
            }
            return Icon.FromHandle(bitmap.GetHicon());
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);

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
                    DrawFallbackIcon(graphics, emoji);
                }
            }
            return bitmap;
        }

        private void DrawFallbackIcon(Graphics graphics, string emoji)
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

        public void UpdateStartupCheckboxState()
        {
            if (_startupMenuItem != null)
            {
                // Simply toggle the current state since we know the operation succeeded
                _startupMenuItem.Checked = !_startupMenuItem.Checked;
            }
        }

        public void Dispose()
        {
            _notifyIcon?.Dispose();
            _contextMenu?.Dispose();
        }
    }
}
