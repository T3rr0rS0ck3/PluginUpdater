using KeePass.Plugins;
using System;
using System.Reflection;
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

        public override string UpdateUrl => "https://github.com/T3rr0rS0ck3/PluginUpdater/blob/main/version.info";

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

            Assembly assembly = Assembly.GetExecutingAssembly();
            var productAttr = assembly.GetCustomAttribute<AssemblyTitleAttribute>();
            StateStorage.Instance().Name = productAttr?.Title ?? "Plugin Updater";

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
            this._updateTask.Dispose();
        }

        private void MainWindow_UIStateUpdated(object sender, EventArgs e)
        {
            this._updateTask = PluginManager.Instance().Execute();
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
                tsmi.Click += this.OnOptionsClicked;
                return tsmi;
            }

            return null; // No menu items in other locations
        }

        private void OnOptionsClicked(object sender, EventArgs e)
        {
            StateStorage.Instance().SettingsForm.ShowDialog(StateStorage.Instance().Host.MainWindow);
        }
    }
}
