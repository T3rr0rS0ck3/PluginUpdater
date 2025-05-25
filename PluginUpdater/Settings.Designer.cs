namespace PluginUpdater
{
    partial class Settings
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
            this.components = new System.ComponentModel.Container();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.pluginInfoBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.btnOk = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.cbEnableUpdate = new System.Windows.Forms.CheckBox();
            this.cbEnableNotification = new System.Windows.Forms.CheckBox();
            this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDownloadUrl = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CurrentVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LatestVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pluginInfoBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.nameDataGridViewTextBoxColumn,
            this.colDownloadUrl,
            this.CurrentVersion,
            this.LatestVersion});
            this.dataGridView1.DataSource = this.pluginInfoBindingSource;
            this.dataGridView1.Location = new System.Drawing.Point(12, 197);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 62;
            this.dataGridView1.RowTemplate.Height = 28;
            this.dataGridView1.Size = new System.Drawing.Size(776, 390);
            this.dataGridView1.TabIndex = 0;
            // 
            // pluginInfoBindingSource
            // 
            this.pluginInfoBindingSource.DataSource = typeof(PluginUpdater.PluginInfo);
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(688, 593);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(100, 30);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.Location = new System.Drawing.Point(12, 12);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(776, 109);
            this.richTextBox1.TabIndex = 2;
            this.richTextBox1.Text = "The download URLs must be configured so that the automatic update of the plugins " +
    "works.  \n\n!!! IMPORTANT !!! \nThe URL must contain the placeholder “<version>” (w" +
    "ithout quotation marks)";
            // 
            // cbEnableUpdate
            // 
            this.cbEnableUpdate.AutoSize = true;
            this.cbEnableUpdate.Checked = true;
            this.cbEnableUpdate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbEnableUpdate.Location = new System.Drawing.Point(12, 137);
            this.cbEnableUpdate.Name = "cbEnableUpdate";
            this.cbEnableUpdate.Size = new System.Drawing.Size(222, 24);
            this.cbEnableUpdate.TabIndex = 3;
            this.cbEnableUpdate.Text = "Automatic update enabled";
            this.cbEnableUpdate.UseVisualStyleBackColor = true;
            this.cbEnableUpdate.CheckedChanged += new System.EventHandler(this.cbEnableUpdate_CheckedChanged);
            // 
            // cbEnableNotification
            // 
            this.cbEnableNotification.AutoSize = true;
            this.cbEnableNotification.Checked = true;
            this.cbEnableNotification.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbEnableNotification.Location = new System.Drawing.Point(12, 167);
            this.cbEnableNotification.Name = "cbEnableNotification";
            this.cbEnableNotification.Size = new System.Drawing.Size(158, 24);
            this.cbEnableNotification.TabIndex = 4;
            this.cbEnableNotification.Text = "Show Notification";
            this.cbEnableNotification.UseVisualStyleBackColor = true;
            this.cbEnableNotification.CheckedChanged += new System.EventHandler(this.cbEnableNotification_CheckedChanged);
            // 
            // nameDataGridViewTextBoxColumn
            // 
            this.nameDataGridViewTextBoxColumn.DataPropertyName = "Name";
            this.nameDataGridViewTextBoxColumn.HeaderText = "Name";
            this.nameDataGridViewTextBoxColumn.MinimumWidth = 8;
            this.nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
            // 
            // colDownloadUrl
            // 
            this.colDownloadUrl.DataPropertyName = "DownloadUrl";
            this.colDownloadUrl.HeaderText = "Download URL";
            this.colDownloadUrl.MinimumWidth = 8;
            this.colDownloadUrl.Name = "colDownloadUrl";
            // 
            // CurrentVersion
            // 
            this.CurrentVersion.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.CurrentVersion.DataPropertyName = "CurrentVersion";
            this.CurrentVersion.HeaderText = "Current Version";
            this.CurrentVersion.MinimumWidth = 8;
            this.CurrentVersion.Name = "CurrentVersion";
            this.CurrentVersion.ReadOnly = true;
            this.CurrentVersion.Width = 144;
            // 
            // LatestVersion
            // 
            this.LatestVersion.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.LatestVersion.DataPropertyName = "LatestVersion";
            this.LatestVersion.HeaderText = "Latest Version";
            this.LatestVersion.MinimumWidth = 8;
            this.LatestVersion.Name = "LatestVersion";
            this.LatestVersion.ReadOnly = true;
            this.LatestVersion.Width = 136;
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 634);
            this.Controls.Add(this.cbEnableNotification);
            this.Controls.Add(this.cbEnableUpdate);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.dataGridView1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "Settings";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Settings";
            this.Shown += new System.EventHandler(this.Settings_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pluginInfoBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.BindingSource pluginInfoBindingSource;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.DataGridViewTextBoxColumn fileVersionDataGridViewTextBoxColumn;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.CheckBox cbEnableUpdate;
        private System.Windows.Forms.CheckBox cbEnableNotification;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDownloadUrl;
        private System.Windows.Forms.DataGridViewTextBoxColumn CurrentVersion;
        private System.Windows.Forms.DataGridViewTextBoxColumn LatestVersion;
    }
}