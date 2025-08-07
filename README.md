# WinKeysRemapper

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)](https://www.microsoft.com/windows)

A high-performance Windows keyboard input remapper that allows you to remap specific keys for targeted applications. Built with PowerToys-inspired architecture for minimal system impact.

## ✨ Features

- 🎯 **Application-Specific Remapping** - Target specific applications without affecting system-wide behavior
- ⚡ **High Performance** - PowerToys-inspired architecture with minimal CPU and memory overhead
- 🔧 **Comprehensive Key Support** - 200+ supported keys including function keys, media keys, numpad, and symbols
- 📝 **JSON Configuration** - Easy-to-edit configuration files
- 🚀 **Low-Level Hooks** - Direct Windows API integration for reliable key interception
- 🛡️ **Background Processing** - Optimized hook callbacks with background process detection

## 🚀 Quick Start

### Prerequisites
- Windows 10/11
- .NET 8.0 Runtime

### Installation

1. **Clone the repository:**
   ```bash
   git clone https://github.com/yourusername/WinKeysRemapper.git
   cd WinKeysRemapper
   ```

2. **Build the project:**
   ```bash
   dotnet build src/WinKeysRemapper.csproj
   ```

3. **Run the application:**
   ```bash
   dotnet run --project src/WinKeysRemapper.csproj
   ```

### Quick Build Script
```bash
./build.bat
```

## 📁 Project Structure

```
WinKeysRemapper/
├── src/                          # Source code
│   ├── Configuration/            # Configuration management
│   │   └── ConfigurationManager.cs
│   ├── Input/                    # Input handling and keyboard hooks
│   │   ├── LowLevelKeyboardHook.cs
│   │   └── VirtualKeyParser.cs
│   ├── Program.cs               # Application entry point
│   └── WinKeysRemapper.csproj   # Project file
├── config/                      # Configuration files
│   └── key_mappings_example.json
├── docs/                       # Documentation
│   └── README.md              # Detailed documentation
├── LICENSE                     # MIT License
├── README.md                   # This file
└── WinKeysRemapper.sln        # Solution file
```

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

## 🏗️ Architecture

### PowerToys-Inspired Design
- **Background Process Detection** - Expensive operations moved out of hook callbacks
- **Event Flagging System** - Prevents infinite loops and duplicate events
- **Timer-Based Optimization** - Process checking on background threads
- **Early Return Patterns** - Minimal processing in hot paths

### Performance Features
- **Pre-normalized Keys** - Configuration keys normalized once during loading
- **Optimized Hook Callbacks** - Minimal allocations and processing
- **Target Application Filtering** - Only active when target application has focus

## 🛠️ Development

### Building from Source
```bash
# Clone and build
git clone https://github.com/yourusername/WinKeysRemapper.git
cd WinKeysRemapper
dotnet build src/WinKeysRemapper.csproj

# Run tests (if available)
dotnet test

# Create release build
dotnet build src/WinKeysRemapper.csproj -c Release
```

### VS Code Support
The project includes VS Code tasks for building and running:
- `Ctrl+Shift+P` → "Tasks: Run Task" → "build" or "run"

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

## 🐛 Issues

If you encounter any issues or have feature requests, please [open an issue](https://github.com/yourusername/WinKeysRemapper/issues) on GitHub.

## 🙏 Acknowledgments

- Inspired by [Microsoft PowerToys](https://github.com/microsoft/PowerToys) architecture patterns
- Built with .NET 8.0 and Windows API integration
