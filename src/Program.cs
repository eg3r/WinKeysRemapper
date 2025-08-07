using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using WinKeysRemapper.Configuration;
using WinKeysRemapper.Input;

namespace WinKeysRemapper
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("WinKeysRemapper - Windows Keyboard Input Remapper");
            Console.WriteLine("================================================");
            Console.WriteLine();
            
            try
            {
                // Load configuration
                var configManager = new ConfigurationManager();
                var config = configManager.LoadConfig();
                
                Console.WriteLine($"Target Application: {config.TargetApplication}");
                Console.WriteLine("Key Mappings:");
                foreach (var mapping in config.KeyMappings)
                {
                    Console.WriteLine($"  {mapping.Key} -> {mapping.Value}");
                }
                Console.WriteLine();
                
                // Convert configuration to the format expected by the new hook
                var keyMappings = new Dictionary<int, int>();
                var targetApplications = new HashSet<string> { config.TargetApplication };
                
                // Convert string key names to virtual key codes
                foreach (var mapping in config.KeyMappings)
                {
                    if (VirtualKeyParser.TryParseVirtualKey(mapping.Key, out int fromKey) && 
                        VirtualKeyParser.TryParseVirtualKey(mapping.Value, out int toKey))
                    {
                        keyMappings[fromKey] = toKey;
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Could not parse key mapping {mapping.Key} -> {mapping.Value}");
                    }
                }
                
                // Create keyboard hook with new architecture
                var keyboardHook = LowLevelKeyboardHook.CreateInstance(keyMappings, targetApplications);
                
                // Start the hook
                keyboardHook.InstallHook();
                Console.WriteLine("Hook installed. Monitoring keyboard input...");
                Console.WriteLine("Press Ctrl+C to stop the application");
                Console.WriteLine();
                
                // Set up Ctrl+C handler for graceful shutdown
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    keyboardHook.Dispose();
                    Console.WriteLine("\nHook removed. Application stopped.");
                    Environment.Exit(0);
                };
                
                // Keep the application running with a simple wait loop
                // No need for Windows Forms message loop - the hook uses its own thread
                Console.WriteLine("Application running. Press Ctrl+C to exit.");
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
