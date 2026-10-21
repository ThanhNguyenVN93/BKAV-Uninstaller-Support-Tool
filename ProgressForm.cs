using System;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class ProgressForm : Form
    {
        private Label lblIcon;
        private Label lblStatus;
        private ProgressBar progressBar;

        public ProgressForm(string title)
        {
            InitializeComponent();
            this.Text = title;
        }

        private void InitializeComponent()
        {
            this.lblIcon = new Label();
            this.lblStatus = new Label();
            this.progressBar = new ProgressBar();
            this.SuspendLayout();

            // lblIcon
            this.lblIcon.AutoSize = true;
            this.lblIcon.Font = new System.Drawing.Font("Segoe UI", 16F);
            this.lblIcon.Location = new System.Drawing.Point(20, 20);
            this.lblIcon.Name = "lblIcon";
            this.lblIcon.Size = new System.Drawing.Size(30, 30);
            this.lblIcon.Text = "🧹";

            // lblStatus
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.lblStatus.Location = new System.Drawing.Point(60, 26);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(320, 20);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "Đang xử lý...";

            // progressBar
            this.progressBar.Location = new System.Drawing.Point(20, 65);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(380, 18);
            this.progressBar.Style = ProgressBarStyle.Marquee;
            this.progressBar.MarqueeAnimationSpeed = 30;

            // ProgressForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(420, 105);
            this.ControlBox = false;
            this.Controls.Add(this.lblIcon);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.progressBar);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProgressForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        public void UpdateStatus(string message)
        {
            if (lblStatus.InvokeRequired)
            {
                lblStatus.Invoke(new Action(() => lblStatus.Text = message));
            }
            else
            {
                lblStatus.Text = message;
            }
        }
    }
}
