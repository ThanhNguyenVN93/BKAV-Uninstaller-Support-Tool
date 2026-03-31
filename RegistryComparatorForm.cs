using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class RegistryComparatorForm : Form
    {
        private TextBox txtBeforePath;
        private TextBox txtAfterPath;
        private Button btnBrowseBefore;
        private Button btnBrowseAfter;
        private Button btnCompare;
        private TextBox txtResults;
        private Label lblStatus;

        public RegistryComparatorForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.txtBeforePath = new TextBox();
            this.txtAfterPath = new TextBox();
            this.btnBrowseBefore = new Button();
            this.btnBrowseAfter = new Button();
            this.btnCompare = new Button();
            this.txtResults = new TextBox();
            this.lblStatus = new Label();
            this.SuspendLayout();

            // txtBeforePath
            this.txtBeforePath.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtBeforePath.Location = new System.Drawing.Point(20, 20);
            this.txtBeforePath.Name = "txtBeforePath";
            this.txtBeforePath.Size = new System.Drawing.Size(400, 23);
            this.txtBeforePath.TabIndex = 0;
            this.txtBeforePath.Text = "bkav - before.txt";

            // btnBrowseBefore
            this.btnBrowseBefore.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnBrowseBefore.Location = new System.Drawing.Point(430, 20);
            this.btnBrowseBefore.Name = "btnBrowseBefore";
            this.btnBrowseBefore.Size = new System.Drawing.Size(80, 23);
            this.btnBrowseBefore.TabIndex = 1;
            this.btnBrowseBefore.Text = "Browse";
            this.btnBrowseBefore.Click += BtnBrowseBefore_Click;

            // txtAfterPath
            this.txtAfterPath.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtAfterPath.Location = new System.Drawing.Point(20, 55);
            this.txtAfterPath.Name = "txtAfterPath";
            this.txtAfterPath.Size = new System.Drawing.Size(400, 23);
            this.txtAfterPath.TabIndex = 2;
            this.txtAfterPath.Text = "bkav - after.txt";

            // btnBrowseAfter
            this.btnBrowseAfter.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnBrowseAfter.Location = new System.Drawing.Point(430, 55);
            this.btnBrowseAfter.Name = "btnBrowseAfter";
            this.btnBrowseAfter.Size = new System.Drawing.Size(80, 23);
            this.btnBrowseAfter.TabIndex = 3;
            this.btnBrowseAfter.Text = "Browse";
            this.btnBrowseAfter.Click += BtnBrowseAfter_Click;

            // btnCompare
            this.btnCompare.BackColor = System.Drawing.Color.FromArgb(40, 167, 69);
            this.btnCompare.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCompare.ForeColor = System.Drawing.Color.White;
            this.btnCompare.Location = new System.Drawing.Point(20, 90);
            this.btnCompare.Name = "btnCompare";
            this.btnCompare.Size = new System.Drawing.Size(490, 35);
            this.btnCompare.TabIndex = 4;
            this.btnCompare.Text = "Compare Registry Files";
            this.btnCompare.UseVisualStyleBackColor = false;
            this.btnCompare.Click += BtnCompare_Click;

            // lblStatus
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStatus.Location = new System.Drawing.Point(20, 135);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 15);
            this.lblStatus.TabIndex = 5;

            // txtResults
            this.txtResults.Font = new System.Drawing.Font("Courier New", 9F);
            this.txtResults.Location = new System.Drawing.Point(20, 160);
            this.txtResults.Multiline = true;
            this.txtResults.Name = "txtResults";
            this.txtResults.ReadOnly = true;
            this.txtResults.ScrollBars = ScrollBars.Both;
            this.txtResults.Size = new System.Drawing.Size(490, 350);
            this.txtResults.TabIndex = 6;
            this.txtResults.WordWrap = false;

            // RegistryComparatorForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(530, 530);
            this.Controls.Add(this.txtResults);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnCompare);
            this.Controls.Add(this.btnBrowseAfter);
            this.Controls.Add(this.btnBrowseBefore);
            this.Controls.Add(this.txtAfterPath);
            this.Controls.Add(this.txtBeforePath);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.Name = "RegistryComparatorForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Registry Comparator - Before/After Analysis";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void BtnBrowseBefore_Click(object sender, EventArgs e)
        {
            BrowseFile(txtBeforePath);
        }

        private void BtnBrowseAfter_Click(object sender, EventArgs e)
        {
            BrowseFile(txtAfterPath);
        }

        private void BrowseFile(TextBox textBox)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Text Files|*.txt|All Files|*.*";
                dialog.Title = "Select Registry File";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textBox.Text = dialog.FileName;
                }
            }
        }

        private void BtnCompare_Click(object sender, EventArgs e)
        {
            if (!File.Exists(txtBeforePath.Text))
            {
                MessageBox.Show("Before file not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(txtAfterPath.Text))
            {
                MessageBox.Show("After file not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            lblStatus.Text = "Comparing...";
            Application.DoEvents();

            try
            {
                var remainingEntries = CompareRegistries(txtBeforePath.Text, txtAfterPath.Text);
                DisplayResults(remainingEntries);
                lblStatus.Text = $"Found {remainingEntries.Count} remaining entries";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Error during comparison";
            }
        }

        private List<string> CompareRegistries(string beforePath, string afterPath)
        {
            var beforeEntries = ParseRegistryFile(beforePath);
            var afterEntries = ParseRegistryFile(afterPath);

            // Find entries that are in AFTER but should be deleted (in BEFORE)
            var remainingEntries = new List<string>();
            var deletedEntries = beforeEntries.Where(e => e.StartsWith("[-")).Select(e => e.Substring(2, e.Length - 3)).ToList();

            foreach (var deletedEntry in deletedEntries)
            {
                if (afterEntries.Any(e => e.StartsWith($"[{deletedEntry}]")))
                {
                    remainingEntries.Add(deletedEntry);
                }
            }

            return remainingEntries;
        }

        private List<string> ParseRegistryFile(string filePath)
        {
            var entries = new List<string>();

            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith("["))
                        {
                            entries.Add(line.Trim());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ParseRegistryFile Error: {ex.Message}");
            }

            return entries;
        }

        private void DisplayResults(List<string> remainingEntries)
        {
            txtResults.Clear();

            if (remainingEntries.Count == 0)
            {
                txtResults.AppendText("✓ SUCCESS! No remaining entries found.\n");
                txtResults.AppendText("All BKAV registry entries have been successfully removed.\n");
                return;
            }

            txtResults.AppendText($"⚠ WARNING: {remainingEntries.Count} registry entries still remain:\n\n");
            txtResults.AppendText("═══════════════════════════════════════════════════════════════\n\n");

            foreach (var entry in remainingEntries.OrderBy(e => e))
            {
                txtResults.AppendText(entry + "\n");
            }

            txtResults.AppendText("\n═══════════════════════════════════════════════════════════════\n\n");
            txtResults.AppendText("RECOMMENDATIONS:\n");
            txtResults.AppendText("1. These entries should have been deleted but still exist\n");
            txtResults.AppendText("2. You may need to:\n");
            txtResults.AppendText("   - Run the tool again with admin rights\n");
            txtResults.AppendText("   - Manually delete these registry entries\n");
            txtResults.AppendText("   - Run Registry Finder tool to verify\n");
        }
    }
}
