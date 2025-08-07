using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace WinKeysRemapper.Configuration
{
    public class KeyMappingConfig
    {
        public string TargetApplication { get; set; } = "";
        public Dictionary<string, string> KeyMappings { get; set; } = new Dictionary<string, string>();
    }

    public class ConfigurationManager
    {
        private const string ConfigFileName = "key_mappings.json";
        private string _configPath;

        public ConfigurationManager()
        {
            _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);
        }

        public KeyMappingConfig LoadConfig()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    string jsonContent = File.ReadAllText(_configPath);
                    var config = JsonSerializer.Deserialize<KeyMappingConfig>(jsonContent);
                    if (config != null)
                    {
                        // Normalize keys to uppercase and trimmed during loading for better performance
                        config.KeyMappings = NormalizeKeyMappings(config.KeyMappings);
                        return config;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configuration: {ex.Message}");
            }

            return GetDefaultConfig();
        }

        public void SaveConfig(KeyMappingConfig config)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonContent = JsonSerializer.Serialize(config, options);
                File.WriteAllText(_configPath, jsonContent);
                Console.WriteLine($"Configuration saved to {_configPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving configuration: {ex.Message}");
            }
        }

        private KeyMappingConfig GetDefaultConfig()
        {
            var defaultConfig = new KeyMappingConfig
            {
                TargetApplication = "notepad",
                KeyMappings = new Dictionary<string, string>
                {
                    { "A", "LeftArrow" },   // A -> Left Arrow
                    { "D", "RightArrow" },  // D -> Right Arrow
                    { "W", "UpArrow" },     // W -> Up Arrow
                    { "S", "DownArrow" },   // S -> Down Arrow
                    { "1", "E" }            // 1 -> E
                }
            };

            // Normalize the default keys as well
            defaultConfig.KeyMappings = NormalizeKeyMappings(defaultConfig.KeyMappings);

            // Save the default config if it doesn't exist
            if (!File.Exists(_configPath))
            {
                SaveConfig(defaultConfig);
            }

            return defaultConfig;
        }

        /// <summary>
        /// Normalizes key mappings to uppercase and trimmed for consistent parsing
        /// </summary>
        /// <param name="keyMappings">Original key mappings</param>
        /// <returns>Normalized key mappings</returns>
        private Dictionary<string, string> NormalizeKeyMappings(Dictionary<string, string> keyMappings)
        {
            var normalized = new Dictionary<string, string>();
            foreach (var mapping in keyMappings)
            {
                string normalizedKey = mapping.Key.ToUpperInvariant().Trim();
                string normalizedValue = mapping.Value.ToUpperInvariant().Trim();
                normalized[normalizedKey] = normalizedValue;
            }
            return normalized;
        }
    }
}
