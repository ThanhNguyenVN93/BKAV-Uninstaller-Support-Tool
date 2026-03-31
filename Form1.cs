using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Windows.Forms;
using Microsoft.Win32;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        // Constants - Paths & Messages
        private const string BKAV_PRO_FOLDER = "BkavPro";
        private const string BKAV_CORP_FOLDER = "Bkav Corporation";
        private const string BKAV_UNINSTALLER = "BkavUnst.exe";
        private const string BKAV_NOT_INSTALLED = "Bạn đang không dùng BKAV!";
        private const string BKAV_REMOVED = "Bạn đã gỡ BKAV Pro. Vui lòng bấm nút Delete Leftover để xoá thư mục còn xót";

        // Constants - Colors (RGB)
        private static readonly int[] BTN_UNINSTALL_NORMAL = { 40, 167, 69 };
        private static readonly int[] BTN_UNINSTALL_HOVER = { 28, 140, 80 };
        private static readonly int[] BTN_DELETE_NORMAL = { 220, 53, 69 };
        private static readonly int[] BTN_DELETE_HOVER = { 200, 40, 55 };

        // Cached paths
        private string _bkavProPath;
        private string _bkavCorpPath;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.ControlBox = false;
            lblwin.Text = GetOSInfo();
            ApplyButtonStyles();

            // Check if BKAV is installed
            InitializeBkavPaths();

            // If BKAV is installed (has uninstaller), show as usual
            if (File.Exists(Path.Combine(_bkavProPath, BKAV_UNINSTALLER)))
            {
                txtinsdrect.Text = Path.Combine(_bkavProPath, BKAV_UNINSTALLER);
                btnunin.Enabled = true;
                btnunin.PerformClick();
            }
            // If BKAV is not installed but there are leftover files/registry
            else if (HasBkavLeftovers())
            {
                txtinsdrect.Text = "Phát hiện tàn dư BKAV. Đang xóa...";
                btnremove.Enabled = true;
                // Use BeginInvoke to run on the UI thread after Load completes
                this.BeginInvoke(new Action(() => btnremove.PerformClick()));
            }
            // If no BKAV at all (not installed and no leftovers)
            else
            {
                MessageBox.Show(BKAV_NOT_INSTALLED, "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Environment.Exit(0);
            }
        }

        private void ApplyButtonStyles()
        {
            SetButtonHoverEffect(btnunin, BTN_UNINSTALL_NORMAL, BTN_UNINSTALL_HOVER);
            SetButtonHoverEffect(btnremove, BTN_DELETE_NORMAL, BTN_DELETE_HOVER);
        }

        private void SetButtonHoverEffect(Button btn, int[] normalColor, int[] hoverColor)
        {
            var normalBrush = System.Drawing.Color.FromArgb(normalColor[0], normalColor[1], normalColor[2]);
            var hoverBrush = System.Drawing.Color.FromArgb(hoverColor[0], hoverColor[1], hoverColor[2]);

            btn.MouseEnter += (s, e) => btn.BackColor = hoverBrush;
            btn.MouseLeave += (s, e) => btn.BackColor = normalBrush;
        }

        private string GetOSInfo()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
                {
                    var results = searcher.Get();
                    if (results.Count > 0)
                    {
                        ManagementObject osObj = null;
                        foreach (ManagementObject obj in results)
                        {
                            osObj = obj;
                            break;
                        }

                        if (osObj != null)
                        {
                            string osInfo = $"{osObj["Caption"]} - {osObj["OSArchitecture"]}";
                            return osInfo
                                .Replace("NT 5.1.2600", "XP")
                                .Replace("NT 5.2.3790", "Server 2003");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetOSInfo Error: {ex.Message}");
            }
            return "Unknown OS";
        }

        private void InitializeBkavPaths()
        {
            string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            _bkavProPath = Path.Combine(programFilesX86, BKAV_PRO_FOLDER);
            _bkavCorpPath = Path.Combine(programFilesX86, BKAV_CORP_FOLDER);
        }

        private bool HasBkavLeftovers()
        {
            // Check for leftover folders
            if (Directory.Exists(_bkavProPath) || Directory.Exists(_bkavCorpPath))
                return true;

            // Check for registry entries
            if (HasBkavRegistryEntries())
                return true;

            // Check for startup shortcuts
            if (HasBkavStartupShortcuts())
                return true;

            return false;
        }

        private bool HasBkavRegistryEntries()
        {
            try
            {
                // Check main BKAV keys
                using (var key = Registry.LocalMachine.OpenSubKey(@"Software\BKAV"))
                {
                    if (key != null) return true;
                }

                using (var key = Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\BKAV"))
                {
                    if (key != null) return true;
                }

                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\BKAV"))
                {
                    if (key != null) return true;
                }

                // Check for BKAV service
                using (var key = Registry.LocalMachine.OpenSubKey(@"System\CurrentControlSet\Services"))
                {
                    if (key != null)
                    {
                        foreach (var serviceName in key.GetSubKeyNames())
                        {
                            if (serviceName.ToLower().Contains("bkav") || serviceName.ToLower().Contains("vsmart"))
                                return true;
                        }
                    }
                }

                // Check for Uninstall entries
                using (var key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall"))
                {
                    if (key != null)
                    {
                        foreach (var appName in key.GetSubKeyNames())
                        {
                            if (appName.ToLower().Contains("bkav"))
                                return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HasBkavRegistryEntries Error: {ex.Message}");
            }

            return false;
        }

        private bool HasBkavStartupShortcuts()
        {
            try
            {
                string startupPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    @"Microsoft\Windows\Start Menu\Programs\Startup"
                );

                if (Directory.Exists(startupPath))
                {
                    foreach (var file in Directory.GetFiles(startupPath, "*.lnk"))
                    {
                        if (file.ToLower().Contains("bkav") || file.ToLower().Contains("v-smart"))
                            return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HasBkavStartupShortcuts Error: {ex.Message}");
            }

            return false;
        }

        private void btnunin_Click(object sender, EventArgs e)
        {
            try
            {
                string uninstallerPath = Path.Combine(_bkavProPath, BKAV_UNINSTALLER);

                if (File.Exists(uninstallerPath))
                {
                    Process.Start(uninstallerPath);
                    btnunin.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnremove_Click(object sender, EventArgs e)
        {
            // Check if there's actually anything to delete
            if (!Directory.Exists(_bkavProPath) && !Directory.Exists(_bkavCorpPath) && !HasBkavRegistryEntries() && !HasBkavStartupShortcuts())
            {
                MessageBox.Show(BKAV_NOT_INSTALLED, "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Environment.Exit(0);
                return;
            }

            DialogResult result = MessageBox.Show(
                "Điều này sẽ xóa:\n" +
                "• Các thư mục BKAV\n" +
                "• Các entry Registry của BKAV\n" +
                "• Các shortcut liên quan\n\n" +
                "Bạn có chắc chắn?",
                "Xác Nhận Xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result != DialogResult.Yes)
                return;

            try
            {
                // Show progress
                using (var progressForm = new ProgressForm("Đang xóa tất cả BKAV leftover..."))
                {
                    progressForm.Show();
                    Application.DoEvents();

                    // 1. Delete folders via CMD
                    progressForm.UpdateStatus("Xóa thư mục BKAV...");
                    DeleteFoldersViaCMD();
                    Application.DoEvents();

                    // 2. Delete Registry entries
                    progressForm.UpdateStatus("Xóa Registry entries...");
                    DeleteBkavRegistry();
                    Application.DoEvents();

                    // 3. Delete startup shortcuts
                    progressForm.UpdateStatus("Xóa startup shortcuts...");
                    DeleteStartupShortcuts();
                    Application.DoEvents();

                    // 4. Remove from environment variables
                    progressForm.UpdateStatus("Dọn dẹp environment variables...");
                    CleanEnvironmentVariables();
                    Application.DoEvents();

                    progressForm.Close();
                }

                MessageBox.Show(
                    "Xóa BKAV leftover thành công!\n\n" +
                    "Hãy khởi động lại máy để hoàn tất.",
                    "Thành Công",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteFoldersViaCMD()
        {
            try
            {
                var procInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C rd /s /q \"{_bkavProPath}\" & rd /s /q \"{_bkavCorpPath}\"",
                    Verb = "runas",
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (var process = Process.Start(procInfo))
                {
                    process?.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteFoldersViaCMD Error: {ex.Message}");
            }
        }

        private void DeleteBkavRegistry()
        {
            try
            {
                // 1. Delete specific BKAV registry paths
                DeleteFromRegistry(Registry.LocalMachine, @"Software\BKAV");
                DeleteFromRegistry(Registry.LocalMachine, @"Software\Wow6432Node\BKAV");
                DeleteFromRegistry(Registry.CurrentUser, @"Software\BKAV");
                DeleteFromRegistry(Registry.LocalMachine, @"Software\Microsoft\Windows NT\CurrentVersion\Drivers32");

                // 2. Search and delete Uninstall entries
                SearchAndDeleteUninstallEntry();

                // 3. Search and delete App Paths
                SearchAndDeleteAppPaths();

                // 4. Delete File Associations
                DeleteFileAssociations();

                // 5. Delete Services
                DeleteBkavServices();

                // 6. Delete Scheduled Tasks
                DeleteScheduledTasks();

                // 7. Delete Run entries
                DeleteRunEntries();

                // 8. Delete Classes associations
                DeleteClassesAssociations();

                // 9. Delete AppID entries
                DeleteAppIDEntries();

                // 10. Delete CLSID entries
                DeleteCLSIDEntries();

                // 11. Delete TypeLib entries
                DeleteTypeLibEntries();

                // 12. Delete Shell Integration
                DeleteShellIntegration();

                // 13. Delete Recent/MUI Cache
                DeleteMuiCache();

                // 14. Delete DownloadManager entries
                DeleteDownloadManagerEntries();

                // 15. FINAL PASS - Comprehensive cleanup of any remaining BKAV entries
                FinalRegistryCleanup();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteBkavRegistry Error: {ex.Message}");
            }
        }

        private void DeleteAppIDEntries()
        {
            try
            {
                // Delete HKEY_CLASSES_ROOT\AppID\*BKAV*
                DeleteBkavFromPath(Registry.ClassesRoot, @"AppID");
                DeleteBkavFromPath(Registry.LocalMachine, @"Software\Classes\AppID");
                DeleteBkavFromPath(Registry.LocalMachine, @"Software\Wow6432Node\Classes\AppID");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteAppIDEntries Error: {ex.Message}");
            }
        }

        private void DeleteCLSIDEntries()
        {
            try
            {
                // Delete HKEY_CLASSES_ROOT\CLSID\*Bkav*
                SearchAndDeleteCLSIDs(Registry.ClassesRoot, @"CLSID");
                SearchAndDeleteCLSIDs(Registry.LocalMachine, @"Software\Classes\CLSID");
                SearchAndDeleteCLSIDs(Registry.LocalMachine, @"Software\Wow6432Node\Classes\CLSID");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteCLSIDEntries Error: {ex.Message}");
            }
        }

        private void SearchAndDeleteCLSIDs(RegistryKey baseKey, string clsidPath)
        {
            try
            {
                using (RegistryKey key = baseKey.OpenSubKey(clsidPath, true))
                {
                    if (key != null)
                    {
                        foreach (var guid in key.GetSubKeyNames())
                        {
                            try
                            {
                                using (var subKey = key.OpenSubKey(guid))
                                {
                                    // Check if CLSID contains BKAV reference
                                    var defValue = subKey?.GetValue("")?.ToString() ?? "";
                                    if (defValue.ToLower().Contains("bkav"))
                                    {
                                        key.DeleteSubKeyTree(guid, false);
                                        Debug.WriteLine($"Deleted CLSID: {guid}");
                                    }

                                    // Also check InProcServer32 path
                                    using (var inProcKey = subKey?.OpenSubKey("InProcServer32"))
                                    {
                                        var dllPath = inProcKey?.GetValue("")?.ToString() ?? "";
                                        if (dllPath.ToLower().Contains("bkav"))
                                        {
                                            key.DeleteSubKeyTree(guid, false);
                                            Debug.WriteLine($"Deleted CLSID with BkavDLL: {guid}");
                                        }
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SearchAndDeleteCLSIDs Error: {ex.Message}");
            }
        }

        private void DeleteTypeLibEntries()
        {
            try
            {
                // Search in GUID-based TypeLib entries
                string typeLibPath = @"Software\Classes\TypeLib";
                DeleteBkavFromTypeLib(Registry.LocalMachine, typeLibPath);
                DeleteBkavFromTypeLib(Registry.LocalMachine, @"Software\Wow6432Node\Classes\TypeLib");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteTypeLibEntries Error: {ex.Message}");
            }
        }

        private void DeleteBkavFromTypeLib(RegistryKey baseKey, string typeLibPath)
        {
            try
            {
                using (RegistryKey key = baseKey.OpenSubKey(typeLibPath, true))
                {
                    if (key != null)
                    {
                        foreach (var guid in key.GetSubKeyNames())
                        {
                            try
                            {
                                using (var guidKey = key.OpenSubKey(guid))
                                {
                                    // Check version subkeys
                                    foreach (var version in guidKey?.GetSubKeyNames() ?? new string[] { })
                                    {
                                        using (var verKey = guidKey.OpenSubKey(version))
                                        {
                                            // Check win32 path
                                            using (var win32Key = verKey?.OpenSubKey("0\\win32"))
                                            {
                                                var path = win32Key?.GetValue("")?.ToString() ?? "";
                                                if (path.ToLower().Contains("bkav"))
                                                {
                                                    key.DeleteSubKeyTree(guid, false);
                                                    Debug.WriteLine($"Deleted TypeLib: {guid}");
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteBkavFromTypeLib Error: {ex.Message}");
            }
        }

        private void DeleteShellIntegration()
        {
            try
            {
                // Delete shell context menu handlers
                string shellExPath = @"Software\Classes\*\shellex\ContextMenuHandlers";
                DeleteShellEntries(Registry.LocalMachine, shellExPath);

                // Delete file type shell handlers
                shellExPath = @"Software\Classes\lnkfile\shellex\ContextMenuHandlers";
                DeleteShellEntries(Registry.LocalMachine, shellExPath);

                // Also check Wow6432Node
                shellExPath = @"Software\Wow6432Node\Classes\*\shellex\ContextMenuHandlers";
                DeleteShellEntries(Registry.LocalMachine, shellExPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteShellIntegration Error: {ex.Message}");
            }
        }

        private void DeleteShellEntries(RegistryKey baseKey, string shellPath)
        {
            try
            {
                using (RegistryKey key = baseKey.OpenSubKey(shellPath, true))
                {
                    if (key != null)
                    {
                        foreach (var handlerName in key.GetSubKeyNames())
                        {
                            if (handlerName.ToLower().Contains("bkav"))
                            {
                                try
                                {
                                    key.DeleteSubKeyTree(handlerName, false);
                                    Debug.WriteLine($"Deleted shell handler: {handlerName}");
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteShellEntries Error: {ex.Message}");
            }
        }

        private void DeleteMuiCache()
        {
            try
            {
                // Delete from MuiCache
                string muiPath = @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\MuiCache";
                DeleteMuiCacheEntries(Registry.ClassesRoot, muiPath);
                DeleteMuiCacheEntries(Registry.CurrentUser, @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\MuiCache");

                // Also delete BagMRU entries
                string bagPath = @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\BagMRU";
                DeleteBagMRUEntries(Registry.ClassesRoot, bagPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteMuiCache Error: {ex.Message}");
            }
        }

        private void DeleteMuiCacheEntries(RegistryKey baseKey, string muiPath)
        {
            try
            {
                using (RegistryKey key = baseKey.OpenSubKey(muiPath, true))
                {
                    if (key != null)
                    {
                        foreach (var valueName in key.GetValueNames().ToList())
                        {
                            try
                            {
                                var value = key.GetValue(valueName)?.ToString() ?? "";
                                if (value.ToLower().Contains("bkav") || value.ToLower().Contains("uninstaller"))
                                {
                                    key.DeleteValue(valueName);
                                    Debug.WriteLine($"Deleted MuiCache entry: {valueName}");
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteMuiCacheEntries Error: {ex.Message}");
            }
        }

        private void DeleteBagMRUEntries(RegistryKey baseKey, string bagPath)
        {
            try
            {
                using (RegistryKey key = baseKey.OpenSubKey(bagPath, true))
                {
                    if (key != null)
                    {
                        // Recursively delete subkeys containing BKAV
                        DeleteBagMRURecursive(key, bagPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteBagMRUEntries Error: {ex.Message}");
            }
        }

        private void DeleteBagMRURecursive(RegistryKey key, string path)
        {
            try
            {
                foreach (var subKeyName in key.GetSubKeyNames().ToList())
                {
                    using (var subKey = key.OpenSubKey(subKeyName, true))
                    {
                        // Check for BKAV references in value
                        foreach (var valueName in subKey?.GetValueNames() ?? new string[] { })
                        {
                            var value = subKey?.GetValue(valueName)?.ToString() ?? "";
                            if (value.Contains("BKAV") || value.Contains("BkavPro"))
                            {
                                try
                                {
                                    key.DeleteSubKeyTree(subKeyName, false);
                                    Debug.WriteLine($"Deleted BagMRU entry: {subKeyName}");
                                    return;
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteBagMRURecursive Error: {ex.Message}");
            }
        }

        private void DeleteDownloadManagerEntries()
        {
            try
            {
                // Delete DownloadManager entries
                string dmPath = @"Software\DownloadManager";
                DeleteDownloadEntries(Registry.CurrentUser, dmPath);

                // Delete AppCompatFlags entries
                string compatPath = @"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Compatibility Assistant\Store";
                DeleteCompatFlagsEntries(Registry.CurrentUser, compatPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteDownloadManagerEntries Error: {ex.Message}");
            }
        }

        private void DeleteDownloadEntries(RegistryKey baseKey, string dmPath)
        {
            try
            {
                using (RegistryKey key = baseKey.OpenSubKey(dmPath, true))
                {
                    if (key != null)
                    {
                        // Delete specific subkeys
                        using (var subKey4 = key.OpenSubKey("4"))
                        {
                            if (subKey4 != null)
                            {
                                key.DeleteSubKeyTree("4", false);
                                Debug.WriteLine("Deleted DownloadManager entry: 4");
                            }
                        }

                        // Delete value entries containing BKAV
                        foreach (var valueName in key.GetValueNames().ToList())
                        {
                            var value = key.GetValue(valueName)?.ToString() ?? "";
                            if (value.ToLower().Contains("bkav") || value.ToLower().Contains("update.bkav"))
                            {
                                try
                                {
                                    key.DeleteValue(valueName);
                                    Debug.WriteLine($"Deleted DownloadManager value: {valueName}");
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteDownloadEntries Error: {ex.Message}");
            }
        }

        private void DeleteCompatFlagsEntries(RegistryKey baseKey, string compatPath)
        {
            try
            {
                using (RegistryKey key = baseKey.OpenSubKey(compatPath, true))
                {
                    if (key != null)
                    {
                        foreach (var valueName in key.GetValueNames().ToList())
                        {
                            try
                            {
                                if (valueName.ToLower().Contains("bkav") || valueName.ToLower().Contains("uninstaller"))
                                {
                                    key.DeleteValue(valueName);
                                    Debug.WriteLine($"Deleted AppCompatFlags entry: {valueName}");
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteCompatFlagsEntries Error: {ex.Message}");
            }
        }

        private void FinalRegistryCleanup()
        {
            try
            {
                Debug.WriteLine("Starting Final Registry Cleanup (Bước 7)...");

                // Comprehensive scan of all registry hives
                var registryPaths = new[]
                {
                    // HKEY_LOCAL_MACHINE paths
                    new { Key = Registry.LocalMachine, Path = @"Software" },
                    new { Key = Registry.LocalMachine, Path = @"Software\Wow6432Node" },
                    new { Key = Registry.LocalMachine, Path = @"Software\Classes" },
                    new { Key = Registry.LocalMachine, Path = @"Software\Wow6432Node\Classes" },
                    new { Key = Registry.LocalMachine, Path = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders" },
                    new { Key = Registry.LocalMachine, Path = @"Software\Microsoft\Shared Tools\MSEnv\Macros" },
                    new { Key = Registry.LocalMachine, Path = @"System\CurrentControlSet\Enum" },
                    new { Key = Registry.LocalMachine, Path = @"System\CurrentControlSet\Services" },

                    // HKEY_CURRENT_USER paths
                    new { Key = Registry.CurrentUser, Path = @"Software" },
                    new { Key = Registry.CurrentUser, Path = @"Software\Classes" },
                    new { Key = Registry.CurrentUser, Path = @"Software\Microsoft\Windows\CurrentVersion\Explorer\MountPoints2" },
                };

                foreach (var regPath in registryPaths)
                {
                    ComprehensiveCleanupPass(regPath.Key, regPath.Path);
                }

                Debug.WriteLine("Final Registry Cleanup completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FinalRegistryCleanup Error: {ex.Message}");
            }
        }

        private void ComprehensiveCleanupPass(RegistryKey baseKey, string searchPath)
        {
            try
            {
                using (RegistryKey key = baseKey.OpenSubKey(searchPath, true))
                {
                    if (key != null)
                    {
                        // Clean subkeys recursively
                        CleanRegistrySubkeysRecursive(key, searchPath);

                        // Clean values
                        foreach (var valueName in key.GetValueNames().ToList())
                        {
                            try
                            {
                                var value = key.GetValue(valueName)?.ToString() ?? "";
                                if (value.ToLower().Contains("bkav") || 
                                    value.ToLower().Contains("vsmart") ||
                                    value.ToLower().Contains("v-smart") ||
                                    value.ToLower().Contains("bka.exe"))
                                {
                                    key.DeleteValue(valueName);
                                    Debug.WriteLine($"Deleted final value: {searchPath}\\{valueName}");
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ComprehensiveCleanupPass Error for {searchPath}: {ex.Message}");
            }
        }

        private void CleanRegistrySubkeysRecursive(RegistryKey key, string path)
        {
            try
            {
                var subKeyNames = key.GetSubKeyNames().ToList();

                foreach (var subKeyName in subKeyNames)
                {
                    // Check if subkey name contains BKAV references
                    if (subKeyName.ToLower().Contains("bkav") || 
                        subKeyName.ToLower().Contains("vsmart") ||
                        subKeyName.ToLower().Contains("v-smart") ||
                        subKeyName.ToLower().Contains("bkavsystem") ||
                        subKeyName.ToLower().Contains("bkavutil"))
                    {
                        try
                        {
                            key.DeleteSubKeyTree(subKeyName, false);
                            Debug.WriteLine($"Deleted final subkey: {path}\\{subKeyName}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Failed to delete subkey {subKeyName}: {ex.Message}");
                        }
                    }
                    else
                    {
                        // Recursively check nested keys
                        try
                        {
                            using (var subKey = key.OpenSubKey(subKeyName, true))
                            {
                                if (subKey != null)
                                {
                                    CleanRegistrySubkeysRecursive(subKey, $"{path}\\{subKeyName}");
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CleanRegistrySubkeysRecursive Error: {ex.Message}");
            }
        }

        private void SearchAndDeleteAppPaths()
        {
            try
            {
                string appPathsKey = @"Software\Microsoft\Windows\CurrentVersion\App Paths";
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(appPathsKey, true))
                {
                    if (key != null)
                    {
                        foreach (var subKeyName in key.GetSubKeyNames())
                        {
                            if (subKeyName.ToLower().Contains("bkav") || 
                                subKeyName.ToLower().Contains("v-smart") ||
                                subKeyName.ToLower().Contains("vsmart"))
                            {
                                try
                                {
                                    key.DeleteSubKeyTree(subKeyName, false);
                                    Debug.WriteLine($"Deleted App Path: {subKeyName}");
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"Failed to delete App Path {subKeyName}: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SearchAndDeleteAppPaths Error: {ex.Message}");
            }
        }

        private void DeleteFileAssociations()
        {
            try
            {
                // Delete from HKLM\Software\Classes
                DeleteFromRegistry(Registry.LocalMachine, @"Software\Classes\.bkav");
                DeleteFromRegistry(Registry.LocalMachine, @"Software\Classes\.vsmart");

                // Search for BKAV-related file extensions
                SearchAndDeleteFromClasses(Registry.LocalMachine, @"Software\Classes");
                SearchAndDeleteFromClasses(Registry.CurrentUser, @"Software\Classes");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteFileAssociations Error: {ex.Message}");
            }
        }

        private void SearchAndDeleteFromClasses(RegistryKey baseKey, string classesPath)
        {
            try
            {
                using (RegistryKey key = baseKey.OpenSubKey(classesPath, true))
                {
                    if (key != null)
                    {
                        foreach (var subKeyName in key.GetSubKeyNames())
                        {
                            if (subKeyName.ToLower().Contains("bkav") || 
                                subKeyName.ToLower().Contains("vsmart"))
                            {
                                try
                                {
                                    key.DeleteSubKeyTree(subKeyName, false);
                                    Debug.WriteLine($"Deleted class association: {subKeyName}");
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SearchAndDeleteFromClasses Error: {ex.Message}");
            }
        }

        private void DeleteBkavServices()
        {
            try
            {
                string servicesPath = @"System\CurrentControlSet\Services";
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(servicesPath, true))
                {
                    if (key != null)
                    {
                        foreach (var serviceName in key.GetSubKeyNames())
                        {
                            if (serviceName.ToLower().Contains("bkav") || 
                                serviceName.ToLower().Contains("v-smart") ||
                                serviceName.ToLower().Contains("vsmart"))
                            {
                                try
                                {
                                    key.DeleteSubKeyTree(serviceName, false);
                                    Debug.WriteLine($"Deleted service: {serviceName}");
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"Failed to delete service {serviceName}: {ex.Message}");
                                }
                            }
                        }
                    }
                }

                // Also scan ControlSet001 and ControlSet002 for BKAV services
                DeleteBkavServicesFromPath(Registry.LocalMachine, @"System\ControlSet001\Services");
                DeleteBkavServicesFromPath(Registry.LocalMachine, @"System\ControlSet002\Services");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteBkavServices Error: {ex.Message}");
            }
        }

        private void DeleteScheduledTasks()
        {
            try
            {
                string tasksPath = @"Software\Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tree";
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(tasksPath, true))
                {
                    if (key != null)
                    {
                        foreach (var taskName in key.GetSubKeyNames())
                        {
                            if (taskName.ToLower().Contains("bkav") || 
                                taskName.ToLower().Contains("v-smart") ||
                                taskName.ToLower().Contains("vsmart"))
                            {
                                try
                                {
                                    key.DeleteSubKeyTree(taskName, false);
                                    Debug.WriteLine($"Deleted scheduled task: {taskName}");
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"Failed to delete task {taskName}: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteScheduledTasks Error: {ex.Message}");
            }
        }

        private void DeleteBkavServicesFromPath(RegistryKey baseKey, string servicesPath)
        {
            try
            {
                using (RegistryKey key = baseKey.OpenSubKey(servicesPath, true))
                {
                    if (key != null)
                    {
                        foreach (var serviceName in key.GetSubKeyNames())
                        {
                            if (serviceName.ToLower().Contains("bkav") ||
                                serviceName.ToLower().Contains("v-smart") ||
                                serviceName.ToLower().Contains("vsmart"))
                            {
                                try
                                {
                                    key.DeleteSubKeyTree(serviceName, false);
                                    Debug.WriteLine($"Deleted service from {servicesPath}: {serviceName}");
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"Failed to delete service {serviceName} from {servicesPath}: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteBkavServicesFromPath Error: {ex.Message}");
            }
        }

        private void DeleteRunEntries()
        {
            try
            {
                // Delete from HKLM Run
                DeleteRunFromKey(Registry.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\Run");
                DeleteRunFromKey(Registry.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\RunOnce");
                DeleteRunFromKey(Registry.LocalMachine, @"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Run");
                DeleteRunFromKey(Registry.LocalMachine, @"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\RunOnce");

                // Delete from HKCU Run
                DeleteRunFromKey(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Run");
                DeleteRunFromKey(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\RunOnce");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteRunEntries Error: {ex.Message}");
            }
        }

        private void DeleteRunFromKey(RegistryKey baseKey, string runPath)
        {
            try
            {
                using (RegistryKey key = baseKey.OpenSubKey(runPath, true))
                {
                    if (key != null)
                    {
                        foreach (var valueName in key.GetValueNames())
                        {
                            try
                            {
                                var value = key.GetValue(valueName)?.ToString().ToLower() ?? "";
                                if (value.Contains("bkav") || value.Contains("v-smart") || value.Contains("vsmart"))
                                {
                                    key.DeleteValue(valueName);
                                    Debug.WriteLine($"Deleted run entry: {valueName}");
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteRunFromKey Error: {ex.Message}");
            }
        }

        private void DeleteClassesAssociations()
        {
            try
            {
                // Delete BKAV-related class handlers
                string handlersPath = @"Software\Classes\CLSID";
                DeleteBkavFromPath(Registry.LocalMachine, handlersPath);

                // Delete ProgIDs
                string progIdsPath = @"Software\Classes";
                DeleteBkavFromPath(Registry.LocalMachine, progIdsPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteClassesAssociations Error: {ex.Message}");
            }
        }

        private void DeleteBkavFromPath(RegistryKey baseKey, string path)
        {
            try
            {
                using (RegistryKey key = baseKey.OpenSubKey(path, true))
                {
                    if (key != null)
                    {
                        foreach (var subKeyName in key.GetSubKeyNames().ToList())
                        {
                            if (subKeyName.ToLower().Contains("bkav") || 
                                subKeyName.ToLower().Contains("vsmart") ||
                                subKeyName.ToLower().Contains("bkavsystem"))
                            {
                                try
                                {
                                    key.DeleteSubKeyTree(subKeyName, false);
                                    Debug.WriteLine($"Deleted from {path}: {subKeyName}");
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteBkavFromPath Error: {ex.Message}");
            }
        }

        private void DeleteFromRegistry(RegistryKey baseKey, string subKeyPath)
        {
            try
            {
                using (RegistryKey key = baseKey.OpenSubKey(subKeyPath, true))
                {
                    if (key != null)
                    {
                        baseKey.DeleteSubKeyTree(subKeyPath, false);
                        Debug.WriteLine($"Deleted registry key: {subKeyPath}");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Debug.WriteLine($"Access denied to registry key: {subKeyPath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting registry key {subKeyPath}: {ex.Message}");
            }
        }

        private void SearchAndDeleteUninstallEntry()
        {
            try
            {
                // Search in HKLM Uninstall
                SearchUninstallKeys(Registry.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\Uninstall");
                SearchUninstallKeys(Registry.LocalMachine, @"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
                // Search in HKCU Uninstall
                SearchUninstallKeys(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Uninstall");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SearchAndDeleteUninstallEntry Error: {ex.Message}");
            }
        }

        private void SearchUninstallKeys(RegistryKey baseKey, string uninstallPath)
        {
            try
            {
                using (RegistryKey key = baseKey.OpenSubKey(uninstallPath, true))
                {
                    if (key != null)
                    {
                        foreach (string subKeyName in key.GetSubKeyNames())
                        {
                            if (subKeyName.ToLower().Contains("bkav") || subKeyName.ToLower().Contains("v-smart"))
                            {
                                try
                                {
                                    key.DeleteSubKeyTree(subKeyName, false);
                                    Debug.WriteLine($"Deleted uninstall entry: {subKeyName}");
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"Failed to delete {subKeyName}: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SearchUninstallKeys Error: {ex.Message}");
            }
        }

        private void DeleteStartupShortcuts()
        {
            try
            {
                string startupPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    @"Microsoft\Windows\Start Menu\Programs\Startup"
                );

                if (Directory.Exists(startupPath))
                {
                    foreach (var file in Directory.GetFiles(startupPath, "*.lnk"))
                    {
                        if (file.ToLower().Contains("bkav") || file.ToLower().Contains("v-smart"))
                        {
                            try
                            {
                                File.Delete(file);
                                Debug.WriteLine($"Deleted startup shortcut: {file}");
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Failed to delete startup shortcut {file}: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteStartupShortcuts Error: {ex.Message}");
            }
        }

        private void CleanEnvironmentVariables()
        {
            try
            {
                // Remove BKAV from PATH environment variable
                string pathValue = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
                string[] pathEntries = pathValue.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                var cleanedPaths = pathEntries
                    .Where(p => !p.ToLower().Contains("bkav") && !p.ToLower().Contains("v-smart"))
                    .ToArray();

                if (cleanedPaths.Length < pathEntries.Length)
                {
                    string cleanedPath = string.Join(";", cleanedPaths);
                    try
                    {
                        Environment.SetEnvironmentVariable("PATH", cleanedPath, EnvironmentVariableTarget.User);
                        Debug.WriteLine("Cleaned PATH environment variable");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to update PATH: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CleanEnvironmentVariables Error: {ex.Message}");
            }
        }

        private void lblwin_Click(object sender, EventArgs e)
        {
            // Show menu with options
            using (ContextMenuStrip menu = new ContextMenuStrip())
            {
                // Option 1: Registry Finder
                var finderItem = new ToolStripMenuItem("🔍 Registry Finder - Scan All BKAV Entries");
                finderItem.Click += (s, args) => OpenRegistryFinder();
                menu.Items.Add(finderItem);

                menu.Items.Add(new ToolStripSeparator());

                // Option 2: Registry Comparator
                var comparatorItem = new ToolStripMenuItem("📊 Registry Comparator - Before/After Analysis");
                comparatorItem.Click += (s, args) => OpenRegistryComparator();
                menu.Items.Add(comparatorItem);

                menu.Show(System.Windows.Forms.Cursor.Position);
            }
        }

        private void OpenRegistryFinder()
        {
            using (var finder = new RegistryFinderForm())
            {
                finder.ShowDialog();
            }
        }

        private void OpenRegistryComparator()
        {
            using (var comparator = new RegistryComparatorForm())
            {
                comparator.ShowDialog();
            }
        }
    }
}
