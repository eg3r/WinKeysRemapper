# Release v2.8.2 - Preparation Complete

## 📝 Changes in This Release

This release includes:
- Fix: Update release notes generation to use URL-encoded tag message for better formatting in GitHub releases
- Updated version to 2.8.2 in project file

## 🚀 To Create the Release

The release preparation is complete. To publish this release, follow these steps:

### Option 1: GitHub UI (Recommended)
1. Go to https://github.com/eg3r/WinKeysRemapper/releases/new
2. Click **"Choose a tag"** → Type `v2.8.2` → **"Create new tag: v2.8.2 on publish"**
3. Set **Release title** to: `WinKeysRemapper v2.8.2`
4. Add **Release description**:
   ```
   Improved release workflow with URL-encoded tag messages for better formatting
   ```
5. Click **"Publish release"**

### Option 2: Command Line
```bash
git tag -a v2.8.2 -m "Improved release workflow with URL-encoded tag messages for better formatting"
git push origin v2.8.2
```

## ⚙️ What Happens Automatically

Once you create and push the tag, the GitHub Actions workflow will automatically:
- ✅ Build x64, x86, and portable versions with version 2.8.2
- ✅ Create optimized ZIP files
- ✅ Upload to GitHub Releases with proper release notes
- ✅ Apply proper version numbering to all binaries

## 📦 Artifacts That Will Be Created

The workflow will create and upload:
- `WinKeysRemapper-2.8.2-x64.zip` - 64-bit Windows (self-contained)
- `WinKeysRemapper-2.8.2-x86.zip` - 32-bit Windows (self-contained)  
- `WinKeysRemapper-2.8.2-portable.zip` - Portable version (requires .NET 8)

Each package will include:
- WinKeysRemapper.exe with version 2.8.2
- README.md with complete documentation

## 🎯 Release Notes Preview

The release will show:
```
Release v2.8.2

## 🎯 What's New
Improved release workflow with URL-encoded tag messages for better formatting

## 🛡️ Windows Security Warning
Windows may show a security warning when running downloaded software. This is normal for unsigned applications.

## 📥 Downloads
- **x64**: For modern 64-bit Windows (most common)
- **x86**: For older or 32-bit Windows
- **portable**: Requires .NET 8 (smallest download)

## 📖 Documentation
Each download includes:
- Complete usage guide
- Setup instructions
- Security guidance
```

## ✅ Verification

After the release is published, verify:
1. All three ZIP files are uploaded
2. Release notes are properly formatted
3. Download counts start incrementing
4. The release appears on the main repository page

---

**Note**: This file can be deleted after the release is published.
