using System.Collections.Generic;
using EPiServer.Notification;
using EPiServer.ServiceLocation;

namespace Ascend2016.Business.Twitter
{
    /// <summary>
    /// Handling formation of notifications and instant user notifications.
    /// </summary>
    [ServiceConfiguration(typeof(IUserNotificationFormatter))]
    [ServiceConfiguration(typeof(INotificationFormatter))]
    public class TwitterNotificationFormatter : IUserNotificationFormatter, INotificationFormatter
    {
        public const string ChannelName = "epi-ascend-2016-twitter";

        /// <summary>
        /// Gets the list of channels supported by this formatter
        /// </summary>
        public IEnumerable<string> SupportedChannelNames => new[] { ChannelName };

        /// <summary>
        /// Gets the name of the formatter
        /// </summary>
        public string FormatterName => "Epi-DefaultNotificationFormatter";

        public IEnumerable<FormatterNotificationMessage> FormatMessages(IEnumerable<FormatterNotificationMessage> notifications, string recipient, NotificationFormat format, string channelName)
        {
            return notifications;
        }

        public UserNotificationMessage FormatUserMessage(UserNotificationMessage notification)
        {
            return notification;
        }
    }
}
