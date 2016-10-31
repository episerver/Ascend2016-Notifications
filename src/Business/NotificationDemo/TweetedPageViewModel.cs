using System;

namespace Ascend2016.Business.NotificationDemo
{
    /// <summary>
    /// Simple view model for storing data used by the formatters.
    /// </summary>
    public struct TweetedPageViewModel
    {
        public string PageName;
        public int ShareCount;
        public Uri ContentLink;
    }
}
