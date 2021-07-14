using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace NickvisionPromote.Models.Update
{
    public class Updater
    {
        private string _linkToConfig;
        private Version _currentApplicationVersion;
        private UpdateConfig _updateConfig;
        private bool _updateAvailable;

        public bool UpdateAvailable => _updateAvailable && _updateConfig != null;
        public Version LatestVersion => _updateConfig == null ? null : new Version(_updateConfig.LatestVersion);
        public string Changelog => _updateConfig.Changelog ?? null;

        public Updater(string linkToConfig, Version currentApplicationVersion)
        {
            _linkToConfig = linkToConfig;
            _currentApplicationVersion = currentApplicationVersion;
            _updateConfig = null;
            _updateAvailable = false;
        }

        public async Task<bool> CheckForUpdatesAsync()
        {
            _updateConfig = await UpdateConfig.LoadFromWebAsync(_linkToConfig);
            if (_updateConfig != null && LatestVersion > _currentApplicationVersion)
            {
                _updateAvailable = true;
            }
            return UpdateAvailable;
        }

        public async Task<bool> UpdateAsync(Window windowToClose)
        {
            if (!UpdateAvailable)
            {
                return false;
            }
            var dataDir = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Nickvision\\NickvisionPromote";
            var pathToDownload = $"{dataDir}\\Setup.exe";
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }
            try
            {
                using var client = new WebClient();
                await client.DownloadFileTaskAsync(_updateConfig.LinkToInstaller, pathToDownload);
            }
            catch
            {
                return false;
            }
            Process.Start(new ProcessStartInfo() { FileName = pathToDownload, UseShellExecute = true, Verb = "runas" });
            windowToClose.Close();
            return true;
        }
    }
}
