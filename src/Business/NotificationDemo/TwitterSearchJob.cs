using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Framework.Serialization;
using EPiServer.Notification;
using EPiServer.PlugIn;
using EPiServer.Scheduler;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Tweetinvi;
using Tweetinvi.Core.Interfaces;

namespace Ascend2016.Business.NotificationDemo
{
    /// <summary>
    /// Scheduled job that searches Twitter for shared <see cref="PageData"/> and notifies the users subscribing to it.
    /// </summary>
    [ScheduledPlugIn(DisplayName = "Twitter Search", IntervalLength = 10, IntervalType = ScheduledIntervalType.Seconds)]
    public class TwitterSearchJob : ScheduledJobBase
    {
        private readonly Injected<INotifier> _notifier;
        private readonly Injected<ISubscriptionService> _subscriptionService;
        private readonly IObjectSerializer _objectSerializer;

        /// <summary>
        /// Will be called periodically with every page that's been recently published.
        /// It will then notify the subscribers about their page's retweet count.
        /// </summary>
        /// <param name="page">The relevant page to check for tweet count</param>
        /// <returns>True if the subscribers were notified</returns>
        private bool NotifyPageSubscribers(PageData page)
        {
            // Get Twitter shares for the page
            var lastShareCount = CountShares(OldTweets(page));
            var currentShareCount = CountShares(GetUpdatedTweets(page));

            // UserNotifications are sent directly and can't be batched in the formatter,
            // so we only notify if the tweet count doubled.
            if (currentShareCount <= lastShareCount * 2)
            {
                return false;
            }

            // Find relevant notification receivers with the Subscription service
            var subscriptionKey = PageSubscription.SubscriptionKey(page.ContentLink);
            var recipients = _subscriptionService.Service.FindSubscribersAsync(subscriptionKey).Result.ToArray();
            // If there's no one to notify we can skip the rest
            if (!recipients.Any())
            {
                return false;
            }

            // Create notification
            const string sender = "jojoh";
            var notificationMessage = CreateNotificationMessage(sender, recipients, page, currentShareCount);

            // Send the notification
            _notifier.Service.PostNotificationAsync(notificationMessage).Wait();

            return true;
        }

        /// <summary>
        /// Creates the notification message.
        /// </summary>
        /// <param name="sender">Author of the message</param>
        /// <param name="recipients">Recipients of the message</param>
        /// <param name="page">The page shared on Twitter</param>
        /// <param name="shareCount">Number of tweets and retweets for the page</param>
        /// <returns>NotificationMessage with serialized data in the Content property</returns>
        private NotificationMessage CreateNotificationMessage(string sender, IEnumerable<INotificationUser> recipients, PageData page, int shareCount)
        {
            var tweetData = new TweetedPageViewModel
            {
                PageName = page.Name,
                ShareCount = shareCount,
                ContentLink = new Uri("epi.cms.contentdata:///" + page.ContentLink)
            };
            var serializedViewModel = _objectSerializer.Serialize(tweetData);

            return new NotificationMessage
            {
                ChannelName = NotificationFormatter.ChannelName,
                TypeName = "Tweet",
                Sender = new NotificationUser(sender),
                Recipients = recipients,
                Content = serializedViewModel
            };
        }

        #region Not important for Notifications API demonstration

        private bool _stopSignaled;
        private static readonly Dictionary<string, IEnumerable<ITweet>> TwitterCache = new Dictionary<string, IEnumerable<ITweet>>();

        public TwitterSearchJob()
        {
            IsStoppable = true;

            var objectSerializerFactory = new Injected<IObjectSerializerFactory>();
            _objectSerializer = objectSerializerFactory.Service.GetSerializer(KnownContentTypes.Json);

            SetupTwitterAppAccount();
        }

        public override void Stop()
        {
            _stopSignaled = true;
        }

        // TODO: Move this to NotificationInitialize?
        private static void SetupTwitterAppAccount()
        {
            var consumerKey = System.Configuration.ConfigurationManager.AppSettings["TwitterConsumerKey"];
            var consumerSecret = System.Configuration.ConfigurationManager.AppSettings["TwitterConsumerSecret"];

            // If you do not already have a BearerToken, use the TRUE parameter to automatically generate it.
            // Note that this will result in a WebRequest to be performed and you will therefore need to make this code safe
            var appCreds = Auth.SetApplicationOnlyCredentials(consumerKey, consumerSecret, true);
            // This method execute the required webrequest to set the bearer Token
            Auth.InitializeApplicationOnlyCredentials(appCreds);
        }

