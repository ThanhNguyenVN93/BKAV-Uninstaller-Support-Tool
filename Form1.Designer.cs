namespace WindowsFormsApp1
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.grbtitle = new System.Windows.Forms.GroupBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lbltitle = new System.Windows.Forms.Label();
            this.grbinfo = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtinsdrect = new System.Windows.Forms.TextBox();
            this.grbfunct = new System.Windows.Forms.GroupBox();
            this.btnremove = new System.Windows.Forms.Button();
            this.btnunin = new System.Windows.Forms.Button();
            this.grbsysinfo = new System.Windows.Forms.GroupBox();
            this.lblwin = new System.Windows.Forms.Label();
            this.grbtitle.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.grbinfo.SuspendLayout();
            this.grbfunct.SuspendLayout();
            this.grbsysinfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // grbtitle
            // 
            this.grbtitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.grbtitle.Controls.Add(this.pictureBox1);
            this.grbtitle.Controls.Add(this.lbltitle);
            this.grbtitle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.grbtitle.Location = new System.Drawing.Point(0, 0);
            this.grbtitle.Name = "grbtitle";
            this.grbtitle.Size = new System.Drawing.Size(680, 70);
            this.grbtitle.TabIndex = 0;
            this.grbtitle.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(10, 8);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(50, 50);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // lbltitle
            // 
            this.lbltitle.AutoSize = true;
            this.lbltitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lbltitle.ForeColor = System.Drawing.Color.White;
            this.lbltitle.Location = new System.Drawing.Point(70, 18);
            this.lbltitle.Name = "lbltitle";
            this.lbltitle.Size = new System.Drawing.Size(400, 32);
            this.lbltitle.TabIndex = 0;
            this.lbltitle.Text = "BKAV UNINSTALLER";
            // 
            // grbinfo
            // 
            this.grbinfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.grbinfo.Controls.Add(this.label1);
            this.grbinfo.Controls.Add(this.txtinsdrect);
            this.grbinfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grbinfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.grbinfo.Location = new System.Drawing.Point(10, 78);
            this.grbinfo.Name = "grbinfo";
            this.grbinfo.Size = new System.Drawing.Size(660, 90);
            this.grbinfo.TabIndex = 1;
            this.grbinfo.TabStop = false;
            this.grbinfo.Text = "📁 Installation Info";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.label1.Location = new System.Drawing.Point(15, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Location:";
            // 
            // txtinsdrect
            // 
            this.txtinsdrect.BackColor = System.Drawing.Color.White;
            this.txtinsdrect.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtinsdrect.Enabled = false;
            this.txtinsdrect.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtinsdrect.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.txtinsdrect.Location = new System.Drawing.Point(15, 42);
            this.txtinsdrect.Name = "txtinsdrect";
            this.txtinsdrect.Size = new System.Drawing.Size(630, 22);
            this.txtinsdrect.TabIndex = 0;
            // 
            // grbfunct
            // 
            this.grbfunct.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.grbfunct.Controls.Add(this.btnremove);
            this.grbfunct.Controls.Add(this.btnunin);
            this.grbfunct.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grbfunct.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.grbfunct.Location = new System.Drawing.Point(10, 176);
            this.grbfunct.Name = "grbfunct";
            this.grbfunct.Size = new System.Drawing.Size(660, 75);
            this.grbfunct.TabIndex = 2;
            this.grbfunct.TabStop = false;
            this.grbfunct.Text = "⚙️ Actions";
            // 
            // btnremove
            // 
            this.btnremove.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(53)))), ((int)(((byte)(69)))));
            this.btnremove.Enabled = false;
            this.btnremove.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(40)))), ((int)(((byte)(55)))));
            this.btnremove.FlatAppearance.BorderSize = 0;
            this.btnremove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnremove.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnremove.ForeColor = System.Drawing.Color.White;
            this.btnremove.Location = new System.Drawing.Point(350, 30);
            this.btnremove.Name = "btnremove";
            this.btnremove.Size = new System.Drawing.Size(295, 35);
            this.btnremove.TabIndex = 1;
            this.btnremove.Text = "🗑️  Delete Leftover";
            this.btnremove.UseVisualStyleBackColor = false;
            this.btnremove.Click += new System.EventHandler(this.btnremove_Click);
            // 
            // btnunin
            // 
            this.btnunin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this.btnunin.Enabled = false;
            this.btnunin.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(120)))), ((int)(((byte)(50)))));
            this.btnunin.FlatAppearance.BorderSize = 0;
            this.btnunin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnunin.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnunin.ForeColor = System.Drawing.Color.White;
            this.btnunin.Location = new System.Drawing.Point(15, 30);
            this.btnunin.Name = "btnunin";
            this.btnunin.Size = new System.Drawing.Size(295, 35);
            this.btnunin.TabIndex = 0;
            this.btnunin.Text = "🔧  Uninstall BKAV";
            this.btnunin.UseVisualStyleBackColor = false;
            this.btnunin.Click += new System.EventHandler(this.btnunin_Click);
            // 
            // grbsysinfo
            // 
            this.grbsysinfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.grbsysinfo.Controls.Add(this.lblwin);
            this.grbsysinfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grbsysinfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.grbsysinfo.Location = new System.Drawing.Point(10, 259);
            this.grbsysinfo.Name = "grbsysinfo";
            this.grbsysinfo.Size = new System.Drawing.Size(660, 65);
            this.grbsysinfo.TabIndex = 1;
            this.grbsysinfo.TabStop = false;
            this.grbsysinfo.Text = "💻 System Info";
            // 
            // lblwin
            // 
            this.lblwin.AutoSize = true;
            this.lblwin.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblwin.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.lblwin.Location = new System.Drawing.Point(15, 28);
            this.lblwin.Name = "lblwin";
            this.lblwin.Size = new System.Drawing.Size(66, 15);
            this.lblwin.TabIndex = 0;
            this.lblwin.Text = "Loading...";
            this.lblwin.Click += new System.EventHandler(this.lblwin_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(680, 335);
            this.Controls.Add(this.grbsysinfo);
            this.Controls.Add(this.grbfunct);
            this.Controls.Add(this.grbinfo);
            this.Controls.Add(this.grbtitle);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(696, 373);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(696, 373);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "BKAV UNINSTALLER TOOL";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.grbtitle.ResumeLayout(false);
            this.grbtitle.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.grbinfo.ResumeLayout(false);
            this.grbinfo.ResumeLayout(false);
            this.grbinfo.PerformLayout();
            this.grbfunct.ResumeLayout(false);
            this.grbsysinfo.ResumeLayout(false);
            this.grbsysinfo.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grbtitle;
        private System.Windows.Forms.GroupBox grbinfo;
        private System.Windows.Forms.GroupBox grbfunct;
        private System.Windows.Forms.GroupBox grbsysinfo;
        private System.Windows.Forms.Label lbltitle;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtinsdrect;
        private System.Windows.Forms.Button btnunin;
        private System.Windows.Forms.Label lblwin;
        private System.Windows.Forms.Button btnremove;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}

