using System;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Notification;

namespace Ascend2016.Business.Twitter
{
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class TwitterSubscription : IInitializableModule
    {
        private IContentEvents _contentEvents;
        private ISubscriptionService _subscriptionService;
        private const string SubscriptionKeyBase = "ascend://twitter/content";

        public void Initialize(InitializationEngine context)
        {
            _contentEvents = context.Locate.Advanced.GetInstance<IContentEvents>();
            _subscriptionService = context.Locate.Advanced.GetInstance<ISubscriptionService>();

            // TMP: Only for demo purposes
            _subscriptionService.SubscribeAsync(new Uri("ascend://twitter/content/6"), new NotificationUser("jojoh")).Wait();

            _contentEvents.PublishedContent += OnPublishedContent;
        }

        public void Uninitialize(InitializationEngine context)
        {
            _contentEvents.PublishedContent -= OnPublishedContent;
        }

        public static Uri SubscriptionKey(ContentReference contentLink)
        {
            return new Uri($"{SubscriptionKeyBase}/{contentLink.ID}");
        }

        private void OnPublishedContent(object sender, EPiServer.ContentEventArgs e)
        {
            // TODO: Tweet the article?

            var page = e.Content as IChangeTrackable;

            if (page == null)
            {
                return;
            }

            // Make a subscriptionKey
            var contentLinkId = e.Content.ContentLink;
            var subscriptionKey = SubscriptionKey(contentLinkId);

            // Subscribe all recipients of the notification, including the sender of the notification
            _subscriptionService.SubscribeAsync(subscriptionKey, new NotificationUser(page.ChangedBy)).Wait();
        }
    }
}
