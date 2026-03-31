# Contributing Guidelines

## How to Report Bugs

1. **Search existing issues** - Avoid duplicates
2. **Create new issue** with:
   - OS version (Windows XP, 7, 10, 11, Server, etc.)
   - .NET Framework version
   - Clear description of bug
   - Steps to reproduce
   - Screenshots if applicable

Example:
```
Title: Registry Finder crashes on Windows 11

OS: Windows 11 Pro (22H2)
.NET: Framework 4.8
Reproducible: Yes

Steps:
1. Launch tool
2. Click System Info
3. Select Registry Finder
4. Click Scan Registry
→ Crash with error "..."

Expected: Should scan and display entries
Actual: Crash
```

## How to Contribute Code

### 1. Fork & Clone
```bash
git clone https://github.com/[your-username]/BKAV-Pro-Uninstaller-Tool.git
cd BKAV-Pro-Uninstaller-Tool-master
```

### 2. Create Feature Branch
```bash
git checkout -b feature/your-feature-name
# or
git checkout -b fix/your-bug-fix
```

### 3. Make Changes

#### Code Style
- Use C# 7.3 style (required by .NET 4.6)
- Follow Microsoft naming conventions
- Add comments for complex logic
- Keep lines under 120 characters

#### Registry Safety (CRITICAL!)
- ✅ **DO:** Filter by specific name before deletion
  ```csharp
  if (subKeyName.ToLower().Contains("bkav"))
      key.DeleteSubKeyTree(subKeyName, false);
  ```
- ❌ **DON'T:** Delete entire paths without filtering
  ```csharp
  // NEVER do this!
  DeleteFromRegistry(Registry.LocalMachine, @"System\CurrentControlSet\Services");
  ```
- ✅ **DO:** Use `using` for registry handles
- ❌ **DON'T:** Forget to dispose registry keys

#### UI Threading
- ✅ **DO:** Use `BeginInvoke()` for cross-thread UI access
  ```csharp
  this.BeginInvoke(new Action(() => { /* UI code */ }));
  ```
- ❌ **DON'T:** Access UI from background threads
  ```csharp
  // NEVER
  Task.Run(() => btnremove.PerformClick());
  ```

#### Error Handling
- ✅ **DO:** Catch and log exceptions
  ```csharp
  catch (Exception ex)
  {
      Debug.WriteLine($"Error: {ex.Message}");
  }
  ```
- ❌ **DON'T:** Leave bare catches
  ```csharp
  // NEVER
  catch { }
  ```

### 4. Test Your Changes

```bash
# Build in debug mode
dotnet build -c Debug

# Run in Visual Studio
# Press F5 or Ctrl+F5

# Test checklist:
# [ ] No compilation errors
# [ ] No compiler warnings
# [ ] Launch doesn't crash
# [ ] All buttons work
# [ ] Registry operations safe
# [ ] Clean up after tests
```

### 5. Commit & Push

```bash
# Commit with clear message
git add .
git commit -m "Fix: Description of what was fixed"

# Or for features
git commit -m "Feature: Add new capability"

# Push to your fork
git push origin feature/your-feature-name
```

### 6. Create Pull Request

1. Go to GitHub repository
2. Click "New Pull Request"
3. Select your branch
4. Add description:
   - What does this fix/add?
   - Why is it needed?
   - How to test?
5. Submit PR

## Testing Checklist Before PR

- [ ] Code compiles without errors
- [ ] No new warnings
- [ ] Registry safety verified (no unsafe deletes)
- [ ] Cross-thread UI access fixed (if any)
- [ ] Proper error handling added
- [ ] Registry handles disposed properly
- [ ] No resource leaks
- [ ] Application doesn't crash on launch
- [ ] All UI elements functional
- [ ] Documentation updated (if needed)

## Code Review Process

1. Maintainer reviews code
2. Request changes if needed
3. Push new commits to same branch
4. Process repeats until approved
5. Merge to main branch

## Coding Standards

### Constants
```csharp
private const string BKAV_PRO_FOLDER = "BkavPro";
private const string ERROR_MESSAGE = "An error occurred";
private static readonly int[] BTN_COLOR = { 40, 167, 69 };
```

### Methods
```csharp
// Clear, descriptive names
private void DeleteBkavFromRegistry() { }
private bool HasBkavLeftovers() { }
private void CleanEnvironmentVariables() { }

// NOT
private void Delete() { }
private bool Check() { }
```

### Comments
```csharp
// Good
using (RegistryKey key = baseKey.OpenSubKey(searchPath, true))
{
    if (key != null)
    {
        // Iterate each subkey and check name for BKAV reference
        foreach (var subKeyName in key.GetSubKeyNames())
```

### Try-Catch
```csharp
try
{
    // Operation
}
catch (UnauthorizedAccessException ex)
{
    Debug.WriteLine($"Permission denied: {ex.Message}");
}
catch (Exception ex)
{
    Debug.WriteLine($"Unexpected error: {ex.Message}");
}
```

## Common Issues to Fix

### 1. Registry Handle Leak
```csharp
// ❌ WRONG
var key = baseKey.OpenSubKey(path, true);
if (key != null) { }

// ✅ CORRECT
using (var key = baseKey.OpenSubKey(path, true))
{
    if (key != null) { }
}
```

### 2. Overly Broad Pattern Matching
```csharp
// ❌ WRONG - Matches too much
if (name.Contains("context")) { delete(); }

// ✅ CORRECT - Specific to BKAV
if (name.ToLower().Contains("bkav")) { delete(); }
```

### 3. Cross-Thread UI Access
```csharp
// ❌ WRONG
Task.Run(() => btnremove.PerformClick());

// ✅ CORRECT
this.BeginInvoke(new Action(() => btnremove.PerformClick()));
```

### 4. Unsafe Registry Deletion
```csharp
// ❌ WRONG - Deletes entire tree!
DeleteFromRegistry(Registry.LocalMachine, @"System\Services");

// ✅ CORRECT - Filter by name
foreach (var service in key.GetSubKeyNames())
{
    if (service.Contains("bkav"))
        key.DeleteSubKeyTree(service);
}
```

## Questions?

- Check existing issues/discussions
- Read `README.md` for overview
- Review `BUGFIXES.md` for known issues
- Check `.github/copilot-instructions.md` for dev guidelines

---

**Thank you for contributing!** 🙏
