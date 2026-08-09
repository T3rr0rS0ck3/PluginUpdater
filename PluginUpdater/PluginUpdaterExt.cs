using KeePass.Plugins;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PluginUpdater
{
    /// <summary>
    /// KeePass plugin entry point that wires the updater into the host application.
    /// </summary>
    public sealed class PluginUpdaterExt : Plugin
    {
        private Task _updateTask;
        private ToolStripMenuItem _updateNowMenuItem;

        /// <summary>
        /// Gets the update URL used by KeePass to check for new PluginUpdater releases.
        /// </summary>
        public override string UpdateUrl
        {
            get { return "https://raw.githubusercontent.com/T3rr0rS0ck3/PluginUpdater/refs/heads/main/version.info"; }
        }

        /// <summary>
        /// Initializes the plugin with the provided host.
        /// </summary>
        /// <returns></returns>
        public override bool Initialize(IPluginHost host)
        {
            if (host == null)
            {
                return false;
            }

            StateStorage.Instance().Host = host;

            return true;
        }

        /// <summary>
        /// Terminates the plugin, cleaning up resources and event handlers.
        /// </summary>
        /// <summary>
        /// Cleans up plugin resources before KeePass unloads the plugin.
        /// </summary>
        public override void Terminate()
        {
            this._updateTask = null;
            this._updateNowMenuItem = null;
        }

        /// <summary>
        /// Returns a menu item for the plugin in the specified menu type.
        /// </summary>
        /// <returns></returns>
        /// <summary>
        /// Returns the plugin menu entry for the KeePass main menu.
        /// </summary>
        public override ToolStripMenuItem GetMenuItem(PluginMenuType t)
        {
            // Provide a menu item for the main location(s)
            if (t == PluginMenuType.Main)
            {
                ToolStripMenuItem tsmi = new ToolStripMenuItem();
                tsmi.Text = StateStorage.Instance().Name;
                tsmi.DropDownItems.Add("Settings", null, this.OnOptionsClicked);

                this._updateNowMenuItem = new ToolStripMenuItem("Update now");
                this._updateNowMenuItem.Click += this.OnUpdateNowClicked;
                tsmi.DropDownItems.Add(this._updateNowMenuItem);

                return tsmi;
            }

            return null; // No menu items in other locations
        }

        /// <summary>
        /// Opens the settings dialog.
        /// </summary>
        private void OnOptionsClicked(object sender, EventArgs e)
        {
            if (StateStorage.Instance().SettingsForm == null)
            {
                StateStorage.Instance().SettingsForm = new Settings
                {
                    StartPosition = System.Windows.Forms.FormStartPosition.CenterParent,
                    Text = StateStorage.Instance().Name + " Settings"
                };
            }

            StateStorage.Instance().SettingsForm.ShowDialog(StateStorage.Instance().Host.MainWindow);
        }

        /// <summary>
        /// Starts a manual update check and installation.
        /// </summary>
        private async void OnUpdateNowClicked(object sender, EventArgs e)
        {
            try
            {
                await this.StartUpdate(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(StateStorage.Instance().Host.MainWindow, "Plugin update failed: " + ex.Message, StateStorage.Instance().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Runs the update workflow and prevents concurrent executions.
        /// </summary>
        private async Task StartUpdate(bool forceUpdate)
        {
            if (StateStorage.Instance().Host == null || StateStorage.Instance().Host.MainWindow == null)
            {
                return;
            }

            if (!forceUpdate && (StateStorage.Instance().Host.Database == null || !StateStorage.Instance().Host.Database.IsOpen))
            {
                return;
            }

            if (this._updateTask != null && !this._updateTask.IsCompleted)
            {
                return;
            }

            if (this._updateNowMenuItem != null)
            {
                this._updateNowMenuItem.Enabled = false;
            }

            this._updateTask = PluginManager.Instance().Execute(forceUpdate);

            try
            {
                await this._updateTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine("PluginUpdater failed: {0}", ex);
                throw;
            }
            finally
            {
                if (this._updateNowMenuItem != null)
                {
                    this._updateNowMenuItem.Enabled = true;
                }
            }
        }
    }
}
