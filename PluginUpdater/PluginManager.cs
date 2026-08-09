using KeePass.App;
using KeePass.Forms;
using KeePass.Plugins;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PluginUpdater
{
    /// <summary>
    /// Manages the plugin updates for KeePass by checking for available updates, downloading them, and applying them.
    /// </summary>
    public class PluginManager
    {
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
            this.updatedPlugins.Clear();
            StateStorage.Instance().RestartRequired = false;
            StateStorage.Instance().Settings.PluginList = getPluginList();
            loadSettings();
            await checkForPluginUpdates();
            await updatePlugins(forceUpdate);
            restartApplication();
        }

        /// <summary>
        /// Retrieves the list of plugins using reflection to access the private PluginManager property of the MainForm.
        /// </summary>
        /// <returns></returns>
        private IList<PluginInfo> getPluginList()
        {
            List<PluginInfo> plugins = new List<PluginInfo>();

            if (StateStorage.Instance().Host == null)
            {
                return Enumerable.Empty<PluginInfo>().ToList();
            }

            // Use reflection to access the private PluginManager property
            PropertyInfo pluginManagerProperty = typeof(MainForm).GetProperty("PluginManager", BindingFlags.NonPublic | BindingFlags.Instance);
            object pluginManager = pluginManagerProperty.GetValue(StateStorage.Instance().Host.MainWindow);
            Type pluginManagerType = pluginManager.GetType();

            // Use reflection to call the GetEnumerator method on the PluginManager
            MethodInfo methodGetEnumerator = pluginManagerType.GetMethod("GetEnumerator", BindingFlags.Public | BindingFlags.Instance);
            if (methodGetEnumerator != null)
            {
                // Call the GetEnumerator method to get the enumerator for the plugins
                object result = methodGetEnumerator.Invoke(pluginManager, null);
                Type enumeratorType = result.GetType();

                // Use reflection to call MoveNext on the enumerator
                MethodInfo methodMoveNext = enumeratorType.GetMethod("MoveNext", BindingFlags.Public | BindingFlags.Instance);
                while ((bool)methodMoveNext.Invoke(result, null))
                {
                    // Use reflection to get the Current property of the enumerator
                    PropertyInfo propertyCurrent = enumeratorType.GetProperty("Current", BindingFlags.Public | BindingFlags.Instance);
                    object plugin = propertyCurrent.GetValue(result, null);
                    Type pluginType = plugin.GetType();

                    // Use reflection to get the properties of the plugin
                    PropertyInfo nameProperty = pluginType.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
                    PropertyInfo fileVersionProperty = pluginType.GetProperty("FileVersion", BindingFlags.Public | BindingFlags.Instance);
                    PropertyInfo interfaceProperty = pluginType.GetProperty("Interface", BindingFlags.Public | BindingFlags.Instance);


                    plugins.Add(new PluginInfo
                    {
                        Name = nameProperty?.GetValue(plugin)?.ToString() ?? "Unknown",
                        CurrentVersionStr = fileVersionProperty?.GetValue(plugin)?.ToString() ?? "",
                        UpdateUrl = (interfaceProperty?.GetValue(plugin) as Plugin)?.UpdateUrl ?? string.Empty
                    });

                }

                return plugins;
            }


            return Enumerable.Empty<PluginInfo>().ToList();
        }

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
                if (!pluginInfo.HasUpdate || string.IsNullOrEmpty(pluginInfo.DownloadUrl) || !pluginInfo.DownloadUrl.Contains("<version>"))
                {
                    continue; // Skip plugins without a versioned download URL
                }

                string downloadUrl = pluginInfo.DownloadUrl.Replace("<version>", pluginInfo.LatestVersionStr);
                Uri downloadUri = new Uri(downloadUrl);
                string filename = Path.GetFileName(downloadUri.LocalPath);
                bool isZip = downloadUri.AbsolutePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);
                try
                {
                    using (HttpClient httpClient = new HttpClient())
                    {
                        byte[] pluginData = await httpClient.GetByteArrayAsync(downloadUrl);

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

                        Console.WriteLine($"Updated {pluginInfo.Name} to version {pluginInfo.CurrentVersionStr}");

                        StateStorage.Instance().RestartRequired = true; // Indicate that a restart is required to apply the update
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating {pluginInfo.Name}: {ex.Message}");
                }
            }
        }

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

        private void restartApplication()
        {
            if (StateStorage.Instance().RestartRequired && StateStorage.Instance().Settings.AdditionalSettings.ShowUpdateNotification)
            {
                DialogResult dialogResult = MessageBox.Show($"The following plugins have been updated: {string.Join(", ", this.updatedPlugins)}. The application must be restarted for the updates to take effect", "Restart Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (dialogResult != DialogResult.OK)
                {
                    return; // User chose not to restart
                }
            }
        }


        /// <summary>
        /// Checks for updates for each plugin in the provided list by making HTTP requests to their update URLs.
        /// </summary>
        /// <returns></returns>
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

                            string pluginVersionStr = splitContent[index];
                            string newVersionStr = pluginVersionStr.Split(':').LastOrDefault();
                            Version newVersion = new Version(newVersionStr);

                            pluginInfo.LatestVersionStr = newVersionStr;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error checking updates for {pluginInfo.Name}: {ex.Message}");
                    }
                }
            }
        }

        private void loadSettings()
        {
            string settings = StateStorage.Instance().Host.CustomConfig.GetString("PluginUpdater");
            if (string.IsNullOrEmpty(settings))
            {
                return; // No settings found, nothing to load
            }

            try
            {
                SettingsItem settingsPlugin = JsonConvert.DeserializeObject<SettingsItem>(settings);

                // Update the download URLs for each plugin in the main plugin list
                foreach (PluginInfo item in StateStorage.Instance().Settings.PluginList)
                {
                    item.DownloadUrl = settingsPlugin.PluginList.FirstOrDefault(p => p.Name == item.Name)?.DownloadUrl;
                }

                StateStorage.Instance().Settings.AdditionalSettings = settingsPlugin.AdditionalSettings;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves the current settings to the custom configuration of the plugin host.
        /// </summary>
        public void SaveSettings()
        {
            string pluginsSettings = JsonConvert.SerializeObject(StateStorage.Instance().Settings);
            StateStorage.Instance().Host.CustomConfig.SetString("PluginUpdater", pluginsSettings);
            StateStorage.Instance().SettingsForm.Close();
            StateStorage.Instance().Host.MainWindow.SaveConfig(); // Save the configuration after updating settings
        }

        /// <summary>
        /// Sets the download URL for a specific plugin in the plugin list.
        /// </summary>
        /// <param name="pluginName"></param>
        /// <param name="downloadUrl"></param>
        public void SetDownloadUrl(string pluginName, string downloadUrl)
        {
            StateStorage.Instance().Settings.PluginList.FirstOrDefault(p => p.Name == pluginName).DownloadUrl = downloadUrl;
        }
    }
}
