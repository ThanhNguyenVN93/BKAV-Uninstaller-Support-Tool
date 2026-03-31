# Changelog

## [1.1.0] - 2024-01-XX

### 🔴 Critical Fixes
- **FIX #4:** Fixed catastrophic registry deletion bug where `DeleteFromRegistry()` was deleting entire `System\ControlSet001/002\Services` tree. Now uses safe `DeleteBkavServicesFromPath()` to filter by name only.
- **FIX #1:** Fixed cross-thread UI exception by using `BeginInvoke()` instead of `Task.ContinueWith()` for auto-triggering cleanup.
- **FIX #2:** Fixed overly broad CLSID matching - removed `Contains("context")` pattern that was deleting unrelated app context menus.
- **FIX #3:** Fixed shell handler matching - removed broad pattern to avoid deleting other apps' shell integration.

### 🟡 Bug Fixes
- **FIX #5:** Fixed TypeLib deletion early return - changed `return` to `break` to continue scanning remaining GUIDs.
- **FIX #6:** Fixed registry handle leak - wrapped `OpenSubKey()` in `using` statement.

### 🟢 Code Cleanup
- Removed 9 unnecessary documentation .md files from source
- Code style and optimization maintained

### ✨ Features (Already Implemented)
- Smart auto-detection of BKAV installation/leftovers
- 9-step comprehensive cleanup process
- Registry Finder tool for real-time scanning
- Registry Comparator for before/after verification
- Progress dialog showing cleanup steps
- Safe error handling throughout

### 🔒 Safety Improvements
- All registry operations now filter by specific names
- No more broad pattern matching in critical paths
- Added `DeleteBkavServicesFromPath()` for safe Services cleanup
- Proper resource disposal for registry handles

---

## [1.0.0] - 2023-XX-XX

### Initial Release
- BKAV Pro uninstaller with registry cleanup
- Auto-detection of installation/leftovers
- Registry Finder tool
- Registry Comparator tool
- Multi-step cleanup process

---

## Known Issues

None at this time. All critical bugs fixed in v1.1.0.

---

## Testing Status

- ✅ Build: Successful
- ✅ All syntax errors resolved
- ✅ Cross-thread UI issues fixed
- ✅ Registry deletion safety verified
- ✅ Error handling comprehensive

---

## Deployment Status

**Ready for production** ✅

Use with:
- Windows XP or newer
- .NET Framework 4.6+
- Administrator privileges required
