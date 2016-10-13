using System;
using System.Globalization;
using System.Linq;
using System.Web;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.PlugIn;
using EPiServer.Scheduler;
using EPiServer.Web.Routing;
using Tweetinvi;

namespace Ascend2016.Business.Twitter
{
    [ScheduledPlugIn(DisplayName = "Twitter Search", IntervalLength = 10, IntervalType = ScheduledIntervalType.Seconds)]
    public class TwitterSearchJob : ScheduledJobBase
    {
        private bool _stopSignaled;

        public TwitterSearchJob()
        {
            IsStoppable = true;

            var consumerKey = System.Configuration.ConfigurationManager.AppSettings["TwitterConsumerKey"];
            var consumerSecret = System.Configuration.ConfigurationManager.AppSettings["TwitterConsumerSecret"];

            // If you do not already have a BearerToken, use the TRUE parameter to automatically generate it.
            // Note that this will result in a WebRequest to be performed and you will therefore need to make this code safe
            var appCreds = Auth.SetApplicationOnlyCredentials(consumerKey, consumerSecret, true);
            // This method execute the required webrequest to set the bearer Token
            Auth.InitializeApplicationOnlyCredentials(appCreds);
        }

        /// <summary>
        /// Called when a user clicks on Stop for a manually started job, or when ASP.NET shuts down.
        /// </summary>
        public override void Stop()
        {
            _stopSignaled = true;
        }

        /// <summary>
        /// Called when a scheduled job executes
        /// </summary>
        /// <returns>A status message to be stored in the database log and visible from admin mode</returns>
        public override string Execute()
        {
            //Call OnStatusChanged to periodically notify progress of job for manually started jobs
            OnStatusChanged($"Starting execution of {GetType()}");

            //For long running jobs periodically check if stop is signaled and if so stop execution
            if (_stopSignaled)
            {
                return "Stop of job was called";
            }

            //Add implementation
            var page = GetLatestPublishedContent(DateTime.Now.AddDays(-120).ToString(CultureInfo.InvariantCulture));
            if (page == null)
                return "Fail...";

            var url = ExternalUrl(page.ContentLink, CultureInfo.CurrentCulture);

            var tweet = GetTweet(url);
            return tweet ?? $"No tweet found with {url}";
        }

        private string GetTweet(string url)
        {
            //var tweet = Tweet.GetTweet(780929654924378112);
            //var tweet = Search.SearchTweets("https://medium.com/@shemag8/fuck-you-startup-world-ab6cc72fad0e");
            var tweet = Search.SearchTweets(url);

            return tweet.FirstOrDefault()?.FullText;
        }

        private PageData GetLatestPublishedContent(string days)
        {
            var criterias = new PropertyCriteriaCollection();

            var criteria = new PropertyCriteria
            {
                Condition = EPiServer.Filters.CompareCondition.GreaterThan,
                Name = "PageChanged",
                Type = PropertyDataType.Date,
                Value = days,
                Required = true
            };

            criterias.Add(criteria);

            var newsPageItems = DataFactory.Instance.FindPagesWithCriteria(PageReference.StartPage, criterias);
            return newsPageItems.FirstOrDefault();
        }

        private static string ExternalUrl(ContentReference contentLink, CultureInfo language)
        {
            // Borrowed from Henrik: http://stackoverflow.com/a/29934595/703921

            var virtualPathArguments = new VirtualPathArguments {ForceCanonical = true};
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
    }
}
