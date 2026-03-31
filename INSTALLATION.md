# Installation & Setup Guide

## System Requirements

- **OS:** Windows XP or newer
- **.NET Framework:** 4.6 or newer
- **Memory:** 50 MB RAM
- **Disk Space:** 10 MB
- **Permissions:** Administrator privileges (required for registry modifications)

## Download & Install

### Option 1: Pre-built Binary

1. Download `BKAV-Pro-Uninstaller-Tool.exe` from Releases
2. Right-click → Run as Administrator
3. Tool starts automatically

### Option 2: Build from Source

#### Prerequisites
- Visual Studio 2019+ or Visual Studio Code
- .NET Framework 4.6 SDK
- Git

#### Build Steps

```bash
# Clone repository
git clone https://github.com/[username]/BKAV-Pro-Uninstaller-Tool.git
cd BKAV-Pro-Uninstaller-Tool-master

# Build solution
dotnet build -c Release

# Output file
# bin/Release/WindowsFormsApp1.exe
```

Or use Visual Studio:
1. Open `WindowsFormsApp1.sln`
2. Build → Build Solution (Ctrl+Shift+B)
3. Run (F5 or Ctrl+F5)

## First Launch

### What Happens
```
Launch
├─ Detects BKAV installation
│  ├─ Found uninstaller? → Show path, prompt to uninstall
│  ├─ Found leftovers? → Auto-run cleanup
│  └─ Not found? → Exit with message
└─ Display system info (OS version)
```

### Three Scenarios

**Scenario 1: BKAV Installed (Uninstaller Found)**
```
Status: "C:\Program Files (x86)\BkavPro\BkavUnst.exe"
Action: Click "🔧 Uninstall BKAV"
Result: Official uninstaller runs
```

**Scenario 2: BKAV Removed But Leftovers Exist**
```
Status: "Phát hiện tàn dư BKAV. Đang xóa..."
Action: Auto-cleanup starts (no user interaction)
Result: Registry/files/shortcuts deleted, success message
```

**Scenario 3: BKAV Not Installed & No Leftovers**
```
Status: "Bạn đang không dùng BKAV!"
Action: Tool exits
Result: No changes made
```

## Manual Cleanup (If Needed)

### Export Before/After Registry

```powershell
# As Administrator in PowerShell

# Before cleanup
reg export HKEY_CLASSES_ROOT "D:\before-classes.txt" /y
reg export HKEY_LOCAL_MACHINE "D:\before-machine.txt" /y
reg export HKEY_CURRENT_USER "D:\before-user.txt" /y

# [Run cleanup here]

# After cleanup
reg export HKEY_CLASSES_ROOT "D:\after-classes.txt" /y
reg export HKEY_LOCAL_MACHINE "D:\after-machine.txt" /y
reg export HKEY_CURRENT_USER "D:\after-user.txt" /y
```

### Use Registry Comparator

1. Click "💻 System Info" label
2. Select "📊 Registry Comparator"
3. Choose before & after files
4. Click "Compare Registry Files"
5. See remaining entries (should be ~0)

### Manual Registry Deletion

If Registry Comparator shows entries:

```powershell
# Delete specific BKAV registry entries as Admin
reg delete "HKEY_CLASSES_ROOT\AppID\BkavSystemServer.exe" /f
reg delete "HKEY_LOCAL_MACHINE\Software\BKAV" /s /f
reg delete "HKEY_CURRENT_USER\Software\BKAV" /s /f
# ... etc for each entry from Comparator
```

## Verification

After cleanup, verify with **Registry Finder**:

1. Click "💻 System Info"
2. Select "🔍 Registry Finder"
3. Click "Scan Registry"
4. Should show: **"✓ SUCCESS! No remaining entries found."**

If entries remain:
- Run cleanup again
- Or manually delete via `regedit.exe`
- Check `Computer\HKEY_LOCAL_MACHINE\Software\BKAV`

## Reboot

After cleanup:
```
1. Click OK on success dialog
2. Reboot Windows (Ctrl+Alt+Delete → Sign out → Restart)
3. Verify no BKAV processes running:
   - Task Manager: No bkav.exe, BkavService, etc.
   - Services: No BKAV services
   - Startup: No BKAV shortcuts
```

## Troubleshooting

### "Access Denied" When Deleting Registry

- **Cause:** Running without admin rights
- **Fix:** Right-click tool → "Run as Administrator"

### Tool Crashes on Launch

- **Cause:** .NET Framework not installed
- **Fix:** Install .NET Framework 4.6+
  ```bash
  # Download from Microsoft
  # https://dotnet.microsoft.com/download/dotnet-framework/net46
  ```

### Leftover Entries Still Visible in Registry

- **Cause:** Tool couldn't access some keys
- **Fix:** 
  1. Use Registry Finder to get exact path
  2. Open `regedit.exe` as Admin
  3. Navigate to key
  4. Right-click → Delete
  5. Restart

### Tool Won't Start

- **Cause:** Windows Defender/antivirus blocking
- **Fix:** Add exe to antivirus whitelist or compile from source

## Advanced: Build from Command Line

```bash
# Navigate to project directory
cd D:\code\BKAV-Pro-Uninstaller-Tool-master\BKAV-Pro-Uninstaller-Tool-master

# Release build
dotnet build -c Release -o ./bin/Release

# Run tests (if available)
dotnet test

# Create installer (optional)
# Use tools like NSIS or InnoSetup for .msi/.exe installer
```

## Support Files

- `README.md` - Overview & features
- `BUGFIXES.md` - Detailed bug fixes (v1.1.0)
- `CHANGELOG.md` - Version history
- `.github/copilot-instructions.md` - Development guidelines

---

**Ready to use!** 🎉
