namespace PluginUpdater
{
    /// <summary>
    /// Represents additional settings for the plugin updater, such as update enablement and notification preferences.
    /// </summary>
    public class AdditionalSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether updates are enabled for the plugins.
        /// </summary>
        public bool IsUpdateEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show notifications for available updates.
        /// </summary>
        public bool ShowUpdateNotification { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalSettings"/> class with default values.
        /// </summary>
        public AdditionalSettings()
        {
            this.IsUpdateEnabled = true; // Default to enabled
            this.ShowUpdateNotification = true; // Default to show notifications
        }
    }
}
