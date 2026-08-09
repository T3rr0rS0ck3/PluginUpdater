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
            string downloadUrl = cellUrl.Value != null ? cellUrl.Value.ToString() : null;
            string pluginName = cellName.Value != null ? cellName.Value.ToString() : null;

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
        private async void Settings_Shown(object sender, EventArgs e)
        {
            await this.RefreshPluginGrid(true);
            this.cbEnableUpdate.Checked = StateStorage.Instance().Settings.AdditionalSettings.IsUpdateEnabled;
            this.cbEnableNotification.Checked = StateStorage.Instance().Settings.AdditionalSettings.ShowUpdateNotification;
        }

        /// <summary>
        /// Refreshes the plugin grid and optionally checks update URLs.
        /// </summary>
        private async System.Threading.Tasks.Task RefreshPluginGrid(bool checkForUpdates)
        {
            await PluginManager.Instance().RefreshPluginList(checkForUpdates);

            this.dataGridView1.DataSource = null;
            this.dataGridView1.DataSource = StateStorage.Instance().Settings.PluginList;
        }

        /// <summary>
        /// Checks update URLs and refreshes the latest version data.
        /// </summary>
        private async void btnCheckUpdates_Click(object sender, EventArgs e)
        {
            this.btnCheckUpdates.Enabled = false;
            this.btnCheckUpdates.Text = "Checking...";

            try
            {
                await this.RefreshPluginGrid(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Update check failed: " + ex.Message, StateStorage.Instance().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.btnCheckUpdates.Text = "Check for updates";
                this.btnCheckUpdates.Enabled = true;
            }
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
