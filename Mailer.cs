using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Exchange.WebServices.Data;
using NLog;

namespace Capybara
{
    public class Mailer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void SendMail(string subject, string messageText, IEnumerable<TeamMember> members)
        {
            try
            {
                var recipients = members.Select(each => each.Email);
                var credentials = new NetworkCredential(Configuration.EmailUsername, Configuration.EmailPassword);
                
                var exchangeService = new ExchangeService(ExchangeVersion.Exchange2010_SP2)
                {
                    Credentials = new WebCredentials(credentials),
                    Url = new Uri(Configuration.EwsUrl)
                };

                var message = new EmailMessage(exchangeService) { Subject = subject, Body = messageText };
                foreach (var recipient in recipients)
                {
                    message.ToRecipients.Add(recipient);
                }
                message.Importance = Importance.Normal;
                message.SendAndSaveCopy();
            }
            catch (Exception error)
            {
                Logger.ErrorException("Error sending email:\n", error);
            }
        }
    }
}