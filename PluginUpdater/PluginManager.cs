using KeePass.App;
using KeePass.Forms;
using KeePass.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PluginUpdater
{
    /// <summary>
    /// Manages the plugin updates for KeePass by checking for available updates, downloading them, and applying them.
    /// </summary>
    public class PluginManager
    {
        private const string PluginUpdaterDownloadUrl = "https://github.com/T3rr0rS0ck3/PluginUpdater/releases/download/<version>/PluginUpdater.plgx";
        private static PluginManager _instance;
        private static readonly object _lock = new object();
        private IList<string> updatedPlugins = new List<string>();

        /// <summary>
        /// Singleton instance of the PluginManager.
        /// </summary>
        /// <returns></returns>
        public static PluginManager Instance()
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new PluginManager();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginManager"/> class.
        /// </summary>
        private PluginManager()
        {
            this.updatedPlugins = new List<string>();
        }

        /// <summary>
        /// Executes the plugin update process, which includes retrieving the plugin list, checking for updates, downloading updates, and restarting the application if necessary.
        /// </summary>
        /// <returns></returns>
        public async Task Execute(bool forceUpdate = false)
        {
            try
            {
                this.updatedPlugins.Clear();
                StateStorage.Instance().RestartRequired = false;
                await RefreshPluginList(true);
                await updatePlugins(forceUpdate);
                restartApplication();
            }
            catch (Exception ex)
            {
                Console.WriteLine("PluginUpdater execution failed: {0}", ex);
                if (forceUpdate)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Refreshes the current KeePass plugin list and merges persisted updater settings.
        /// </summary>
        public async Task RefreshPluginList(bool checkForUpdates)
        {
            StateStorage.Instance().Settings.PluginList = getPluginList().ToList();
            loadSettings();
            ApplyDefaultDownloadUrls();

            if (checkForUpdates)
            {
                await checkForPluginUpdates();
            }
        }

        /// <summary>
        /// Applies built-in download URLs that should not depend on user configuration.
        /// </summary>
        private static void ApplyDefaultDownloadUrls()
        {
            foreach (PluginInfo pluginInfo in StateStorage.Instance().Settings.PluginList)
            {
                if (string.Equals(pluginInfo.Name, StateStorage.Instance().Name, StringComparison.OrdinalIgnoreCase))
                {
                    pluginInfo.DownloadUrl = PluginUpdaterDownloadUrl;
                }
            }
        }

        /// <summary>
        /// Retrieves the list of plugins using reflection to access the private PluginManager property of the MainForm.
        /// </summary>
        /// <returns></returns>
        /// <summary>
        /// Collects the currently loaded KeePass plugins and converts them into updater metadata.
        /// </summary>
        private IList<PluginInfo> getPluginList()
        {
            if (StateStorage.Instance().Host == null || StateStorage.Instance().Host.MainWindow == null)
            {
                return Enumerable.Empty<PluginInfo>().ToList();
            }

            Form mainWindow = StateStorage.Instance().Host.MainWindow;
            if (mainWindow.InvokeRequired)
            {
                return (IList<PluginInfo>)mainWindow.Invoke(new Func<IList<PluginInfo>>(getPluginList));
            }

            return getPluginListInternal();
        }

        /// <summary>
        /// Collects the currently loaded KeePass plugins on the UI thread.
        /// </summary>
        private IList<PluginInfo> getPluginListInternal()
        {
            List<PluginInfo> plugins = new List<PluginInfo>();

            if (StateStorage.Instance().Host == null)
            {
                return Enumerable.Empty<PluginInfo>().ToList();
            }

            // Use reflection to access the private PluginManager property
            PropertyInfo pluginManagerProperty = typeof(MainForm).GetProperty("PluginManager", BindingFlags.NonPublic | BindingFlags.Instance);
            if (pluginManagerProperty == null)
            {
                return plugins;
            }

            object pluginManager = pluginManagerProperty.GetValue(StateStorage.Instance().Host.MainWindow);
            if (pluginManager == null)
            {
                return plugins;
            }

            Type pluginManagerType = pluginManager.GetType();

            // Use reflection to call the GetEnumerator method on the PluginManager
            MethodInfo methodGetEnumerator = pluginManagerType.GetMethod("GetEnumerator", BindingFlags.Public | BindingFlags.Instance);
            if (methodGetEnumerator != null)
            {
                // Call the GetEnumerator method to get the enumerator for the plugins
                object result = methodGetEnumerator.Invoke(pluginManager, null);
                if (result == null)
                {
                    return plugins;
                }

                Type enumeratorType = result.GetType();

                // Use reflection to call MoveNext on the enumerator
                MethodInfo methodMoveNext = enumeratorType.GetMethod("MoveNext", BindingFlags.Public | BindingFlags.Instance);
                PropertyInfo propertyCurrent = enumeratorType.GetProperty("Current", BindingFlags.Public | BindingFlags.Instance);
                if (methodMoveNext == null || propertyCurrent == null)
                {
                    return plugins;
                }

                while ((bool)methodMoveNext.Invoke(result, null))
                {
                    // Use reflection to get the Current property of the enumerator
                    object plugin = propertyCurrent.GetValue(result, null);
                    if (plugin == null)
                    {
                        continue;
                    }

                    Type pluginType = plugin.GetType();

                    // Use reflection to get the properties of the plugin
                    PropertyInfo nameProperty = pluginType.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
                    PropertyInfo fileVersionProperty = pluginType.GetProperty("FileVersion", BindingFlags.Public | BindingFlags.Instance);
                    PropertyInfo interfaceProperty = pluginType.GetProperty("Interface", BindingFlags.Public | BindingFlags.Instance);


                    plugins.Add(new PluginInfo
                    {
                        Name = nameProperty != null && nameProperty.GetValue(plugin) != null ? nameProperty.GetValue(plugin).ToString() : "Unknown",
                        CurrentVersionStr = fileVersionProperty != null && fileVersionProperty.GetValue(plugin) != null ? fileVersionProperty.GetValue(plugin).ToString() : string.Empty,
                        UpdateUrl = GetPluginUpdateUrl(interfaceProperty, plugin)
                    });

                }

                return plugins;
            }


            return Enumerable.Empty<PluginInfo>().ToList();
        }

        /// <summary>
        /// Downloads and installs plugin updates that were identified as newer versions.
        /// </summary>
        private async Task updatePlugins(bool forceUpdate)
        {
            if (!forceUpdate && !StateStorage.Instance().Settings.AdditionalSettings.IsUpdateEnabled)
            {
                return; // Skip updates if the setting is disabled
            }

            string pluginDir = Path.Combine(KeePassLib.Utility.UrlUtil.GetFileDirectory(KeePass.Util.WinUtil.GetExecutable(), bAppendTerminatingChar: false, bEnsureValidDirSpec: true), AppDefs.PluginsDir);
            Directory.CreateDirectory(pluginDir);

            foreach (PluginInfo pluginInfo in StateStorage.Instance().Settings.PluginList)
            {
                if (!pluginInfo.HasUpdate || string.IsNullOrEmpty(pluginInfo.DownloadUrl) || !HasVersionPlaceholder(pluginInfo.DownloadUrl))
                {
                    continue; // Skip plugins without a versioned download URL
                }

                string downloadUrl = ReplaceVersionPlaceholder(pluginInfo.DownloadUrl, pluginInfo.LatestVersionStr);
                Uri downloadUri = new Uri(downloadUrl);
                string filename = GetNormalizedArtifactFileName(downloadUri, pluginInfo.LatestVersionStr);
                bool isZip = downloadUri.AbsolutePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);
                bool isPlgx = downloadUri.AbsolutePath.EndsWith(".plgx", StringComparison.OrdinalIgnoreCase);
                try
                {
                    using (HttpClient httpClient = new HttpClient())
                    {
                        byte[] pluginData = await httpClient.GetByteArrayAsync(downloadUrl);

                        RemoveExistingPluginArtifacts(pluginDir, filename);

                        if (isZip)
                        {
                            string zipFilePath = Path.Combine(pluginDir, filename);
                            File.WriteAllBytes(zipFilePath, pluginData);
                            ExtractZipFile(zipFilePath, pluginDir);
                            File.Delete(zipFilePath);
                        }
                        else
                        {
                            string filePath = Path.Combine(pluginDir, filename);
                            File.WriteAllBytes(filePath, pluginData);
                        }

                        // Update the plugin info with the new version
                        pluginInfo.CurrentVersionStr = pluginInfo.LatestVersionStr;
                        this.updatedPlugins.Add(pluginInfo.Name); // Add to the list of updated plugins

                        Console.WriteLine("Updated {0} to version {1}", pluginInfo.Name, pluginInfo.CurrentVersionStr);
                        if (isPlgx)
                        {
                            ClearPluginCache();
                        }

                        StateStorage.Instance().RestartRequired = true; // Indicate that a restart is required to apply the update
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error updating {0}: {1}", pluginInfo.Name, ex.Message);
                }
            }
        }

        /// <summary>
        /// Removes the KeePass PLGX cache so updated packages are compiled on next start.
        /// </summary>
        private static void ClearPluginCache()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrEmpty(localAppData))
            {
                return;
            }

            string pluginCacheDir = Path.Combine(localAppData, "KeePass", "PluginCache");
            if (!Directory.Exists(pluginCacheDir))
            {
                return;
            }

            TryDeleteDirectory(pluginCacheDir);
        }

        private static void TryDeleteDirectory(string directoryPath)
        {
            try
            {
                Directory.Delete(directoryPath, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not delete plugin cache directory {0}: {1}", directoryPath, ex.Message);
            }
        }

        /// <summary>
        /// Returns whether a download URL contains a supported version placeholder.
        /// </summary>
        private static bool HasVersionPlaceholder(string downloadUrl)
        {
            return downloadUrl.Contains("<version>") || downloadUrl.IndexOf("%3Cversion%3E", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Replaces supported version placeholders in a download URL.
        /// </summary>
        private static string ReplaceVersionPlaceholder(string downloadUrl, string version)
        {
            return downloadUrl
                .Replace("<version>", version)
                .Replace("%3Cversion%3E", version)
                .Replace("%3cversion%3e", version);
        }

        /// <summary>
        /// Builds a normalized artifact file name by removing the version suffix from the downloaded file name when possible.
        /// </summary>
        private static string GetNormalizedArtifactFileName(Uri downloadUri, string version)
        {
            string fileName = Path.GetFileName(downloadUri.LocalPath);
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(version))
            {
                return fileName;
            }

            string extension = Path.GetExtension(fileName);
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            if (nameWithoutExtension.EndsWith(version, StringComparison.OrdinalIgnoreCase))
            {
                nameWithoutExtension = nameWithoutExtension.Substring(0, nameWithoutExtension.Length - version.Length)
                    .TrimEnd(' ', '-', '_', 'v', 'V');
            }

            return nameWithoutExtension + extension;
        }

        /// <summary>
        /// Removes existing files or folders that match the same plugin base name as the new artifact.
        /// </summary>
        private static void RemoveExistingPluginArtifacts(string pluginDir, string newFileName)
        {
            string targetBaseName = NormalizeArtifactBaseName(newFileName);

            foreach (string existingPath in Directory.GetFileSystemEntries(pluginDir))
            {
                string existingName = Path.GetFileName(existingPath);
                string existingBaseName = NormalizeArtifactBaseName(existingName);

                if (!string.Equals(existingBaseName, targetBaseName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (Directory.Exists(existingPath))
                {
                    Directory.Delete(existingPath, true);
                }
                else if (File.Exists(existingPath))
                {
                    File.Delete(existingPath);
                }
            }
        }

        /// <summary>
        /// Normalizes an artifact file name to a version-independent base name.
        /// </summary>
        private static string NormalizeArtifactBaseName(string fileName)
        {
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName ?? string.Empty);
            if (string.IsNullOrEmpty(nameWithoutExtension))
            {
                return string.Empty;
            }

            return Regex.Replace(nameWithoutExtension, @"([\s\-_]?v?\d+(?:\.\d+){1,4})$", string.Empty, RegexOptions.IgnoreCase).TrimEnd(' ', '-', '_');
        }

        /// <summary>
        /// Reads the update URL from the loaded plugin interface in a C# 5 compatible way.
        /// </summary>
        private static string GetPluginUpdateUrl(PropertyInfo interfaceProperty, object plugin)
        {
            if (interfaceProperty == null)
            {
                return string.Empty;
            }

            object pluginInterface = interfaceProperty.GetValue(plugin);
            Plugin keePassPlugin = pluginInterface as Plugin;
            if (keePassPlugin == null)
            {
                return string.Empty;
            }

            return keePassPlugin.UpdateUrl ?? string.Empty;
        }

        /// <summary>
        /// Extracts a ZIP package into the plugin directory.
        /// </summary>
        private static void ExtractZipFile(string zipFilePath, string destinationDirectory)
        {
            Assembly compressionAssembly = Assembly.Load("System.IO.Compression");
            Type zipArchiveType = compressionAssembly.GetType("System.IO.Compression.ZipArchive");
            Type zipArchiveModeType = compressionAssembly.GetType("System.IO.Compression.ZipArchiveMode");

            if (zipArchiveType == null || zipArchiveModeType == null)
            {
                throw new InvalidOperationException("ZIP support is not available.");
            }

            string destinationRoot = Path.GetFullPath(destinationDirectory.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);

            using (FileStream zipFileStream = new FileStream(zipFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                object zipArchiveMode = Enum.ToObject(zipArchiveModeType, 0);
                object archive = Activator.CreateInstance(zipArchiveType, zipFileStream, zipArchiveMode);

                try
                {
                    dynamic archiveDynamic = archive;
                    foreach (object entry in archiveDynamic.Entries)
                    {
                        dynamic entryDynamic = entry;
                        string entryPath = ((string)entryDynamic.FullName).Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
                        string destinationPath = Path.GetFullPath(Path.Combine(destinationDirectory, entryPath));

                        if (!destinationPath.StartsWith(destinationRoot, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (string.IsNullOrEmpty((string)entryDynamic.Name))
                        {
                            Directory.CreateDirectory(destinationPath);
                            continue;
                        }

                        string directoryName = Path.GetDirectoryName(destinationPath);
                        if (!string.IsNullOrEmpty(directoryName))
                        {
                            Directory.CreateDirectory(directoryName);
                        }

                        using (Stream entryStream = (Stream)entryDynamic.Open())
                        using (FileStream destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            entryStream.CopyTo(destinationStream);
                        }
                    }
                }
                finally
                {
                    IDisposable disposableArchive = archive as IDisposable;
                    if (disposableArchive != null)
                    {
                        disposableArchive.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Shows the restart notification if updates were applied and notifications are enabled.
        /// </summary>
        private void restartApplication()
        {
            if (StateStorage.Instance().RestartRequired && StateStorage.Instance().Settings.AdditionalSettings.ShowUpdateNotification)
            {
                ShowRestartNotification();
            }
        }

        /// <summary>
        /// Shows the restart notification on the KeePass UI thread.
        /// </summary>
        private void ShowRestartNotification()
        {
            if (StateStorage.Instance().Host == null || StateStorage.Instance().Host.MainWindow == null)
            {
                return;
            }

            Form mainWindow = StateStorage.Instance().Host.MainWindow;
            if (mainWindow.InvokeRequired)
            {
                mainWindow.Invoke(new Action(ShowRestartNotification));
                return;
            }

            DialogResult dialogResult = MessageBox.Show("The following plugins have been updated: " + string.Join(", ", this.updatedPlugins) + ". The application must be restarted for the updates to take effect", "Restart Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (dialogResult != DialogResult.OK)
                {
                    return; // User chose not to restart
                }
        }


        /// <summary>
        /// Checks for updates for each plugin in the provided list by making HTTP requests to their update URLs.
        /// </summary>
        /// <returns></returns>
        /// <summary>
        /// Checks all configured plugins for newer versions by querying their update endpoints.
        /// </summary>
        private async Task checkForPluginUpdates()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                foreach (PluginInfo pluginInfo in StateStorage.Instance().Settings.PluginList)
                {
                    if (string.IsNullOrEmpty(pluginInfo.UpdateUrl))
                    {
                        continue; // Skip plugins without an update URL
                    }
                    try
                    {
                        HttpResponseMessage response = await httpClient.GetAsync(pluginInfo.UpdateUrl);
                        if (response.IsSuccessStatusCode)
                        {
                            string content = await response.Content.ReadAsStringAsync();
                            string[] splitContent = content.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                            int index = splitContent.ToList().FindIndex(m => m.Contains(pluginInfo.Name));
                            if (index < 0)
                            {
                                continue;
                            }

                            string pluginVersionStr = splitContent[index];
                            string newVersionStr = pluginVersionStr.Split(':').LastOrDefault();
                            if (string.IsNullOrEmpty(newVersionStr))
                            {
                                continue;
                            }

                            Version newVersion = new Version(newVersionStr);

                            pluginInfo.LatestVersionStr = newVersionStr;
                        }
                    }
                    catch (Exception ex)
                    {
                    Console.WriteLine("Error checking updates for {0}: {1}", pluginInfo.Name, ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Loads persisted settings from the KeePass custom configuration storage.
        /// </summary>
        private void loadSettings()
        {
            string settings = StateStorage.Instance().Host.CustomConfig.GetString("PluginUpdater");
            if (string.IsNullOrEmpty(settings))
            {
                return; // No settings found, nothing to load
            }

            try
            {
                SettingsItem settingsPlugin = DeserializeSettings(settings);
                if (settingsPlugin.PluginList == null)
                {
                    settingsPlugin.PluginList = new List<PluginInfo>();
                }

                if (settingsPlugin.AdditionalSettings == null)
                {
                    settingsPlugin.AdditionalSettings = new AdditionalSettings();
                }

                // Update the download URLs for each plugin in the main plugin list
                foreach (PluginInfo item in StateStorage.Instance().Settings.PluginList)
                {
                            PluginInfo matchingPlugin = settingsPlugin.PluginList.FirstOrDefault(p => p.Name == item.Name);
                            item.DownloadUrl = matchingPlugin != null ? matchingPlugin.DownloadUrl : null;
                }

                StateStorage.Instance().Settings.AdditionalSettings = settingsPlugin.AdditionalSettings;
            }
            catch (Exception ex)
            {
                    Console.WriteLine("Error loading settings: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Saves the current settings to the custom configuration of the plugin host.
        /// </summary>
        /// <summary>
        /// Serializes the current settings and stores them in the KeePass custom configuration.
        /// </summary>
        public void SaveSettings()
        {
            string pluginsSettings = SerializeSettings(StateStorage.Instance().Settings);
            StateStorage.Instance().Host.CustomConfig.SetString("PluginUpdater", pluginsSettings);
            StateStorage.Instance().SettingsForm.Close();
            StateStorage.Instance().Host.MainWindow.SaveConfig(); // Save the configuration after updating settings
        }

        /// <summary>
        /// Serializes settings without requiring third-party assemblies in PLGX builds.
        /// </summary>
        private static string SerializeSettings(SettingsItem settings)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(SettingsItem));

            using (MemoryStream stream = new MemoryStream())
            {
                serializer.WriteObject(stream, settings);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        /// <summary>
        /// Deserializes settings without requiring third-party assemblies in PLGX builds.
        /// </summary>
        private static SettingsItem DeserializeSettings(string settings)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(SettingsItem));

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(settings)))
            {
                SettingsItem result = serializer.ReadObject(stream) as SettingsItem;
                return result ?? new SettingsItem();
            }
        }

        /// <summary>
        /// Sets the download URL for a specific plugin in the plugin list.
        /// </summary>
        /// <param name="pluginName"></param>
        /// <param name="downloadUrl"></param>
        /// <summary>
        /// Updates the configured download URL for a single plugin.
        /// </summary>
        public void SetDownloadUrl(string pluginName, string downloadUrl)
        {
            PluginInfo pluginInfo = StateStorage.Instance().Settings.PluginList.FirstOrDefault(p => p.Name == pluginName);
            if (pluginInfo != null)
            {
                pluginInfo.DownloadUrl = downloadUrl;
            }
        }
    }
}
