using Newtonsoft.Json;
using System;

namespace PluginUpdater
{
    /// <summary>
    /// Represents information about a plugin, including its name, file version, update URL, and download URL.
    /// </summary>
    public class PluginInfo
    {
        private string _currentVersionStr;
        private Version _currentVersion;
        private string _latestVersionStr;
        private Version _latestVersion;

        /// <summary>
        /// Gets or sets the name of the plugin.
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// Gets the Current Version of the plugin.
        /// </summary>
        [JsonIgnore]
        public Version CurrentVersion
        {
            get
            {
                return _currentVersion;
            }
        }

        /// <summary>
        /// Gets or sets the Current Version of the plugin as a string.
        /// </summary>
        public string CurrentVersionStr
        {
            get
            {
                return _currentVersionStr;
            }
            set
            {
                this._currentVersion = Version.TryParse(value, out Version version) ? version : new Version(0, 0, 0, 0);
                _currentVersionStr = value;
            }
        }

        /// <summary>
        /// Gets or sets the URL for checking updates for the plugin.
        /// </summary>
        public string UpdateUrl { get; set; }


        /// <summary>
        /// Gets a value indicating whether there is an update available for the plugin.
        /// </summary>
        public bool HasUpdate
        {
            get
            {
                if (_latestVersion == null || _currentVersion == null)
                {
                    return false;
                }
                return _latestVersion.CompareTo(_currentVersion) > 0;
            }
        }

        /// <summary>
        /// Gets the Latest Version of the plugin.
        /// </summary>
        [JsonIgnore]
        public Version LatestVersion
        {
            get
            {
                return _latestVersion;
            }
        }

        /// <summary>
        /// Gets or sets the Latest Version of the plugin as a string.
        /// </summary>
        public string LatestVersionStr
        {
            get
            {
                return _latestVersionStr;
            }
            set
            {
                this._latestVersion = Version.TryParse(value, out Version version) ? version : new Version(0, 0, 0, 0);
                _latestVersionStr = value;
            }
        }

        /// <summary>
        /// Gets or sets the URL to download the plugin.
        /// </summary>
        public string DownloadUrl { get; set; }
    }
}
