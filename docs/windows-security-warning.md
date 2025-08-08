# 🛡️ **Windows Security Warning - This is Normal!**

When you download and run WinKeysRemapper, Windows may show a security warning like:

> **"Windows protected your PC"**  
> **"Microsoft Defender SmartScreen prevented an unrecognized app from starting"**

## ✅ **This is completely normal for downloaded software!**

### **Why does this happen?**
- WinKeysRemapper is **not code-signed** with an expensive certificate
- Windows shows this warning for **any unsigned executable**
- This happens to **most open-source software**
- It does **NOT** mean the software is malicious

### **How to run WinKeysRemapper safely:**

#### **Method 1: Click "More info" → "Run anyway"**
1. When the warning appears, click **"More info"**
2. Click **"Run anyway"** button
3. WinKeysRemapper will start normally

#### **Method 2: Right-click → Properties → Unblock**
1. Right-click the downloaded file
2. Select **"Properties"**
3. Check **"Unblock"** at the bottom
4. Click **"OK"**
5. Run the file normally

#### **Method 3: Add to Windows Defender exclusions**
1. Open **Windows Security**
2. Go to **Virus & threat protection**
3. Add the folder to **exclusions**

## 🔒 **Security Verification:**
- **Source code**: Fully open on GitHub
- **Builds**: Created by GitHub Actions (transparent)
- **No network access**: App works completely offline
- **No data collection**: No telemetry or tracking

## 🤔 **Still concerned?**
- Review the **source code** on GitHub
- Build from source yourself using `dotnet build`
- Check the **GitHub Actions logs** to see exactly how binaries are built

**This warning is Windows being extra cautious - it's safe to proceed!** ✅
