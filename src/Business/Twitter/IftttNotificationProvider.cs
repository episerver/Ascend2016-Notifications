using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using EPiServer.Notification;
using EPiServer.ServiceLocation;

namespace Ascend2016.Business.Twitter
{
    [ServiceConfiguration(typeof(INotificationProvider))]
    public class IftttNotificationProvider : INotificationProvider, INotificationProviderStatus
    {
        public const string Name = "IftttNotificationProvider";
        public string ProviderName => Name;

        private const string IftttUri = "https://maker.ifttt.com/trigger/tweets/with/key";

        public NotificationFormat GetProviderFormat()
        {
            return new NotificationFormat { MaxLength = 140, SupportsHtml = false };
        }

        public void Send(IEnumerable<ProviderNotificationMessage> messages,
            Action<ProviderNotificationMessage> succeededAction,
            Action<ProviderNotificationMessage, Exception> failedAction)
        {
            messages = messages
                // No point in sending to no recievers
                .Where(x => x.RecipientAddresses.Any()); // TODO: Is this unnecessary?

            using (var httpClient = new HttpClient())
            {
                foreach (var message in messages)
                {
                    foreach (var recipient in message.RecipientAddresses)
                    {
                        NotifyPageAuthor(httpClient, message, recipient, succeededAction, failedAction);
                    }
                }
            }
        }

        private static void NotifyPageAuthor(HttpClient httpClient, ProviderNotificationMessage message, string recipient,
            Action<ProviderNotificationMessage> succeededAction, Action<ProviderNotificationMessage, Exception> failedAction)
        {
            try
            {
                var uri = $"{IftttUri}/{recipient}?value1={message.Content}";
                var response = httpClient.GetAsync(uri).Result;
                response.EnsureSuccessStatusCode();

                succeededAction(message);
            }
            catch (Exception e)
            {
                failedAction(message, e);
            }
        }

        public bool IsDisabled => DateTime.Now.Year < 2007;

        public string DisabledReason => "The iPhone isn't released yet.";
    }
}
