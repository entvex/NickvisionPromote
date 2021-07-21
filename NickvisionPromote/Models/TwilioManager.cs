using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using NgrokSharp;
using NgrokSharp.DTO;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using JsonSerializer = System.Text.Json.JsonSerializer;


namespace NickvisionPromote.Models
{
    public class TwilioManager : IDisposable
    {
        private CancellationTokenSource _incommingServerCancellationToken;
        private INgrokManager _ngrokManager;
        private bool _downloadedNgrokThisSession;

        public string AccountSID { get; set; }
        public string AuthToken { get; set; }

        public TwilioManager(string accountSID = "", string authToken = "")
        {
            AccountSID = accountSID;
            AuthToken = authToken;
            _incommingServerCancellationToken = new CancellationTokenSource();
            _ngrokManager = new NgrokManager();
            _downloadedNgrokThisSession = false;
        }

        public async Task RunIncommingServerAsync()
        {
            using var webHost = WebHost.CreateDefaultBuilder().UseKestrel().UseStartup<WebStartup>().UseUrls("http://localhost:5001").Build();
            await webHost.RunAsync(_incommingServerCancellationToken.Token);
        }

        public void StopIncommingServer()
        {
            _incommingServerCancellationToken.Cancel();
            _ngrokManager.StopNgrok();
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

        public async Task<string> StartPortForwardWithNgrokAsync(string apiKey)
        {
            if(!_downloadedNgrokThisSession)
            {
                await _ngrokManager.DownloadAndUnzipNgrokAsync();
                _downloadedNgrokThisSession = true;
            }
            await _ngrokManager.RegisterAuthTokenAsync(apiKey);
            _ngrokManager.StartNgrok();
            var tunnel = new StartTunnelDTO
            {
                name = "Incomming Messages Server",
                proto = "http",
                addr = "5001"
            };
            var httpResponseMessage = await _ngrokManager.StartTunnelAsync(tunnel);

            if ((int)httpResponseMessage.StatusCode == 201)
            {
                var tunnelDetail =
                    JsonConvert.DeserializeObject<TunnelDetailDTO>(
                        await httpResponseMessage.Content.ReadAsStringAsync());

                return $"{tunnelDetail.PublicUrl}/sms";
            }
            else
            {
                var tunnelError = JsonSerializer.Deserialize<TunnelErrorDTO>(await httpResponseMessage.Content.ReadAsStringAsync());
                throw new ApplicationException(tunnelError.Msg);
            }
        }

        public void Dispose() => _incommingServerCancellationToken.Dispose();
    }
}
