using System;
using System.Collections.Generic;
using System.Net.Http;
using EPiServer.Notification;
using EPiServer.ServiceLocation;

namespace Ascend2016.Business.NotificationDemo
{
    /// <summary>
    /// Custom provider for sending messages through IFTTT, using
    /// their Maker-channel. This requires all users to have one and
    /// include their key in <see cref="NotificationInitialize"/>.
    /// </summary>
    [ServiceConfiguration(typeof(INotificationProvider))]
    public class IftttNotificationProvider : INotificationProvider
    {
        // This doesn't have to be here, but it's convenient.
        public const string Name = "IftttNotificationProvider";

        /// Implements <see cref="INotificationProvider"/>
        public string ProviderName => Name;

        /// <summary>
        /// Implements <see cref="INotificationProvider"/>.
        /// Supported format by this provider. The formatter will use it to properly format the messages.
        /// </summary>
        /// <returns>The supported format.</returns>
        public NotificationFormat GetProviderFormat()
        {
            return new NotificationFormat
            {
                MaxLength = 140,
                SupportsHtml = false
            };
        }

        /// <summary>
        /// Implements <see cref="INotificationProvider"/>.
        /// Sends messages to the IFTTT Maker-channel.
        /// </summary>
        /// <param name="messages">Messages to send. Already formatted by <see cref="NotificationFormatter"/>.</param>
        /// <param name="succeededAction">Callback to be called for each sent message.</param>
        /// <param name="failedAction">Callback to be called for each message that failed to send.</param>
        public void Send(IEnumerable<ProviderNotificationMessage> messages,
            Action<ProviderNotificationMessage> succeededAction,
            Action<ProviderNotificationMessage, Exception> failedAction)
        {
            using (var httpClient = new HttpClient())
            {
                foreach (var message in messages)
                {
                    // Recipients were converted to Maker-keys or removed by the setup in NotificationInitialize.
                    // Note: Currently it's always only one recipient.
                    foreach (var recipient in message.RecipientAddresses)
                    {
                        try
                        {
                            SendToIftttMakerChannel(httpClient, message, recipient);
                            succeededAction(message);
                        }
                        catch (Exception e)
                        {
                            failedAction(message, e);
                        }
                    }
                }
            }
        }

        #region Not important for Notifications API demonstration

        private static void SendToIftttMakerChannel(HttpClient httpClient, ProviderNotificationMessage message, string recipient)
        {
            var uri = $"https://maker.ifttt.com/trigger/tweets/with/key/{recipient}?value1={message.Content}";
            var response = httpClient.GetAsync(uri).Result;
            // Throw an exception if the call failed.
            response.EnsureSuccessStatusCode();
        }

        #endregion
    }
}
