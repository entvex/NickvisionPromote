using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace NickvisionPromote.Models
{
    public class TwilioManager : IDisposable
    {
        private IWebHost _webHost;

        public string AccountSID { get; set; }
        public string AuthToken { get; set; }

        public TwilioManager(string accountSID = "", string authToken = "")
        {
            AccountSID = accountSID;
            AuthToken = authToken;
            _webHost = WebHost.CreateDefaultBuilder().UseKestrel().UseStartup<WebStartup>().UseUrls("http://localhost:5001").Build();
            _webHost.Start();
        }

        public async Task<int> SendMessageAsync(PhoneNumberDatabase phoneDatabase, string fromNumber, string message, string mediaURL)
        {
            TwilioClient.Init(AccountSID, AuthToken);
            var phoneNumbers = await phoneDatabase.GetPhoneNumbersAsync();
            int successCount = 0;
            foreach (var phoneNumber in phoneNumbers)
            {
                List<Uri> mediaUrl = null;
                if (!string.IsNullOrEmpty(mediaURL))
                {
                    mediaUrl = new List<Uri>() { new Uri(mediaURL) };
                }
                var result = await MessageResource.CreateAsync(body: message, from: new PhoneNumber(fromNumber), to: new PhoneNumber(phoneNumber.NumberString), mediaUrl: mediaUrl);
                if (result.Status != MessageResource.StatusEnum.Failed)
                {
                    successCount++;
                }
            }
            return successCount;
        }

        public void Dispose() => _webHost.Dispose();
    }
}
