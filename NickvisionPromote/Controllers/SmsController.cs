using Microsoft.AspNetCore.Mvc;
using NickvisionPromote.Models.Configuration;
using System;
using Twilio.AspNet.Common;
using Twilio.AspNet.Core;
using Twilio.TwiML;

namespace NickvisionPromote.Controllers
{
    [Route("sms")]
    public class SmsController : TwilioController
    {
        private static string _optOutNumbersPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Nickvision\\NickvisionPromote\\optOut.txt";
        private static string _optInNumbersPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Nickvision\\NickvisionPromote\\optIn.txt";

        public TwiMLResult Index(SmsRequest incomingMessage)
        {
            var configuration = Configuration.LoadAsync().GetAwaiter().GetResult();
            MessagingResponse messagingResponse = null;
            if (incomingMessage.Body != null)
            {
                var msg = incomingMessage.Body.ToLower();
                if (msg == "start")
                {
                    messagingResponse = new MessagingResponse();
                    messagingResponse.Message(configuration.StartMessage);
                    System.IO.File.AppendAllTextAsync(_optInNumbersPath, incomingMessage.From + "\n");
                }
                else if (msg == "stop")
                {
                    messagingResponse = new MessagingResponse();
                    messagingResponse.Message(configuration.StopMessage);
                    System.IO.File.AppendAllTextAsync(_optOutNumbersPath, incomingMessage.From + "\n");
                }
            }
            return messagingResponse == null ? null : TwiML(messagingResponse);
        }
    }
}
