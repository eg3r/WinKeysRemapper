using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace WinKeysRemapper.Configuration
{
    public class StartupManager
    {
        private const string TaskName = "WinKeysRemapper";
        private const string RegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string RegistryValueName = "WinKeysRemapper";

        public bool IsStartupEnabled()
        {
            return IsScheduledTaskEnabled() || IsRegistryEntryEnabled();
        }

        public void EnableStartup()
        {
            var executablePath = GetExecutablePath();
            
            // Try scheduled task first (better for Windows 11)
            if (CreateScheduledTask(executablePath))
            {
                return;
            }
            
            // Fallback to registry method
            CreateRegistryEntry(executablePath);
        }

        public void DisableStartup()
        {
            RemoveScheduledTask();
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

        private bool IsScheduledTaskEnabled()
        {
            try
            {
                var script = $@"
                    $task = Get-ScheduledTask -TaskName '{TaskName}' -ErrorAction SilentlyContinue
                    if ($task -and $task.State -ne 'Disabled') {{
                        Write-Output 'EXISTS'
                    }}
                ";

                var processInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                };

                using var process = Process.Start(processInfo);
                if (process == null) return false;

                process.WaitForExit(5000);
                var output = process.StandardOutput.ReadToEnd().Trim();
                return output.Contains("EXISTS");
            }
            catch
            {
                return false;
            }
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

        private bool CreateScheduledTask(string executablePath)
        {
            try
            {
                var userName = Environment.UserName;
                var script = $@"
                    $action = New-ScheduledTaskAction -Execute '{executablePath}' -Argument '--startup'
                    $trigger = New-ScheduledTaskTrigger -AtLogOn -User '{userName}'
                    $settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable
                    $principal = New-ScheduledTaskPrincipal -UserId '{userName}' -LogonType Interactive -RunLevel Highest
                    
                    # Remove existing task if it exists
                    if (Get-ScheduledTask -TaskName '{TaskName}' -ErrorAction SilentlyContinue) {{
                        Unregister-ScheduledTask -TaskName '{TaskName}' -Confirm:$false
                    }}
                    
                    # Register the new task
                    Register-ScheduledTask -TaskName '{TaskName}' -Action $action -Trigger $trigger -Settings $settings -Principal $principal
                ";

                var processInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var process = Process.Start(processInfo);
                if (process == null) return false;

                process.WaitForExit(10000);
                return process.ExitCode == 0;
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

        private void RemoveScheduledTask()
        {
            try
            {
                var script = $@"
                    if (Get-ScheduledTask -TaskName '{TaskName}' -ErrorAction SilentlyContinue) {{
                        Unregister-ScheduledTask -TaskName '{TaskName}' -Confirm:$false
                    }}
                ";

                var processInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(processInfo);
                process?.WaitForExit(5000);
            }
            catch
            {
                // Ignore errors when removing scheduled task
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
