using System;
using System.Threading;
using System.Windows.Forms;
using WinKeysRemapper.UI;

namespace WinKeysRemapper
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Check if started from Windows startup (silent mode)
            bool isStartupMode = args.Length > 0 && (args[0] == "--startup" || args[0] == "/startup");
            
            // Enable visual styles for Windows Forms
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Check if already running (prevent multiple instances)
            using var mutex = new System.Threading.Mutex(true, "WinKeysRemapper_SingleInstance", out bool isNewInstance);
            
            if (!isNewInstance)
            {
                if (!isStartupMode) // Only show message if not started from startup
                {
                    MessageBox.Show("WinKeysRemapper is already running in the system tray.", 
                        "Already Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return;
            }

            try
            {
                // Create and run the tray application with startup mode flag
                using var trayManager = new TrayManager(isStartupMode);
                
                // Run the Windows Forms message loop
                Application.Run();
            }
            catch (Exception ex)
            {
                if (!isStartupMode) // Only show error dialog if not started from startup
                {
                    MessageBox.Show($"Error starting WinKeysRemapper: {ex.Message}", 
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
