using KeePass.Plugins;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PluginUpdater
{
    /// <summary>
    /// Plugin Updater for KeePass
    /// </summary>
    public sealed class PluginUpdaterExt : Plugin
    {
        private Task _updateTask;
        private ToolStripMenuItem _updateNowMenuItem;

        public override string UpdateUrl => "https://raw.githubusercontent.com/T3rr0rS0ck3/PluginUpdater/refs/heads/main/version.info";

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

            StateStorage.Instance().SettingsForm = new Settings
            {
                StartPosition = System.Windows.Forms.FormStartPosition.CenterParent,
                Text = $"{StateStorage.Instance().Name} Settings"
            };

            StateStorage.Instance().Host = host;

            StateStorage.Instance().Host.MainWindow.UIStateUpdated += MainWindow_UIStateUpdated;

            return true;
        }

        /// <summary>
        /// Terminates the plugin, cleaning up resources and event handlers.
        /// </summary>
        public override void Terminate()
        {
            if (this._updateTask != null)
            {
                this._updateTask.Dispose();
            }
        }

        private async void MainWindow_UIStateUpdated(object sender, EventArgs e)
        {
            await this.StartUpdate(false);
        }

        /// <summary>
        /// Returns a menu item for the plugin in the specified menu type.
        /// </summary>
        /// <returns></returns>
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

        private void OnOptionsClicked(object sender, EventArgs e)
        {
            StateStorage.Instance().SettingsForm.ShowDialog(StateStorage.Instance().Host.MainWindow);
        }

        private async void OnUpdateNowClicked(object sender, EventArgs e)
        {
            await this.StartUpdate(true);
        }

        private async Task StartUpdate(bool forceUpdate)
        {
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
