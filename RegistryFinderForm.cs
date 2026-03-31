using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Win32;

namespace WindowsFormsApp1
{
    public partial class RegistryFinderForm : Form
    {
        private TextBox txtResults;
        private Button btnScan;

        public RegistryFinderForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.btnScan = new Button();
            this.txtResults = new TextBox();
            this.SuspendLayout();

            // btnScan
            this.btnScan.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnScan.Location = new System.Drawing.Point(20, 20);
            this.btnScan.Name = "btnScan";
            this.btnScan.Size = new System.Drawing.Size(200, 35);
            this.btnScan.TabIndex = 0;
            this.btnScan.Text = "Scan Registry";
            this.btnScan.UseVisualStyleBackColor = true;
            this.btnScan.Click += BtnScan_Click;

            // txtResults
            this.txtResults.Font = new System.Drawing.Font("Courier New", 9F);
            this.txtResults.Location = new System.Drawing.Point(20, 70);
            this.txtResults.Multiline = true;
            this.txtResults.Name = "txtResults";
            this.txtResults.ReadOnly = true;
            this.txtResults.ScrollBars = ScrollBars.Both;
            this.txtResults.Size = new System.Drawing.Size(700, 400);
            this.txtResults.TabIndex = 1;
            this.txtResults.WordWrap = false;

            // RegistryFinderForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(740, 490);
            this.Controls.Add(this.btnScan);
            this.Controls.Add(this.txtResults);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.Name = "RegistryFinderForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Registry Finder - Tìm BKAV Entries";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void BtnScan_Click(object sender, EventArgs e)
        {
            btnScan.Enabled = false;
            txtResults.Clear();
            txtResults.AppendText("Scanning registry...\r\n");
            Application.DoEvents();

            var results = new List<string>();

            // Scan all common locations
            results.AddRange(ScanHKLM());
            results.AddRange(ScanHKCU());
            results.AddRange(ScanServices());
            results.AddRange(ScanScheduledTasks());

            txtResults.Clear();
            txtResults.AppendText($"Found {results.Count} BKAV-related entries:\r\n\r\n");
            foreach (var result in results)
            {
                txtResults.AppendText(result + "\r\n");
            }

            btnScan.Enabled = true;
        }

        private List<string> ScanHKLM()
        {
            var results = new List<string>();

            var paths = new[]
            {
                @"Software",
                @"Software\Wow6432Node",
                @"Software\Microsoft\Windows\CurrentVersion\Uninstall",
                @"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall",
                @"Software\Microsoft\Windows\CurrentVersion\App Paths",
                @"Software\Classes",
                @"System\CurrentControlSet\Services",
                @"System\CurrentControlSet\Enum\PCI",
                @"System\ControlSet001\Services",
                @"System\ControlSet002\Services"
            };

            foreach (var path in paths)
            {
                results.AddRange(SearchRegistry(Registry.LocalMachine, path, "HKEY_LOCAL_MACHINE"));
            }

            return results;
        }

        private List<string> ScanHKCU()
        {
            var results = new List<string>();

            var paths = new[]
            {
                @"Software",
                @"Software\Classes",
                @"Software\Microsoft\Windows\CurrentVersion\Explorer\RunMRU",
                @"Software\Microsoft\Windows\CurrentVersion\Run",
                @"Software\Microsoft\Windows\CurrentVersion\RunOnce"
            };

            foreach (var path in paths)
            {
                results.AddRange(SearchRegistry(Registry.CurrentUser, path, "HKEY_CURRENT_USER"));
            }

            return results;
        }

        private List<string> ScanServices()
        {
            var results = new List<string>();

            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"System\CurrentControlSet\Services"))
                {
                    if (key != null)
                    {
                        foreach (var subKeyName in key.GetSubKeyNames())
                        {
                            if (subKeyName.ToLower().Contains("bkav") || 
                                subKeyName.ToLower().Contains("v-smart") ||
                                subKeyName.ToLower().Contains("vsmart"))
                            {
                                results.Add($"[SERVICE] HKEY_LOCAL_MACHINE\\System\\CurrentControlSet\\Services\\{subKeyName}");

                                using (var subKey = key.OpenSubKey(subKeyName))
                                {
                                    var imagePath = subKey?.GetValue("ImagePath")?.ToString();
                                    if (imagePath != null)
                                        results.Add($"    → ImagePath: {imagePath}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                results.Add($"[ERROR] ScanServices: {ex.Message}");
            }

            return results;
        }

        private List<string> ScanScheduledTasks()
        {
            var results = new List<string>();

            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(
                    @"Software\Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tree"))
                {
                    if (key != null)
                    {
                        foreach (var subKeyName in key.GetSubKeyNames())
                        {
                            if (subKeyName.ToLower().Contains("bkav") || subKeyName.ToLower().Contains("v-smart"))
                            {
                                results.Add($"[TASK] HKEY_LOCAL_MACHINE\\...\\Schedule\\TaskCache\\Tree\\{subKeyName}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                results.Add($"[ERROR] ScanScheduledTasks: {ex.Message}");
            }

            return results;
        }

        private List<string> SearchRegistry(RegistryKey baseKey, string subPath, string hive)
        {
            var results = new List<string>();

            try
            {
                using (var key = baseKey.OpenSubKey(subPath))
                {
                    if (key != null)
                    {
                        // Search subkey names
                        foreach (var subKeyName in key.GetSubKeyNames())
                        {
                            if (subKeyName.ToLower().Contains("bkav") || 
                                subKeyName.ToLower().Contains("v-smart") ||
                                subKeyName.ToLower().Contains("vsmart") ||
                                subKeyName.ToLower().Contains("kaspersky") ||
                                subKeyName.ToLower().Contains("kasper"))
                            {
                                results.Add($"{hive}\\{subPath}\\{subKeyName}");
                            }
                        }

                        // Search value names and data
                        foreach (var valueName in key.GetValueNames())
                        {
                            try
                            {
                                var value = key.GetValue(valueName)?.ToString().ToLower() ?? "";
                                if (value.Contains("bkav") || value.Contains("v-smart") || value.Contains("vsmart"))
                                {
                                    results.Add($"{hive}\\{subPath}\\[VALUE] {valueName}");
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                results.Add($"[ERROR] {hive}\\{subPath}: {ex.Message}");
            }

            return results;
        }
    }
}
