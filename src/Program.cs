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
            // Enable visual styles for Windows Forms
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Check if already running (prevent multiple instances)
            using var mutex = new System.Threading.Mutex(true, "WinKeysRemapper_SingleInstance", out bool isNewInstance);
            
            if (!isNewInstance)
            {
                MessageBox.Show("WinKeysRemapper is already running in the system tray.", 
                    "Already Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Create and run the tray application
                using var trayManager = new TrayManager();
                
                // Run the Windows Forms message loop
                Application.Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting WinKeysRemapper: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
