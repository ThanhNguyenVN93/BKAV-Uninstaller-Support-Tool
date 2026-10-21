using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        // ── Constants ────────────────────────────────────────────────────────────
        private const string BKAV_PRO_FOLDER    = "BkavPro";
        private const string BKAV_CORP_FOLDER   = "Bkav Corporation";
        private const string BKAV_UNINSTALLER   = "BkavUnst.exe";
        private const string BKAV_NOT_INSTALLED = "Bạn đang không dùng BKAV!";

        /// <summary>
        /// Tập hợp keywords dùng để nhận diện entry liên quan BKAV.
        /// Thêm biến thể thực tế: bkavsrv, bkavss, bkavwsc, bkav2, v-safe, bkav pro.
        /// </summary>
        internal static readonly string[] BkavKeywords =
        {
            "bkav",
            "v-smart",
            "vsmart",
            "bkavsystem",
            "bkavutil",
            "bka.exe",
            "bkavsrv",       // service BkavSrv
            "bkavss",        // service BkavSS
            "bkavwsc",       // Windows Security Center hook
            "bkav2",         // variant tên file cũ
            "v-safe",        // thư viện scan của BKAV
            "bkav pro",      // display name trong Uninstall
            "bkaventerprise" // enterprise variant
        };

        // ── Button colours ───────────────────────────────────────────────────────
        private static readonly System.Drawing.Color BTN_UNINSTALL_NORMAL = System.Drawing.Color.FromArgb(40,  167, 69);
        private static readonly System.Drawing.Color BTN_UNINSTALL_HOVER  = System.Drawing.Color.FromArgb(28,  140, 80);
        private static readonly System.Drawing.Color BTN_DELETE_NORMAL    = System.Drawing.Color.FromArgb(220, 53,  69);
        private static readonly System.Drawing.Color BTN_DELETE_HOVER     = System.Drawing.Color.FromArgb(200, 40,  55);
        private static readonly System.Drawing.Color BTN_TOOLS_NORMAL     = System.Drawing.Color.FromArgb(240, 240, 240);
        private static readonly System.Drawing.Color BTN_TOOLS_HOVER      = System.Drawing.Color.FromArgb(220, 220, 220);

        // ── State ────────────────────────────────────────────────────────────────
        private string _bkavProPath;
        private string _bkavCorpPath;

        // ── Registry views ──────────────────────────────────────────────────────
        // Mở tường minh view "gốc" (64-bit trên Windows 64-bit) để các lệnh Registry.LocalMachine/
        // Registry.ClassesRoot không bị WOW64 redirect sang Wow6432Node, bỏ sót entry gốc
        // (CLSID/AppID dạng GUID...). Trên Windows 32-bit thì RegistryView.Registry64 không
        // tồn tại (ném PlatformNotSupportedException) nên phải kiểm tra OS trước.
        private static readonly RegistryKey HKLM64 = OpenNativeBaseKey(RegistryHive.LocalMachine);
        private static readonly RegistryKey HKCR64 = OpenNativeBaseKey(RegistryHive.ClassesRoot);

        private static RegistryKey OpenNativeBaseKey(RegistryHive hive)
        {
            var view = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
            return RegistryKey.OpenBaseKey(hive, view);
        }

        // ── Constructor ──────────────────────────────────────────────────────────
        public Form1()
        {
            InitializeComponent();
        }

        // ════════════════════════════════════════════════════════════════════════
        //  INITIALISATION
        // ════════════════════════════════════════════════════════════════════════

        private void Form1_Load(object sender, EventArgs e)
        {
            lblwin.Text = GetOSInfo();
            ApplyButtonStyles();
            InitializeBkavPaths();
            CheckForUpdatesInBackground(showUpToDateMessage: false);

            bool uninstallerExists = File.Exists(Path.Combine(_bkavProPath, BKAV_UNINSTALLER));
            bool hasLeftovers      = !uninstallerExists && HasBkavLeftovers();

            if (uninstallerExists)
            {
                txtinsdrect.Text = Path.Combine(_bkavProPath, BKAV_UNINSTALLER);
                btnunin.Enabled  = true;
                btnunin.PerformClick();
            }
            else if (hasLeftovers)
            {
                txtinsdrect.Text  = "Phát hiện tàn dư BKAV. Nhấn 'Xóa Tàn Dư' để xóa.";
                btnremove.Enabled = true;
            }
            else
            {
                MessageBox.Show(BKAV_NOT_INSTALLED, "Thông Báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Environment.Exit(0);
            }
        }

        private void ApplyButtonStyles()
        {
            SetButtonHoverEffect(btnunin,   BTN_UNINSTALL_NORMAL, BTN_UNINSTALL_HOVER);
            SetButtonHoverEffect(btnremove, BTN_DELETE_NORMAL,    BTN_DELETE_HOVER);
            SetButtonHoverEffect(btnTools,  BTN_TOOLS_NORMAL,     BTN_TOOLS_HOVER);
        }

        private static void SetButtonHoverEffect(Button btn,
            System.Drawing.Color normalColor, System.Drawing.Color hoverColor)
        {
            btn.BackColor   = normalColor;
            btn.MouseEnter += (s, e) => btn.BackColor = hoverColor;
            btn.MouseLeave += (s, e) => btn.BackColor = normalColor;
        }

        private static string GetOSInfo()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher(
                    "SELECT Caption,OSArchitecture FROM Win32_OperatingSystem"))
                using (var results = searcher.Get())
                {
                    foreach (ManagementObject obj in results)
                    {
                        using (obj)
                        {
                            return $"{obj["Caption"]} - {obj["OSArchitecture"]}"
                                .Replace("NT 5.1.2600", "XP")
                                .Replace("NT 5.2.3790", "Server 2003");
                        }
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine($"GetOSInfo: {ex.Message}"); }
            return "Không xác định được hệ điều hành";
        }

        private void InitializeBkavPaths()
        {
            string pf86    = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            _bkavProPath   = Path.Combine(pf86, BKAV_PRO_FOLDER);
            _bkavCorpPath  = Path.Combine(pf86, BKAV_CORP_FOLDER);
        }

        // ════════════════════════════════════════════════════════════════════════
        //  DETECTION
        // ════════════════════════════════════════════════════════════════════════

        private bool HasBkavLeftovers()
        {
            return Directory.Exists(_bkavProPath)
                || Directory.Exists(_bkavCorpPath)
                || HasBkavRegistryEntries()
                || HasBkavStartupShortcuts();
        }

        private bool HasBkavRegistryEntries()
        {
            try
            {
                // 1. Direct well-known keys
                foreach (var k in new[] { @"Software\BKAV", @"Software\Wow6432Node\BKAV" })
                    using (var key = Registry.LocalMachine.OpenSubKey(k))
                        if (key != null) return true;

                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\BKAV"))
                    if (key != null) return true;

                // 2. Services
                if (AnySubKeyMatchesKeyword(Registry.LocalMachine,
                    @"System\CurrentControlSet\Services")) return true;

                // 3. Uninstall entries (HKLM 32 + 64 bit)
                foreach (var p in new[]
                {
                    @"Software\Microsoft\Windows\CurrentVersion\Uninstall",
                    @"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
                })
                    if (AnySubKeyMatchesKeyword(Registry.LocalMachine, p)) return true;
            }
            catch (Exception ex) { Debug.WriteLine($"HasBkavRegistryEntries: {ex.Message}"); }
            return false;
        }

        /// <summary>Returns true if any direct child subkey name matches a BKAV keyword.</summary>
        private static bool AnySubKeyMatchesKeyword(RegistryKey baseKey, string path)
        {
            try
            {
                using (var key = baseKey.OpenSubKey(path))
                    if (key != null)
                        foreach (var name in key.GetSubKeyNames())
                            if (ContainsBkavKeyword(name)) return true;
            }
            catch { }
            return false;
        }

        private static bool HasBkavStartupShortcuts()
        {
            try
            {
                string startupPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    @"Microsoft\Windows\Start Menu\Programs\Startup");

                if (Directory.Exists(startupPath))
                    foreach (var file in Directory.GetFiles(startupPath, "*.lnk"))
                        if (ContainsBkavKeyword(Path.GetFileName(file))) return true;
            }
            catch (Exception ex) { Debug.WriteLine($"HasBkavStartupShortcuts: {ex.Message}"); }
            return false;
        }

        // ════════════════════════════════════════════════════════════════════════
        //  BUTTON HANDLERS
        // ════════════════════════════════════════════════════════════════════════

        private void btnunin_Click(object sender, EventArgs e)
        {
            try
            {
                string path = Path.Combine(_bkavProPath, BKAV_UNINSTALLER);
                if (File.Exists(path))
                {
                    var process = Process.Start(path);
                    btnunin.Enabled   = false;
                    btnremove.Enabled = true;
                    // Process.Start() chỉ MỞ trình gỡ cài đặt gốc, không chờ nó chạy xong —
                    // chưa thể coi là "đã gỡ" ngay lúc này (BkavUnst.exe còn hiện khảo sát,
                    // captcha, hỏi xác nhận... trước khi thực sự gỡ). Chờ nó thoát ở luồng nền
                    // rồi mới cập nhật trạng thái cho đúng.
                    txtinsdrect.Text = "Đang chạy trình gỡ cài đặt BKAV gốc (BkavUnst.exe)...";

                    if (process != null)
                    {
                        ThreadPool.QueueUserWorkItem(_ =>
                        {
                            try { process.WaitForExit(); } catch { }
                            if (!this.IsDisposed)
                                this.Invoke(new Action(() =>
                                    txtinsdrect.Text =
                                        "Đã đóng trình gỡ cài đặt BKAV gốc. Nhấn 'Xóa Tàn Dư' để xóa phần còn lại."));
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnremove_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(_bkavProPath)
                && !Directory.Exists(_bkavCorpPath)
                && !HasBkavRegistryEntries()
                && !HasBkavStartupShortcuts())
            {
                MessageBox.Show(BKAV_NOT_INSTALLED, "Thông Báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Environment.Exit(0);
                return;
            }

            if (MessageBox.Show(
                    "Điều này sẽ xóa:\n" +
                    "• Các thư mục BKAV\n" +
                    "• Các entry Registry của BKAV\n" +
                    "• Các shortcut khởi động\n" +
                    "• Biến môi trường PATH của BKAV\n\n" +
                    "Bạn có chắc chắn?",
                    "Xác Nhận Xóa",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            btnremove.Enabled = false;

            var progressForm = new ProgressForm("Đang xóa toàn bộ tàn dư BKAV...");
            progressForm.Show(this);
            Application.DoEvents();

            ThreadPool.QueueUserWorkItem(_ =>
            {
                Exception workerException = null;
                try
                {
                    Report(progressForm, "Xóa thư mục BKAV...");
                    DeleteFoldersViaCMD();

                    Report(progressForm, "Xóa Registry entries...");
                    DeleteBkavRegistry();

                    Report(progressForm, "Xóa startup shortcuts...");
                    DeleteStartupShortcuts();

                    Report(progressForm, "Dọn dẹp environment variables...");
                    CleanEnvironmentVariables();
                }
                catch (Exception ex) { workerException = ex; }
                finally
                {
                    this.Invoke(new Action(() =>
                    {
                        progressForm.Close();
                        progressForm.Dispose();

                        if (workerException != null)
                        {
                            MessageBox.Show($"Lỗi: {workerException.Message}", "Lỗi",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            btnremove.Enabled = true;
                        }
                        else
                        {
                            MessageBox.Show(
                                "Xóa tàn dư BKAV thành công!\n\nHãy khởi động lại máy để hoàn tất.",
                                "Thành Công",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                            Application.Exit();
                        }
                    }));
                }
            });
        }

        private void Report(ProgressForm form, string message)
        {
            if (form.InvokeRequired)
                form.Invoke(new Action(() => form.UpdateStatus(message)));
            else
                form.UpdateStatus(message);
        }

        // ════════════════════════════════════════════════════════════════════════
        //  FOLDER DELETION
        // ════════════════════════════════════════════════════════════════════════

        private void DeleteFoldersViaCMD()
        {
            try
            {
                var args = new List<string>();
                if (Directory.Exists(_bkavProPath))  args.Add($"rd /s /q \"{_bkavProPath}\"");
                if (Directory.Exists(_bkavCorpPath)) args.Add($"rd /s /q \"{_bkavCorpPath}\"");
                if (args.Count == 0) return;

                using (var p = Process.Start(new ProcessStartInfo
                {
                    FileName        = "cmd.exe",
                    Arguments       = "/C " + string.Join(" & ", args),
                    UseShellExecute = false,
                    CreateNoWindow  = true,
                    WindowStyle     = ProcessWindowStyle.Hidden
                }))
                    p?.WaitForExit(30_000);
            }
            catch (Exception ex) { Debug.WriteLine($"DeleteFoldersViaCMD: {ex.Message}"); }
        }

        // ════════════════════════════════════════════════════════════════════════
        //  REGISTRY CLEANUP — ORCHESTRATOR
        // ════════════════════════════════════════════════════════════════════════

        private void DeleteBkavRegistry()
        {
            // 1. Direct well-known keys
            DeleteFromRegistry(Registry.LocalMachine, @"Software\BKAV");
            DeleteFromRegistry(Registry.LocalMachine, @"Software\Wow6432Node\BKAV");
            DeleteFromRegistry(Registry.CurrentUser,  @"Software\BKAV");
            DeleteFromRegistry(Registry.LocalMachine,
                @"Software\Microsoft\Windows NT\CurrentVersion\Drivers32");

            // 2. Uninstall entries
            SearchAndDeleteUninstallEntry();

            // 3. App Paths
            SearchAndDeleteAppPaths();

            // 4. File Associations
            DeleteFileAssociations();

            // 5. Services (CurrentControlSet + ControlSet001/002)
            DeleteBkavServices();

            // 6. Scheduled Tasks
            DeleteScheduledTasks();

            // 7. Run / RunOnce (HKLM + HKCU, 32+64 bit)
            DeleteRunEntries();

            // 8. Classes associations
            DeleteClassesAssociations();

            // 9. AppID entries
            DeleteAppIDEntries();

            // 10. CLSID entries (default value, InProcServer32, LocalServer32)
            DeleteCLSIDEntries();

            // 11. TypeLib entries
            DeleteTypeLibEntries();

            // 12. Shell context-menu integration
            DeleteShellIntegration();

            // 13. MUI / BagMRU cache
            DeleteMuiCache();

            // 14. DownloadManager / AppCompatFlags
            DeleteDownloadManagerEntries();

            // 15. Image File Execution Options hijack
            DeleteIfeoEntries();

            // 16. SafeBoot driver entries
            DeleteSafeBootEntries();

            // 17. Winlogon Notify hooks
            DeleteWinlogonNotify();

            // 18. MSI Installer records
            DeleteInstallerRecords();

            // 19. Final comprehensive pass
            FinalRegistryCleanup();

            // 20. Windows Security Center AV/Firewall provider registration
            DeleteSecurityCenterProviders();

            // 21. Shell Extensions "Approved" list (value name = CLSID GUID)
            DeleteBkavValues(HKLM64, @"Software\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved");

            // 22. Key rời nằm trực tiếp dưới HKLM\SYSTEM (vd: SYSTEM\BkavSR)
            DeleteBkavSubkeys(Registry.LocalMachine, @"SYSTEM");

            // 23. Dọn tên nhóm BKAV khỏi ServiceGroupOrder\List (REG_MULTI_SZ)
            CleanServiceGroupOrder(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\ServiceGroupOrder");
            CleanServiceGroupOrder(Registry.LocalMachine, @"SYSTEM\ControlSet001\Control\ServiceGroupOrder");
        }

        // ════════════════════════════════════════════════════════════════════════
        //  REGISTRY HELPERS
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>Case-insensitive check against BkavKeywords.</summary>
        internal static bool ContainsBkavKeyword(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            foreach (var kw in BkavKeywords)
                if (value.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            return false;
        }

        private static void DeleteFromRegistry(RegistryKey baseKey, string subKeyPath)
        {
            try
            {
                using (var key = baseKey.OpenSubKey(subKeyPath, writable: true))
                    if (key == null) return;
                baseKey.DeleteSubKeyTree(subKeyPath, throwOnMissingSubKey: false);
                Debug.WriteLine($"Deleted: {subKeyPath}");
            }
            catch (UnauthorizedAccessException) { Debug.WriteLine($"Access denied: {subKeyPath}"); }
            catch (Exception ex)               { Debug.WriteLine($"Error deleting {subKeyPath}: {ex.Message}"); }
        }

        /// <summary>Delete direct child subkeys whose name matches a BKAV keyword.</summary>
        private static void DeleteBkavSubkeys(RegistryKey baseKey, string path)
        {
            try
            {
                using (var key = baseKey.OpenSubKey(path, writable: true))
                {
                    if (key == null) return;
                    foreach (var name in key.GetSubKeyNames().ToArray())
                    {
                        if (!ContainsBkavKeyword(name)) continue;
                        try
                        {
                            key.DeleteSubKeyTree(name, throwOnMissingSubKey: false);
                            Debug.WriteLine($"Deleted subkey: {path}\\{name}");
                        }
                        catch (Exception ex) { Debug.WriteLine($"Failed subkey {name}: {ex.Message}"); }
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine($"DeleteBkavSubkeys({path}): {ex.Message}"); }
        }

        /// <summary>Delete values whose string data matches a BKAV keyword.</summary>
        private static void DeleteBkavValues(RegistryKey baseKey, string path)
        {
            try
            {
                using (var key = baseKey.OpenSubKey(path, writable: true))
                {
                    if (key == null) return;
                    foreach (var valueName in key.GetValueNames().ToArray())
                    {
                        try
                        {
                            var data = key.GetValue(valueName)?.ToString() ?? string.Empty;
                            if (!ContainsBkavKeyword(data)) continue;
                            key.DeleteValue(valueName, throwOnMissingValue: false);
                            Debug.WriteLine($"Deleted value: {path}\\{valueName}");
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine($"DeleteBkavValues({path}): {ex.Message}"); }
        }

        // ════════════════════════════════════════════════════════════════════════
        //  REGISTRY CLEANUP — STEP IMPLEMENTATIONS
        // ════════════════════════════════════════════════════════════════════════

        private static void SearchAndDeleteUninstallEntry()
        {
            foreach (var p in new[]
            {
                @"Software\Microsoft\Windows\CurrentVersion\Uninstall",
                @"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            })
            {
                DeleteBkavSubkeys(Registry.LocalMachine, p);
                DeleteBkavSubkeys(Registry.CurrentUser,  p);
            }
        }

        private static void SearchAndDeleteAppPaths()
        {
            DeleteBkavSubkeys(Registry.LocalMachine,
                @"Software\Microsoft\Windows\CurrentVersion\App Paths");
            DeleteBkavSubkeys(Registry.CurrentUser,
                @"Software\Microsoft\Windows\CurrentVersion\App Paths");
        }

        private static void DeleteFileAssociations()
        {
            DeleteFromRegistry(Registry.LocalMachine, @"Software\Classes\.bkav");
            DeleteFromRegistry(Registry.LocalMachine, @"Software\Classes\.vsmart");
            DeleteBkavSubkeys(Registry.LocalMachine, @"Software\Classes");
            DeleteBkavSubkeys(Registry.CurrentUser,  @"Software\Classes");
        }

        private static void DeleteBkavServices()
        {
            foreach (var p in new[]
            {
                @"System\CurrentControlSet\Services",
                @"System\ControlSet001\Services",
                @"System\ControlSet002\Services"
            })
                DeleteBkavSubkeys(Registry.LocalMachine, p);
        }

        private static void DeleteScheduledTasks()
        {
            DeleteBkavSubkeys(Registry.LocalMachine,
                @"Software\Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tree");
        }

        private static void DeleteRunEntries()
        {
            foreach (var p in new[]
            {
                @"Software\Microsoft\Windows\CurrentVersion\Run",
                @"Software\Microsoft\Windows\CurrentVersion\RunOnce",
                @"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Run",
                @"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\RunOnce"
            })
            {
                DeleteBkavValues(Registry.LocalMachine, p);
                DeleteBkavValues(Registry.CurrentUser,  p);
            }
        }

        private static void DeleteClassesAssociations()
        {
            DeleteBkavSubkeys(Registry.LocalMachine, @"Software\Classes\CLSID");
            DeleteBkavSubkeys(Registry.LocalMachine, @"Software\Classes");
        }

        private static void DeleteAppIDEntries()
        {
            // AppID con thường đặt tên theo GUID (không chứa "bkav") — so theo tên subkey
            // là chưa đủ, cần kiểm tra thêm giá trị bên trong (@ hoặc "LocalService").
            foreach (var p in new[]
            {
                @"AppID",                                   // HKCR (view 64-bit)
                @"Software\Classes\AppID",                   // HKLM (view 64-bit)
                @"Software\Wow6432Node\Classes\AppID",        // HKLM (32-bit view, path chuẩn)
                @"Software\Classes\WOW6432Node\AppID"         // HKLM — path bị BKAV ghi đảo thứ tự
            })
            {
                var hive = p == "AppID" ? HKCR64 : HKLM64;
                DeleteBkavSubkeys(hive, p);
                DeleteSubkeysWhereAnyValueMatches(hive, p);
            }
        }

        private static void DeleteCLSIDEntries()
        {
            SearchAndDeleteCLSIDs(HKCR64,             @"CLSID");
            SearchAndDeleteCLSIDs(HKLM64,              @"Software\Classes\CLSID");
            SearchAndDeleteCLSIDs(Registry.LocalMachine, @"Software\Wow6432Node\Classes\CLSID");
        }

        private static void DeleteSecurityCenterProviders()
        {
            DeleteSubkeysWhereAnyValueMatches(HKLM64, @"Software\Microsoft\Security Center\Provider\Av");
            DeleteSubkeysWhereAnyValueMatches(HKLM64, @"Software\Microsoft\Security Center\Provider\Fw");
        }

        /// <summary>Xóa subkey con nếu TÊN khớp keyword, hoặc bất kỳ value (kể cả giá trị mặc định) bên trong nó chứa keyword.</summary>
        private static void DeleteSubkeysWhereAnyValueMatches(RegistryKey baseKey, string path)
        {
            try
            {
                using (var key = baseKey.OpenSubKey(path, writable: true))
                {
                    if (key == null) return;
                    foreach (var name in key.GetSubKeyNames().ToArray())
                    {
                        try
                        {
                            bool shouldDelete = ContainsBkavKeyword(name);
                            if (!shouldDelete)
                                using (var sub = key.OpenSubKey(name))
                                {
                                    if (sub != null)
                                    {
                                        if (ContainsBkavKeyword(sub.GetValue("")?.ToString()))
                                            shouldDelete = true;
                                        else
                                            foreach (var vn in sub.GetValueNames())
                                                if (ContainsBkavKeyword(sub.GetValue(vn)?.ToString()))
                                                { shouldDelete = true; break; }
                                    }
                                }

                            if (shouldDelete)
                            {
                                key.DeleteSubKeyTree(name, throwOnMissingSubKey: false);
                                Debug.WriteLine($"Deleted (by value): {path}\\{name}");
                            }
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine($"DeleteSubkeysWhereAnyValueMatches({path}): {ex.Message}"); }
        }

        /// <summary>Lọc các phần tử chứa keyword BKAV ra khỏi một giá trị REG_MULTI_SZ (vd: ServiceGroupOrder\List).</summary>
        private static void CleanServiceGroupOrder(RegistryKey baseKey, string path)
        {
            try
            {
                using (var key = baseKey.OpenSubKey(path, writable: true))
                {
                    if (key == null) return;
                    if (key.GetValueKind("List") != RegistryValueKind.MultiString) return;

                    var list = key.GetValue("List") as string[];
                    if (list == null) return;

                    var cleaned = list.Where(s => !ContainsBkavKeyword(s)).ToArray();
                    if (cleaned.Length == list.Length) return;

                    key.SetValue("List", cleaned, RegistryValueKind.MultiString);
                    Debug.WriteLine($"Cleaned ServiceGroupOrder\\List ({path}): removed {list.Length - cleaned.Length} entries");
                }
            }
            catch (Exception ex) { Debug.WriteLine($"CleanServiceGroupOrder({path}): {ex.Message}"); }
        }

        /// <summary>
        /// Xóa CLSID entry nếu default value, InProcServer32, hoặc LocalServer32 chứa BKAV keyword.
        /// </summary>
        private static void SearchAndDeleteCLSIDs(RegistryKey baseKey, string clsidPath)
        {
            try
            {
                using (var key = baseKey.OpenSubKey(clsidPath, writable: true))
                {
                    if (key == null) return;
                    foreach (var guid in key.GetSubKeyNames().ToArray())
                    {
                        try
                        {
                            bool shouldDelete = false;
                            using (var subKey = key.OpenSubKey(guid))
                            {
                                if (subKey == null) continue;

                                // (a) Default value
                                if (ContainsBkavKeyword(subKey.GetValue("")?.ToString()))
                                    shouldDelete = true;

                                // (b) InProcServer32 (in-process COM DLL)
                                if (!shouldDelete)
                                    using (var srv = subKey.OpenSubKey("InProcServer32"))
                                        if (ContainsBkavKeyword(srv?.GetValue("")?.ToString()))
                                            shouldDelete = true;

                                // (c) LocalServer32 (out-of-process COM EXE) ← NEW
                                if (!shouldDelete)
                                    using (var srv = subKey.OpenSubKey("LocalServer32"))
                                        if (ContainsBkavKeyword(srv?.GetValue("")?.ToString()))
                                            shouldDelete = true;

                                // (d) ProgID / VersionIndependentProgID ← NEW
                                if (!shouldDelete)
                                    foreach (var progIdKey in new[] { "ProgID", "VersionIndependentProgID" })
                                        using (var pid = subKey.OpenSubKey(progIdKey))
                                            if (ContainsBkavKeyword(pid?.GetValue("")?.ToString()))
                                            { shouldDelete = true; break; }
                            }

                            if (shouldDelete)
                            {
                                key.DeleteSubKeyTree(guid, throwOnMissingSubKey: false);
                                Debug.WriteLine($"Deleted CLSID: {clsidPath}\\{guid}");
                            }
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine($"SearchAndDeleteCLSIDs: {ex.Message}"); }
        }

        private static void DeleteTypeLibEntries()
        {
            DeleteBkavFromTypeLib(Registry.LocalMachine, @"Software\Classes\TypeLib");
            DeleteBkavFromTypeLib(Registry.LocalMachine, @"Software\Wow6432Node\Classes\TypeLib");
        }

        private static void DeleteBkavFromTypeLib(RegistryKey baseKey, string typeLibPath)
        {
            try
            {
                using (var key = baseKey.OpenSubKey(typeLibPath, writable: true))
                {
                    if (key == null) return;
                    foreach (var guid in key.GetSubKeyNames().ToArray())
                    {
                        try
                        {
                            bool shouldDelete = false;
                            using (var guidKey = key.OpenSubKey(guid))
                            {
                                if (guidKey == null) continue;
                                foreach (var version in guidKey.GetSubKeyNames())
                                {
                                    using (var verKey = guidKey.OpenSubKey(version))
                                    using (var win32Key = verKey?.OpenSubKey(@"0\win32"))
                                    {
                                        if (ContainsBkavKeyword(win32Key?.GetValue("")?.ToString()))
                                        { shouldDelete = true; break; }
                                    }
                                }
                            }
                            if (shouldDelete)
                            {
                                key.DeleteSubKeyTree(guid, throwOnMissingSubKey: false);
                                Debug.WriteLine($"Deleted TypeLib: {guid}");
                            }
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine($"DeleteBkavFromTypeLib: {ex.Message}"); }
        }

        private static void DeleteShellIntegration()
        {
            foreach (var classRoot in new[]
            {
                @"Software\Classes",
                @"Software\Wow6432Node\Classes"
            })
            {
                try
                {
                    using (var classesKey = Registry.LocalMachine.OpenSubKey(classRoot))
                    {
                        if (classesKey == null) continue;
                        foreach (var typeName in classesKey.GetSubKeyNames())
                        {
                            try
                            {
                                DeleteBkavSubkeys(Registry.LocalMachine,
                                    $@"{classRoot}\{typeName}\shellex\ContextMenuHandlers");
                            }
                            catch { }
                        }
                    }
                }
                catch (Exception ex) { Debug.WriteLine($"DeleteShellIntegration ({classRoot}): {ex.Message}"); }
            }

            // "*" wildcard file type (all files) ← NEW
            DeleteBkavSubkeys(Registry.LocalMachine,
                @"Software\Classes\*\shellex\ContextMenuHandlers");
            DeleteBkavSubkeys(Registry.LocalMachine,
                @"Software\Classes\lnkfile\shellex\ContextMenuHandlers");
        }

        private static void DeleteMuiCache()
        {
            const string muiPath = @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\MuiCache";
            DeleteBkavValues(Registry.ClassesRoot, muiPath);
            DeleteBkavValues(Registry.CurrentUser,  muiPath);

            const string bagPath = @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\BagMRU";
            DeleteBagMRUEntries(Registry.ClassesRoot, bagPath);
        }

        private static void DeleteBagMRUEntries(RegistryKey baseKey, string bagPath)
        {
            try
            {
                using (var key = baseKey.OpenSubKey(bagPath, writable: true))
                    if (key != null) DeleteBagMRURecursive(key);
            }
            catch (Exception ex) { Debug.WriteLine($"DeleteBagMRUEntries: {ex.Message}"); }
        }

        private static void DeleteBagMRURecursive(RegistryKey key)
        {
            try
            {
                foreach (var subKeyName in key.GetSubKeyNames().ToArray())
                {
                    try
                    {
                        bool shouldDelete = false;
                        using (var subKey = key.OpenSubKey(subKeyName))
                        {
                            if (subKey == null) continue;
                            foreach (var vn in subKey.GetValueNames())
                                if (ContainsBkavKeyword(subKey.GetValue(vn)?.ToString()))
                                { shouldDelete = true; break; }
                        }

                        if (shouldDelete)
                        {
                            key.DeleteSubKeyTree(subKeyName, throwOnMissingSubKey: false);
                            Debug.WriteLine($"Deleted BagMRU: {subKeyName}");
                        }
                        else
                        {
                            using (var subKey = key.OpenSubKey(subKeyName, writable: true))
                                if (subKey != null) DeleteBagMRURecursive(subKey);
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex) { Debug.WriteLine($"DeleteBagMRURecursive: {ex.Message}"); }
        }

        private static void DeleteDownloadManagerEntries()
        {
            DeleteBkavValues(Registry.CurrentUser, @"Software\DownloadManager");

            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\DownloadManager\4"))
                {
                    if (key != null)
                    {
                        bool hasBkav = key.GetValueNames()
                            .Any(vn => ContainsBkavKeyword(key.GetValue(vn)?.ToString()));
                        if (hasBkav)
                            DeleteFromRegistry(Registry.CurrentUser, @"Software\DownloadManager\4");
                    }
                }
            }
            catch { }

            DeleteBkavValues(Registry.CurrentUser,
                @"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Compatibility Assistant\Store");
        }

        // ── NEW: Image File Execution Options (IFEO) hijack ──────────────────────
        private static void DeleteIfeoEntries()
        {
            DeleteBkavSubkeys(Registry.LocalMachine,
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options");
        }

        // ── NEW: SafeBoot driver entries ─────────────────────────────────────────
        private static void DeleteSafeBootEntries()
        {
            foreach (var p in new[]
            {
                @"SYSTEM\CurrentControlSet\Control\SafeBoot\Minimal",
                @"SYSTEM\CurrentControlSet\Control\SafeBoot\Network"
            })
                DeleteBkavSubkeys(Registry.LocalMachine, p);
        }

        // ── NEW: Winlogon Notify hooks ────────────────────────────────────────────
        private static void DeleteWinlogonNotify()
        {
            DeleteBkavSubkeys(Registry.LocalMachine,
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\Notify");
            DeleteBkavValues(Registry.LocalMachine,
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon");
        }

        // ── NEW: MSI Installer records ───────────────────────────────────────────
        private static void DeleteInstallerRecords()
        {
            // Products / Features: iterate GUIDs, check DisplayName value
            foreach (var p in new[]
            {
                @"Software\Classes\Installer\Products",
                @"Software\Classes\Installer\Features"
            })
            {
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(p, writable: true))
                    {
                        if (key == null) continue;
                        foreach (var guid in key.GetSubKeyNames().ToArray())
                        {
                            try
                            {
                                using (var sub = key.OpenSubKey(guid))
                                {
                                    if (sub == null) continue;
                                    var name = sub.GetValue("ProductName")?.ToString()
                                            ?? sub.GetValue("")?.ToString()
                                            ?? string.Empty;
                                    if (!ContainsBkavKeyword(name)) continue;
                                }
                                key.DeleteSubKeyTree(guid, throwOnMissingSubKey: false);
                                Debug.WriteLine($"Deleted Installer record: {p}\\{guid}");
                            }
                            catch { }
                        }
                    }
                }
                catch (Exception ex) { Debug.WriteLine($"DeleteInstallerRecords({p}): {ex.Message}"); }
            }
        }

        private static void FinalRegistryCleanup()
        {
            Debug.WriteLine("Starting FinalRegistryCleanup...");

            var passes = new[]
            {
                // ── Original paths ──────────────────────────────────────────────
                new { Key = Registry.LocalMachine, Path = @"Software" },
                new { Key = Registry.LocalMachine, Path = @"Software\Wow6432Node" },
                new { Key = Registry.LocalMachine, Path = @"Software\Classes" },
                new { Key = Registry.LocalMachine, Path = @"Software\Wow6432Node\Classes" },
                new { Key = Registry.LocalMachine, Path = @"System\CurrentControlSet\Services" },
                new { Key = Registry.CurrentUser,  Path = @"Software" },
                new { Key = Registry.CurrentUser,  Path = @"Software\Classes" },
                // ── NEW paths ───────────────────────────────────────────────────
                new { Key = Registry.LocalMachine, Path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Browser Helper Objects" },
                new { Key = Registry.LocalMachine, Path = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options" },
                new { Key = Registry.LocalMachine, Path = @"SYSTEM\CurrentControlSet\Control\SafeBoot\Minimal" },
                new { Key = Registry.LocalMachine, Path = @"SYSTEM\CurrentControlSet\Control\SafeBoot\Network" },
                new { Key = Registry.LocalMachine, Path = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon" },
                new { Key = Registry.LocalMachine, Path = @"Software\Classes\Installer\Products" },
                new { Key = Registry.LocalMachine, Path = @"Software\Classes\Installer\Features" },
            };

            foreach (var entry in passes)
                ComprehensiveCleanupPass(entry.Key, entry.Path);

            Debug.WriteLine("FinalRegistryCleanup done.");
        }

        private static void ComprehensiveCleanupPass(RegistryKey baseKey, string searchPath)
        {
            try
            {
                using (var key = baseKey.OpenSubKey(searchPath, writable: true))
                {
                    if (key == null) return;
                    CleanRegistrySubkeysRecursive(key, searchPath);
                    DeleteBkavValues(baseKey, searchPath);
                }
            }
            catch (Exception ex) { Debug.WriteLine($"ComprehensiveCleanupPass({searchPath}): {ex.Message}"); }
        }

        private static void CleanRegistrySubkeysRecursive(RegistryKey key, string path)
        {
            if (key == null) return;
            try
            {
                foreach (var subKeyName in key.GetSubKeyNames().ToArray())
                {
                    if (ContainsBkavKeyword(subKeyName))
                    {
                        try
                        {
                            key.DeleteSubKeyTree(subKeyName, throwOnMissingSubKey: false);
                            Debug.WriteLine($"Deleted (recursive): {path}\\{subKeyName}");
                        }
                        catch (Exception ex) { Debug.WriteLine($"Failed {subKeyName}: {ex.Message}"); }
                    }
                    else
                    {
                        try
                        {
                            using (var subKey = key.OpenSubKey(subKeyName, writable: true))
                                if (subKey != null)
                                    CleanRegistrySubkeysRecursive(subKey, $"{path}\\{subKeyName}");
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine($"CleanRegistrySubkeysRecursive({path}): {ex.Message}"); }
        }

        // ════════════════════════════════════════════════════════════════════════
        //  STARTUP SHORTCUTS
        // ════════════════════════════════════════════════════════════════════════

        private static void DeleteStartupShortcuts()
        {
            try
            {
                string startupPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    @"Microsoft\Windows\Start Menu\Programs\Startup");

                if (!Directory.Exists(startupPath)) return;
                foreach (var file in Directory.GetFiles(startupPath, "*.lnk"))
                {
                    if (!ContainsBkavKeyword(Path.GetFileName(file))) continue;
                    try { File.Delete(file); Debug.WriteLine($"Deleted shortcut: {file}"); }
                    catch (Exception ex) { Debug.WriteLine($"Failed shortcut {file}: {ex.Message}"); }
                }
            }
            catch (Exception ex) { Debug.WriteLine($"DeleteStartupShortcuts: {ex.Message}"); }
        }

        // ════════════════════════════════════════════════════════════════════════
        //  ENVIRONMENT VARIABLES
        // ════════════════════════════════════════════════════════════════════════

        private static void CleanEnvironmentVariables()
        {
            try
            {
                CleanPathVariable(EnvironmentVariableTarget.User);
                CleanPathVariable(EnvironmentVariableTarget.Machine);
            }
            catch (Exception ex) { Debug.WriteLine($"CleanEnvironmentVariables: {ex.Message}"); }
        }

        private static void CleanPathVariable(EnvironmentVariableTarget target)
        {
            try
            {
                string pathValue = Environment.GetEnvironmentVariable("PATH", target) ?? string.Empty;
                var entries      = pathValue.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                var cleaned      = entries.Where(p => !ContainsBkavKeyword(p)).ToArray();
                if (cleaned.Length == entries.Length) return;
                Environment.SetEnvironmentVariable("PATH", string.Join(";", cleaned), target);
                Debug.WriteLine($"Cleaned PATH ({target})");
            }
            catch (Exception ex) { Debug.WriteLine($"CleanPathVariable({target}): {ex.Message}"); }
        }

        // ════════════════════════════════════════════════════════════════════════
        //  CONTEXT MENU (right-click on OS label)
        // ════════════════════════════════════════════════════════════════════════

        private void lblwin_Click(object sender, EventArgs e) => ShowToolsMenu(Cursor.Position);

        private void btnTools_Click(object sender, EventArgs e)
        {
            var screenPoint = btnTools.PointToScreen(new System.Drawing.Point(0, btnTools.Height));
            ShowToolsMenu(screenPoint);
        }

        private void ShowToolsMenu(System.Drawing.Point screenPosition)
        {
            // Không dùng "using" và không tự Dispose(): ContextMenuStrip.Show() không chặn
            // luồng gọi, nên dispose ngay sau Show() (hoặc ngay trong sự kiện Closed của
            // chính nó) có thể hủy menu trong lúc nó vẫn đang tự xử lý việc đóng/chọn mục,
            // gây lỗi "Cannot access a disposed object". Để GC thu dọn là đủ cho một menu
            // nhỏ, dùng tạm thời như thế này.
            var menu = new ContextMenuStrip();

            var updateItem = new ToolStripMenuItem("🔄 Kiểm tra cập nhật");
            updateItem.Click += (s, args) => CheckForUpdatesInBackground(showUpToDateMessage: true);
            menu.Items.Add(updateItem);

            var aboutItem = new ToolStripMenuItem("ℹ️ Giới thiệu");
            aboutItem.Click += (s, args) => ShowAbout();
            menu.Items.Add(aboutItem);

            var exitItem = new ToolStripMenuItem("🚪 Thoát");
            exitItem.Click += (s, args) => Application.Exit();
            menu.Items.Add(exitItem);

            menu.Show(screenPosition);
        }

        private void ShowAbout()
        {
            MessageBox.Show(
                "Công Cụ Gỡ Cài BKAV\n" +
                "Gỡ cài đặt và dọn dẹp triệt để BKAV Pro / BKAV Corporation\n" +
                "khỏi thư mục, registry, shortcut khởi động và biến môi trường.",
                "Giới thiệu",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // ════════════════════════════════════════════════════════════════════════
        //  KIỂM TRA CẬP NHẬT
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>Kiểm tra bản mới ở luồng nền; không chặn UI, lỗi mạng bị bỏ qua lặng lẽ.</summary>
        private async void CheckForUpdatesInBackground(bool showUpToDateMessage)
        {
            UpdateChecker.UpdateInfo info = null;
            try
            {
                info = await UpdateChecker.CheckAsync().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CheckForUpdatesInBackground: {ex.Message}");
                if (showUpToDateMessage)
                    MessageBox.Show("Không thể kiểm tra cập nhật (lỗi mạng hoặc máy chủ).",
                        "Kiểm tra cập nhật", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (this.IsDisposed) return;

            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

            if (info != null && info.Version > currentVersion)
            {
                var result = MessageBox.Show(
                    $"Đã có phiên bản mới: {info.TagName}\n" +
                    $"Phiên bản đang dùng: {currentVersion}\n\n" +
                    "Mở trang tải về ngay bây giờ?",
                    "Có bản cập nhật mới",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    try { Process.Start(info.HtmlUrl); }
                    catch (Exception ex) { Debug.WriteLine($"Open release page: {ex.Message}"); }
                }
            }
            else if (showUpToDateMessage)
            {
                MessageBox.Show($"Bạn đang dùng phiên bản mới nhất ({currentVersion}).",
                    "Kiểm tra cập nhật", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
