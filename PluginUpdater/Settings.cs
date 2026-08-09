using System;
using System.Windows.Forms;

namespace PluginUpdater
{
    /// <summary>
    /// Settings form for managing plugin update URLs and additional settings.
    /// </summary>
    public partial class Settings : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> form.
        /// </summary>
        public Settings()
        {
            InitializeComponent();

            this.dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.CellEndEdit += DataGridView1_CellEndEdit;
        }

        /// <summary>
        /// Stores the edited download URL back into the in-memory plugin configuration.
        /// </summary>
        private void DataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCell cellUrl = this.dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];
            DataGridViewCell cellName = this.dataGridView1.Rows[e.RowIndex].Cells[0];
            string downloadUrl = cellUrl.Value?.ToString();
            string pluginName = cellName.Value?.ToString();

            PluginManager.Instance().SetDownloadUrl(pluginName, downloadUrl);
        }

        /// <summary>
        /// Saves the current settings and closes the form.
        /// </summary>
        private void btnOk_Click(object sender, EventArgs e)
        {
            PluginManager.Instance().SaveSettings();
        }

        /// <summary>
        /// Loads the current plugin and settings data into the dialog when it is shown.
        /// </summary>
        private void Settings_Shown(object sender, EventArgs e)
        {
            this.dataGridView1.DataSource = StateStorage.Instance().Settings.PluginList;
            this.cbEnableUpdate.Checked = StateStorage.Instance().Settings.AdditionalSettings.IsUpdateEnabled;
            this.cbEnableNotification.Checked = StateStorage.Instance().Settings.AdditionalSettings.ShowUpdateNotification;
        }

        /// <summary>
        /// Persists the automatic update setting when the checkbox changes.
        /// </summary>
        private void cbEnableUpdate_CheckedChanged(object sender, EventArgs e)
        {
            StateStorage.Instance().Settings.AdditionalSettings.IsUpdateEnabled = this.cbEnableUpdate.Checked;
        }

        /// <summary>
        /// Persists the notification setting when the checkbox changes.
        /// </summary>
        private void cbEnableNotification_CheckedChanged(object sender, EventArgs e)
        {
            StateStorage.Instance().Settings.AdditionalSettings.ShowUpdateNotification = this.cbEnableNotification.Checked;
        }
    }
}
