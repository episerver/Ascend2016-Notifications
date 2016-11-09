using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Framework.Serialization;
using EPiServer.Notification;
using EPiServer.ServiceLocation;

namespace Ascend2016.Business.NotificationDemo
{
    /// <summary>
    /// Handling formation of notifications and instant user notifications.
    /// </summary>
    /// TODO: Implement as two classes.
    [ServiceConfiguration(typeof(INotificationFormatter))]
    public class NotificationFormatter : INotificationFormatter, IUserNotificationFormatter
    {
        private readonly IObjectSerializer _objectSerializer;

        // This doesn't have to be here, but it's convenient.
        public const string ChannelName = "epi.ascend2016.twitter.channel";

        /// Implements <see cref="INotificationFormatter"/>
        public IEnumerable<string> SupportedChannelNames => new[] { ChannelName };
        public string FormatterName => "epi.ascend2016.twitter.formatter";

        /// <summary>
        /// Implements <see cref="INotificationFormatter"/>
        /// Delayed notification. In our demo handled by <see cref="IftttNotificationProvider"/>.
        /// </summary>
        /// <param name="notifications">Notifications to send out.</param>
        /// <param name="recipient">Recipient of the notification. Transformed and filtered by <see cref="IftttNotificationProvider"/>.</param>
        /// <param name="format">Parameters for supported format by <see cref="IftttNotificationProvider"/></param>
        /// <param name="channelName">Channel name, but we only support one so it's ignored here.</param>
        /// <returns>The formatted messages.</returns>
        public IEnumerable<FormatterNotificationMessage> FormatMessages(
            IEnumerable<FormatterNotificationMessage> notifications,
            string recipient,
            NotificationFormat format,
            string channelName)
        {
            // Join messages with the same content.
            var groupedMessages = notifications.GroupBy(x => x.Content);
            foreach (var group in groupedMessages)
            {
                // Get the serialized content data
                var data = _objectSerializer.Deserialize<TweetedPageViewModel>(group.Last().Content);

                // Respect the provider's Format.
                var content = $@"Your article ""{data.PageName}"" has {data.ShareCount} shares!";
                if (format.MaxLength.HasValue)
                {
                    content = content.Substring(0, Math.Min(content.Length, format.MaxLength.Value));
                }

                // Mark all ID's as processed (otherwise the dispatcher will try again with them)
                var messageIds = group.SelectMany(y => y.ContainedIDs);
                var formattedMessage = new FormatterNotificationMessage(messageIds)
                {
                    Content = content
                };

                yield return formattedMessage;
            }
        }

        /// <summary>
        /// Implements <see cref="IUserNotificationFormatter"/>
        /// Instant user notification. It's displayed in the Editor view (bell icon).
        /// </summary>
        /// <param name="message">Message to be formatted.</param>
        /// <returns>The formatted message.</returns>
        public UserNotificationMessage FormatUserMessage(UserNotificationMessage message)
        {
            var data = _objectSerializer.Deserialize<TweetedPageViewModel>(message.Content);

            message.Subject = $@"Your article ""{data.PageName}"" is going viral!";
            message.Content = $"Your article has {data.ShareCount} tweets and retweets!";
            message.Link = data.ContentLink;
            return message;
        }

        #region Not important for Notifications API demonstration

        public NotificationFormatter(IObjectSerializer objectSerializer)
        {
            _objectSerializer = objectSerializer;
        }

        #endregion
    }
}
