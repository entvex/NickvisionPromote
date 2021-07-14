using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace NickvisionPromote.Models.Update
{
    public class UpdateConfig
    {
        public string LatestVersion { get; set; }
        public string LinkToInstaller { get; set; }
        public string Changelog { get; set; }

        public UpdateConfig()
        {
            LatestVersion = "";
            LinkToInstaller = "";
            Changelog = "";
        }

        public static async Task<UpdateConfig> LoadFromWebAsync(string linkToConfig)
        {
            var dataDir = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Nickvision\\NickvisionPromote";
            var pathToDownload = $"{dataDir}\\updateConfig.json";
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }
            try
            {
                using var client = new WebClient();
                await client.DownloadFileTaskAsync(linkToConfig, pathToDownload);
                var json = await File.ReadAllTextAsync(pathToDownload);
                return JsonSerializer.Deserialize<UpdateConfig>(json);
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> SaveToDiskAsync()
        {
            var pathToSave = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\updateConfig.json";
            var json = JsonSerializer.Serialize(this);
            await File.WriteAllTextAsync(pathToSave, json);
            return pathToSave;
        }
    }
}
