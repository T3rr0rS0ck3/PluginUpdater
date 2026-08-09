# PluginUpdater

`PluginUpdater` is a KeePass 2 plugin that checks installed plugins for updates and downloads new versions automatically.

## Features

- Checks installed plugins for available updates
- Downloads and installs new plugin versions
- Supports `.plgx` files and `.zip` packages
- Shows a restart notification after updates
- Allows per-plugin configuration of update and download URLs

## Installation

1. **Download**
   - Download the latest release of `PluginUpdater.dll` from the [Releases](https://github.com/T3rr0rS0ck3/PluginUpdater/releases) page.

2. **Copy to the KeePass plugin folder**
   - Copy `PluginUpdater.dll` into your KeePass plugins directory.
   - The folder is usually located at:

     ```
     C:\Program Files\KeePass Password Safe 2\Plugins\
     ```

   - You can also open the folder directly in KeePass via `Tools > Plugins > Open Folder`.

3. **Adjust permissions**
   - The user or the Windows group `Users` must have **Full Control** on the KeePass `Plugins` directory.
   - Without write permissions the plugin cannot replace or create update files.

4. **Restart KeePass**
   - Restart KeePass to load `PluginUpdater`.

## Usage

- After installation, a new `PluginUpdater` menu entry appears in KeePass.
- Open the settings to configure plugin download URLs and update behavior.
- The download URL must contain the placeholder `<version>`.
- The plugin replaces `<version>` with the latest detected version before downloading.
- If notifications are enabled, a message is shown when updates require a restart.

## Notes

- Some plugins publish releases as a single `.plgx` file.
- Others publish a `.zip` archive containing one or more files.
- `PluginUpdater` supports both formats.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
