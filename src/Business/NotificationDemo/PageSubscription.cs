using System;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Notification;

namespace Ascend2016.Business.NotificationDemo
{
    /// <summary>
    /// Set up subscriptions so editors receive notifications for published content they contributed too.
    /// </summary>
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class PageSubscription : IInitializableModule
    {
        public static Uri BaseUri = new Uri("ascend://twitter/content/");

        private IContentEvents _contentEvents;
        private ISubscriptionService _subscriptionService;

        /// <summary>
        /// Eventhandler for PublishedContent that adds the page's ChangedBy
        /// user to a subscription based on the page's ContentLink ID.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPublishedContent(object sender, EPiServer.ContentEventArgs e)
        {
            var page = e.Content as IChangeTrackable;

            if (page == null)
            {
                return;
            }

            // Create a subscription
            var key = new Uri(BaseUri, e.Content.ContentLink.ID.ToString());
            var user = new NotificationUser(page.ChangedBy);

            _subscriptionService.SubscribeAsync(key, user).Wait();
        }

        #region Not important for Notifications API demonstration

        public void Initialize(InitializationEngine context)
        {
            _contentEvents = context.Locate.Advanced.GetInstance<IContentEvents>();
            _subscriptionService = context.Locate.Advanced.GetInstance<ISubscriptionService>();

            _contentEvents.PublishedContent += OnPublishedContent;
        }

        public void Uninitialize(InitializationEngine context)
        {
            _contentEvents.PublishedContent -= OnPublishedContent;
        }

        #endregion
    }
}
