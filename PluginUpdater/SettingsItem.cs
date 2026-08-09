using System.Collections.Generic;

namespace PluginUpdater
{
    /// <summary>
    /// Represents a settings item that contains a list of plugins and additional settings for the plugin updater.
    /// </summary>
    [System.Runtime.Serialization.DataContract]
    public class SettingsItem
    {
        /// <summary>
        /// Gets or sets the list of plugins with their update URLs and download URLs.
        /// </summary>
        [System.Runtime.Serialization.DataMember]
        public List<PluginInfo> PluginList { get; set; }

        /// <summary>
        /// Gets or sets the additional settings for the plugin updater, such as update enablement and notification preferences.
        /// </summary>
        [System.Runtime.Serialization.DataMember]
        public AdditionalSettings AdditionalSettings { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsItem"/> class.
        /// </summary>
        public SettingsItem()
        {
            this.PluginList = new List<PluginInfo>();
            this.AdditionalSettings = new AdditionalSettings();
        }
    }
}