        public override string Execute()
        {
            // Call OnStatusChanged to periodically notify progress of job for manually started jobs
            OnStatusChanged($"Starting execution of {GetType()}");

            // For long running jobs periodically check if stop is signaled and if so stop execution
            if (_stopSignaled)
            {
                return "Stop of job was called";
            }

            // Get the latest published pages
            const int numDaysBacklog = 120;
            var pages = GetLatestPublishedContent(DateTime.Now.AddDays(-numDaysBacklog)).ToArray();
            if (!pages.Any())
            {
                return $"No pages published in the last {numDaysBacklog} days.";
            }

            // Send notifications to everyone subscribing to these pages
            var notificationsCount = 0;
            foreach (var page in pages)
            {
                if (NotifyPageSubscribers(page))
                {
                    notificationsCount++;
                }
            }

            var totalShareCount = TwitterCache.Sum(x => CountShares(x.Value));
            return $"Found {pages.Length} pages that were tweeted {totalShareCount} times. Sent {notificationsCount} notifications.";
        }

        private static ITweet[] OldTweets(PageData page)
        {
            var url = ExternalUrl(page.ContentLink, CultureInfo.CurrentCulture);
            return TwitterCache.ContainsKey(url) ? TwitterCache[url].ToArray() : new ITweet[0];
        }

        private static int CountShares(IEnumerable<ITweet> tweets)
        {
            var tweetsArray = tweets.ToArray();
            return tweetsArray.Length + tweetsArray.Sum(x => x.RetweetCount);
        }

        private static IEnumerable<ITweet> GetUpdatedTweets(PageData page)
        {
            // Search Twitter for URL's of the page.
            // Note: For demo purposes I can uncomment these for searches I know will return results.
            //var tweets = new[] { Tweet.GetTweet(780929654924378112) };
            //var tweets = Search.SearchTweets("https://medium.com/@shemag8/fuck-you-startup-world-ab6cc72fad0e");
            var url = ExternalUrl(page.ContentLink, CultureInfo.CurrentCulture);
            var tweets = Search.SearchTweets(url).ToArray();

            // Cache handling. Twitter only gives the most recent tweets and we want to accumulate them to give a better total retweet count.
            var cachedTweets = OldTweets(page);
            var union = tweets
                .Union(cachedTweets) // ITweet doesn't support comparison so we have to filter out duplicates ourselves for now.
                .GroupBy(tweet => tweet.Id)
                .Select(group => group.First())
                .ToArray();
            TwitterCache[url] = union;

            return tweets;
        }

        private static IEnumerable<PageData> GetLatestPublishedContent(DateTime daysBack)
        {
            var criterias = new PropertyCriteriaCollection
            {
                new PropertyCriteria
                {
                    Condition = EPiServer.Filters.CompareCondition.GreaterThan,
                    Name = "PageChanged",
                    Type = PropertyDataType.Date,
                    Value = daysBack.ToString(CultureInfo.InvariantCulture),
                    Required = true
                }
            };

            var newsPageItems = DataFactory.Instance
                .FindPagesWithCriteria(PageReference.StartPage, criterias)
                // Only keep those with a user set otherwise the demo won't show anything. (also: PropertyCriteria can only search for null but we don't want empty strings either)
                .Where(x => !string.IsNullOrEmpty(x.ChangedBy));
            return newsPageItems;
        }

        private static string ExternalUrl(ContentReference contentLink, CultureInfo language)
        {
            // Borrowed from Henrik: http://stackoverflow.com/a/29934595/703921

            var virtualPathArguments = new VirtualPathArguments { ForceCanonical = true };
            var urlString = UrlResolver.Current.GetUrl(contentLink, language.Name, virtualPathArguments);

            if (string.IsNullOrEmpty(urlString) || HttpContext.Current == null)
            {
                return urlString;
            }

            var uri = new Uri(urlString, UriKind.RelativeOrAbsolute);
            if (uri.IsAbsoluteUri)
            {
                return urlString;
            }

            return new Uri(HttpContext.Current.Request.Url, uri).ToString();
        }

        #endregion
    }
}
