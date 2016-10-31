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
        private IContentEvents _contentEvents;
        private ISubscriptionService _subscriptionService;
        private const string SubscriptionKeyBase = "ascend://twitter/content";

        /// <summary>
        /// Generate a subscription key for a <see cref="ContentReference"/>.
        /// It's also used publicly to find subscribers for a given page.
        /// </summary>
        /// <param name="contentLink"></param>
        /// <returns>Uri to use with <see cref="ISubscriptionService"/>.</returns>
        public static Uri SubscriptionKey(ContentReference contentLink)
        {
            return new Uri($"{SubscriptionKeyBase}/{contentLink.ID}");
        }

        /// <summary>
        /// Add subscribers for a page.
        /// </summary>
        /// <param name="contentLink">Used as a subscription key.</param>
        /// <param name="page">Page with relevant users to subscribe.</param>
        private void Subscribe(ContentReference contentLink, IChangeTrackable page)
        {
            // Create a subscriptionKey
            var subscriptionKey = SubscriptionKey(contentLink);

            // Select a notification receiver
            var receiver = new NotificationUser(page.ChangedBy);

            // Subscribe all recipients of the notification, including the sender of the notification
            _subscriptionService.SubscribeAsync(subscriptionKey, receiver).Wait();
        }

        #region Not important for Notifications API demonstration

        public void Initialize(InitializationEngine context)
        {
            _contentEvents = context.Locate.Advanced.GetInstance<IContentEvents>();
            _subscriptionService = context.Locate.Advanced.GetInstance<ISubscriptionService>();

            // TMP: Only for demo purposes
            _subscriptionService.SubscribeAsync(SubscriptionKey(new ContentReference(6)), new NotificationUser("jojoh")).Wait();

            _contentEvents.PublishedContent += OnPublishedContent;
        }

        public void Uninitialize(InitializationEngine context)
        {
            _contentEvents.PublishedContent -= OnPublishedContent;
        }

        private void OnPublishedContent(object sender, EPiServer.ContentEventArgs e)
        {
            // TODO: Tweet the article?

            var page = e.Content as IChangeTrackable;

            if (page == null)
            {
                return;
            }

            Subscribe(e.Content.ContentLink, page);
        }

        #endregion
    }
}
