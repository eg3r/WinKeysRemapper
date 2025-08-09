# WinKeysRemapper

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)](https://www.microsoft.com/windows)
[![GitHub release](https://img.shields.io/github/v/release/eg3r/WinKeysRemapper)](https://github.com/eg3r/WinKeysRemapper/releases/latest)

A lightweight Windows tray application for remapping keyboard keys when specific applications are active.

## ✨ Features

- 🎯 **Application-Specific Remapping** - Key remapping only activates when your target application is in focus
- ⚡ **Dynamic Hook Management** - Automatically enables/disables based on application focus  
- 🛡️ **System Tray Integration** - Runs quietly in the background with tray icon controls
- 🏁 **Windows Startup Support** - Can auto-start with Windows (configurable)
- 📝 **Real-time Configuration** - Reload settings without restarting the application

## ️ User Interface

WinKeysRemapper features a clean system tray interface that keeps it running quietly in the background:

### System Tray
- **Background Operation** - The application runs silently in the system tray without cluttering your desktop
- **Tray Icon** - Look for the WinKeysRemapper icon in your system tray (bottom-right corner of your screen)
- **Context Menu** - Right-click the tray icon to access application controls:
  - **Reload Configuration** - Instantly reload your `key_mappings.json` file without restarting the app
  - **Open Config** - Open the configuration file with your default JSON editor (e.g., VS Code, Notepad++)
  - **Start with Windows** - Toggle automatic startup when Windows boots (adds/removes registry entry)
  - **Exit** - Cleanly shutdown the application and remove the keyboard hook

### Admin Privileges
⚠️ **Important:** WinKeysRemapper requires administrator privileges to install low-level keyboard hooks. When you first run the application, Windows will prompt you to allow elevated permissions.

### Configuration Management
- Edit your `key_mappings.json` file with any text editor
- Use the "Reload Configuration" option from the tray menu to apply changes instantly
- No need to restart the application when updating key mappings

## ⚙️ Configuration

The application uses JSON configuration files. On first run, it creates a default `key_mappings.json`:

```json
{
  "TargetApplication": "notepad",
  "KeyMappings": {
    "A": "LeftArrow",
    "D": "RightArrow", 
    "W": "UpArrow",
    "S": "DownArrow",
    "1": "E"
  }
}
```

**Configuration Notes:**
- The configuration file is created automatically in the same directory as the executable
- Use the system tray "Reload Configuration" option to apply changes without restarting
- The `TargetApplication` field supports partial process name matching (e.g., "note" matches "notepad")

### Supported Keys

The remapper supports 200+ keys including:
- **Letters:** A-Z
- **Numbers:** 0-9, Numpad 0-9
- **Function Keys:** F1-F24
- **Arrows:** Left, Right, Up, Down
- **Modifiers:** Ctrl, Shift, Alt, Windows
- **Media Keys:** Volume, Play/Pause, Next/Previous
- **Special Keys:** Space, Enter, Escape, Tab, etc.
- **Symbols:** Punctuation and special characters

## 🚀 Download & Installation

### For Users

1. **Download the latest release:**
   - Visit the [Releases page](https://github.com/eg3r/WinKeysRemapper/releases/latest)
   - Download `WinKeysRemapper.exe` from the latest release
   - No installation required - it's a portable executable

2. **Run the application:**
   - Right-click `WinKeysRemapper.exe` and select "Run as administrator"
   - The application will appear in your system tray
   - A default `key_mappings.json` file will be created automatically

3. **Configure your key mappings:**
   - Right-click the tray icon → "Open Config"
   - Edit the JSON file to set your target application and key mappings
   - Right-click the tray icon → "Reload Configuration" to apply changes

### Prerequisites
- Windows 10/11
- Administrator privileges (required for keyboard hooks)
- .NET 8.0 Runtime (automatically installed on most Windows systems)

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🐛 Issues

If you encounter any issues or have feature requests, please [open an issue](https://github.com/eg3r/WinKeysRemapper/issues) on GitHub.

---

## 🛠️ Development

### 📁 Project Structure

```
WinKeysRemapper/
├── src/                          # Source code
│   ├── Configuration/            # Configuration management
│   ├── Input/                    # Keyboard hooks and input handling
│   ├── UI/                       # User interface components
│   │   └── Services/             # Service layer classes
│   ├── Program.cs
│   └── WinKeysRemapper.csproj
├── config/                       # Example configuration files
├── docs/                         # Documentation
├── build.bat                     # Build script
├── key_mappings.json             # Runtime configuration
├── LICENSE
└── README.md
```

### 🏗️ Architecture

#### PowerToys-Inspired Design
- **Background Process Detection** - Expensive operations moved out of hook callbacks
- **Event Flagging System** - Prevents infinite loops and duplicate events
- **Timer-Based Optimization** - Process checking on background threads
- **Early Return Patterns** - Minimal processing in hot paths

#### Performance Features
- **Pre-normalized Keys** - Configuration keys normalized once during loading
- **Optimized Hook Callbacks** - Minimal allocations and processing
- **Target Application Filtering** - Only active when target application has focus

### Building from Source

#### Prerequisites for Development
- Windows 10/11
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code

#### Build Commands
```bash
# Clone and build
git clone https://github.com/eg3r/WinKeysRemapper.git
cd WinKeysRemapper
dotnet build src/WinKeysRemapper.csproj

# Run tests (if available)
dotnet test

# Create release build
dotnet build src/WinKeysRemapper.csproj -c Release
```

#### Quick Build Script
```bash
./build.bat
```

### VS Code Support
The project includes VS Code tasks for building and running:
- `Ctrl+Shift+P` → "Tasks: Run Task" → "build" or "run"

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

## 🙏 Acknowledgments

- Inspired by [Microsoft PowerToys](https://github.com/microsoft/PowerToys) architecture patterns
- Built with .NET 8.0 and Windows API integration
