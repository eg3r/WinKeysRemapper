# 📦 **Build Types Explained**

WinKeysRemapper provides three different build types to suit different needs:

## 🏗️ **Build Variants:**

### **1. x64 (Self-Contained)**
- **File**: `WinKeysRemapper-v1.0.0-x64.zip`
- **Size**: ~70-100MB
- **Requirements**: None
- **Best for**: Modern 64-bit Windows users
- **Includes**: Complete .NET runtime bundled

### **2. x86 (Self-Contained)**  
- **File**: `WinKeysRemapper-v1.0.0-x86.zip`
- **Size**: ~70-100MB
- **Requirements**: None
- **Best for**: Older Windows or 32-bit systems
- **Includes**: Complete .NET runtime bundled

### **3. Portable (Framework-Dependent)**
- **File**: `WinKeysRemapper-v1.0.0-portable.zip`
- **Size**: ~1-5MB
- **Requirements**: .NET 8 Runtime installed
- **Best for**: Developers and power users
- **Includes**: Just the application code

## 🎯 **Which Should I Download?**

| If you have... | Download... |
|----------------|-------------|
| **64-bit Windows** (most common) | **x64** |
| **32-bit Windows** or older system | **x86** |
| **.NET 8 already installed** | **Portable** (smallest) |
| **Unsure** | **x64** (safest choice) |

## 💻 **How to Check Your System:**
- **64-bit**: Task Manager → Performance → CPU shows "x64"
- **32-bit**: Task Manager → Performance → CPU shows "x86"
- **.NET 8**: Run `dotnet --version` in Command Prompt
