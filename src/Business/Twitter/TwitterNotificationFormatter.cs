using System.Collections.Generic;
using System.Linq;
using EPiServer.Framework.Serialization;
using EPiServer.Notification;
using EPiServer.ServiceLocation;

namespace Ascend2016.Business.Twitter
{
    /// <summary>
    /// Handling formation of notifications and instant user notifications.
    /// </summary>
    [ServiceConfiguration(typeof(INotificationFormatter))]
    public class TwitterNotificationFormatter : INotificationFormatter, IUserNotificationFormatter
    {
        private readonly IObjectSerializer _objectSerializer;

        public TwitterNotificationFormatter(IObjectSerializer objectSerializer)
        {
            _objectSerializer = objectSerializer;
        }

        // This isn't meant to be here by design, but forced by necessity to have a channel name written down somewhere as a constant.
        public const string ChannelName = "epi.ascend2016.twitter.channel";

        /// <summary>
        /// Gets the list of channels supported by this formatter
        /// </summary>
        public IEnumerable<string> SupportedChannelNames => new[] { ChannelName };

        /// <summary>
        /// Gets the name of the formatter
        /// </summary>
        public string FormatterName => "epi.ascend2016.twitter.formatter";

        public IEnumerable<FormatterNotificationMessage> FormatMessages(IEnumerable<FormatterNotificationMessage> notifications, string recipient, NotificationFormat format, string channelName)
        {
            return notifications
                    // Remove duplicates // TODO: This isn't always needed, but I'm unsure of what caused me to get previously sent notifications over and over.
                    .GroupBy(x => x.Content)
                    .Select(x => x.First())
                    // Format message content
                    .Select(FormatMessageContent)
                    .ToArray();
        }

        public UserNotificationMessage FormatUserMessage(UserNotificationMessage message)
        {
            var data = _objectSerializer.Deserialize<TweetedPageViewModel>(message.Content);

            message.Subject = $@"Your article ""{data.PageName}"" is going viral!";
            message.Content = $"Your article has {data.ShareCount} tweets and retweets!";
            message.Link = data.ContentLink;
            return message;
        }

        private FormatterNotificationMessage FormatMessageContent(FormatterNotificationMessage message)
        {
            var data = _objectSerializer.Deserialize<TweetedPageViewModel>(message.Content);

            message.Content = $@"Your article ""{data.PageName}"" has {data.ShareCount} tweets and retweets!";
            return message;
        }
    }
}
