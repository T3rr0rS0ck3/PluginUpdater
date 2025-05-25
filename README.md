# PluginUpdater

A KeePass 2 plugin to automatically update other plugins.

## Features

- Checks for updates for installed plugins.
- Downloads and installs new plugin versions.
- Notifies the user when a restart is required.
- Allows configuration of plugin update URLs and update settings.

## Installation

1. **Download:**
   - Download the latest release of `PluginUpdater.dll` from the [Releases](https://github.com/T3rr0rS0ck3/PluginUpdater/releases).
2. **Install the Plugin:**
   - Copy the downloaded `PluginUpdater.dll` file to your KeePass plugins directory. This is usually located at:
     ```
     C:\Program Files\KeePass Password Safe 2\Plugins\
     ```
   - Alternatively, you can find your plugins folder by opening KeePass and going to `Tools > Plugins > Open Folder`.

3. **Restart KeePass:**
   - Restart KeePass to load the PluginUpdater.

## Usage

- After installation, a new menu item for PluginUpdater will appear in KeePass.
- Open the PluginUpdater settings to configure plugin download URLs and enable/disable automatic updates or notifications.
- The plugin will check for updates and notify on you if any plugins need to be updated or if a restart is required.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.