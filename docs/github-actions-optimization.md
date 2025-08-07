# GitHub Actions Runner Optimization Notes

## Pre-installed Software on GitHub Runners

### Windows-latest (Windows Server 2022)
The `windows-latest` runners come with multiple .NET versions pre-installed:
- .NET Framework 4.8, 4.7.2, 4.6.2
- .NET Core 3.1.x
- .NET 5.0.x  
- .NET 6.0.x
- .NET 7.0.x
- **✅ .NET 8.0.x** (our target)

### Performance Benefits
By using pre-installed .NET instead of `actions/setup-dotnet@v4`:
- **⚡ ~30-60 seconds faster** builds (no download/install time)
- **🔄 Fewer network requests** and dependencies
- **💿 Less cache usage** and storage overhead
- **🎯 More reliable** (no network download failures)

### Current Configuration
Our workflows now use:
```yaml
- name: Verify .NET version
  run: dotnet --version
```

Instead of:
```yaml
- name: Setup .NET
  uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '8.0.x'
```

### Fallback Strategy
If a specific .NET version is ever needed that's not pre-installed, we can easily add back the setup step. The current approach works because:
1. .NET 8 is LTS and stable
2. GitHub maintains recent versions on their runners
3. Our project doesn't require bleeding-edge .NET features

### Other Runner Options
- **ubuntu-latest**: Also has .NET pre-installed, but we need Windows for Windows API calls
- **macos-latest**: Has .NET pre-installed, but incompatible with Windows-specific APIs
- **Self-hosted**: Could be configured with any .NET version, but adds infrastructure overhead

### References
- [GitHub Actions Runner Images](https://github.com/actions/runner-images)
- [Windows 2022 Included Software](https://github.com/actions/runner-images/blob/main/images/win/Windows2022-Readme.md)
