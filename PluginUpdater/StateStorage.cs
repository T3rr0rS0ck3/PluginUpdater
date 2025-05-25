using KeePass.Plugins;

namespace PluginUpdater
{
    /// <summary>
    /// StateStorage is a singleton class that holds the state of the plugin updater.
    /// </summary>
    public class StateStorage
    {
        private static StateStorage _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// Settings for the plugin updater, including a list of plugins and additional settings.
        /// </summary>
        public SettingsItem Settings { get; set; }

        /// <summary>
        /// Name of the plugin updater, used for identification and display purposes.
        /// </summary>
        public string Name
        {
            get
            {
                return "PluginUpdater";
            }
        }

        /// <summary>
        /// Host for the plugin, which provides access to the KeePass application and its features.
        /// </summary>
        public IPluginHost Host { get; set; }

        /// <summary>
        /// Settings form for managing plugin update URLs and additional settings.
        /// </summary>
        public Settings SettingsForm { get; set; }

        /// <summary>
        /// Indicates whether a restart is required after applying updates or changes.
        /// </summary>
        public bool RestartRequired { get; set; }

        /// <summary>
        /// Singleton instance of the StateStorage class.
        /// </summary>
        /// <returns></returns>
        public static StateStorage Instance()
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new StateStorage();
                }
                return _instance;
            }
        }

        private StateStorage()
        {
            this.Settings = new SettingsItem();
            this.Host = null; // This should be set by the plugin host when the plugin is loaded
            this.SettingsForm = null;
            this.RestartRequired = false;
        }
    }
}
