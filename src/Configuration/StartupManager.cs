using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace WinKeysRemapper.Configuration
{
    public class StartupManager
    {
        private const string RegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string RegistryValueName = "WinKeysRemapper";

        public bool IsStartupEnabled()
        {
            return IsRegistryEntryEnabled();
        }

        public void EnableStartup()
        {
            var executablePath = GetExecutablePath();
            CreateRegistryEntry(executablePath);
        }

        public void DisableStartup()
        {
            RemoveRegistryEntry();
        }

        private string GetExecutablePath()
        {
            // Try Environment.ProcessPath first (.NET 5+)
            var processPath = Environment.ProcessPath;
            if (!string.IsNullOrEmpty(processPath) && File.Exists(processPath))
            {
                return processPath;
            }

            // Fallback to MainModule.FileName
            try
            {
                var mainModulePath = Process.GetCurrentProcess().MainModule?.FileName;
                if (!string.IsNullOrEmpty(mainModulePath) && File.Exists(mainModulePath))
                {
                    return mainModulePath;
                }
            }
            catch { }

            // Final fallback
            var baseDir = AppContext.BaseDirectory;
            var exeName = Process.GetCurrentProcess().ProcessName + ".exe";
            return Path.Combine(baseDir, exeName);
        }

        private bool IsRegistryEntryEnabled()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, false);
                var value = key?.GetValue(RegistryValueName);
                return value != null && !string.IsNullOrEmpty(value.ToString());
            }
            catch
            {
                return false;
            }
        }

        private void CreateRegistryEntry(string executablePath)
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, true);
            if (key == null)
            {
                throw new InvalidOperationException("Could not open registry key for writing");
            }

            key.SetValue(RegistryValueName, $"\"{executablePath}\" --startup");

            // Verify it was set correctly
            var setValue = key.GetValue(RegistryValueName);
            if (setValue == null)
            {
                throw new InvalidOperationException("Registry value was not set properly");
            }
        }

        private void RemoveRegistryEntry()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, true);
                if (key == null) return;

                var currentValue = key.GetValue(RegistryValueName);
                if (currentValue != null)
                {
                    key.DeleteValue(RegistryValueName, false);
                }
            }
            catch
            {
                // Ignore errors when removing registry entry
            }
        }
    }
}
