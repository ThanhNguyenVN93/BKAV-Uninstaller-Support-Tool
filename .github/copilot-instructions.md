# Copilot Instructions

## Project Guidelines
- User works on a BKAV Uninstaller Tool (WinForms, .NET Framework 4.6, C# 7.3). They've had experience with accidentally deleting entire Windows Services registry trees (ControlSet001/002\Services) instead of just BKAV entries. Safety around registry deletion is critical. Always filter by specific names before deleting registry keys in important system paths. Verify that registry operations only target specific entries and never delete entire parent trees like Services.