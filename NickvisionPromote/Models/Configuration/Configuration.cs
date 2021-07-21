using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace NickvisionPromote.Models.Configuration
{
    public class Configuration
    {
        private static readonly string _configDir = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Nickvision\\NickvisionPromote";
        private static readonly string _configPath = $"{_configDir}\\config.json";

        [DisplayName("First Time Open")]
        [Description("Whether or not the application is being opened for the first time.\nCheck this box if you would like to run the first time setup the next time the application is started.")]
        public bool IsFirstTimeOpen { get; set; }

        [DisplayName("Theme")]
        [Description("The theme for the application.")]
        public AppTheme Theme { get; set; }

        [DisplayName("Ribbon Collapsed")]
        [Description("Whether or not the ribbon is collapsed.")]
        public bool IsRibbonCollapsed { get; set; }

        [DisplayName("Twilio Account SID")]
        [Description("The acount sid of your twilio account")]
        public string TwilioAccountSID { get; set; }

        [DisplayName("Twilio Auth Token")]
        [Description("The auth token of your twilio account")]
        public string TwilioAuthToken { get; set; }

        [DisplayName("Twilio From Number")]
        [Description("The from number of your twilio account")]
        public string TwilioFromNumber { get; set; }

        [DisplayName("Start Message")]
        [Description("The message when someone texts START to the twilio from number")]
        public string StartMessage { get; set; }

        [DisplayName("Stop Message")]
        [Description("The message when someone texts STOP to the twilio from number")]
        public string StopMessage { get; set; }

        [DisplayName("Ngrok API Key")]
        [Description("Your api key for ngrok. Required if Port Forwarding with Ngrok")]
        public string NgrokAPIKey { get; set; }

        public Configuration()
        {
            IsFirstTimeOpen = true;
            Theme = AppTheme.Light;
            IsRibbonCollapsed = false;
            TwilioAccountSID = "";
            TwilioAuthToken = "";
            TwilioFromNumber = "";
            StartMessage = "You have subscribed to receiving messages. You can text STOP at any time to opt out.";
            StopMessage = "You have unsubscribed to receiving messages. You can text START at any time to opt in again.";
            NgrokAPIKey = "";
        }

        public static async Task<Configuration> LoadAsync()
        {
            try
            {
                var json = await File.ReadAllTextAsync(_configPath);
                return JsonSerializer.Deserialize<Configuration>(json);
            }
            catch
            {
                return new Configuration();
            }
        }

        public async Task SaveAsync()
        {
            var json = JsonSerializer.Serialize(this);
            if (!Directory.Exists(_configDir))
            {
                Directory.CreateDirectory(_configDir);
            }
            await File.WriteAllTextAsync(_configPath, json);
        }
    }
}
