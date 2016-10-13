using System.Linq;
using EPiServer.DataAbstraction;
using EPiServer.PlugIn;
using EPiServer.Scheduler;
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
            return GetTweet();
        }

        private string GetTweet()
        {
            //var tweet = Tweet.GetTweet(780929654924378112);
            var tweet = Search.SearchTweets("https://medium.com/@shemag8/fuck-you-startup-world-ab6cc72fad0e");

            return tweet.FirstOrDefault()?.FullText;
        }
    }
}
