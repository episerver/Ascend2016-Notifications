using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Notification;

namespace Ascend2016.Business.NotificationDemo
{
    /// <summary>
    /// Simple demonstration of how PostNotificationAsync() is all you need to send notifications to the UI directly.
    /// </summary>
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class SimpleNotification : IInitializableModule
    {
        private INotifier _notifier;

        private void OnPublishedContent(object sender, EPiServer.ContentEventArgs e)
        {
            var page = e.Content as IChangeTrackable;

            if (page != null)
            {
                _notifier.PostNotificationAsync(new NotificationMessage
                {
                    // These needs to be set, but they don't have to match a formatter.
                    ChannelName = "SomeChannelName",
                    TypeName = "SomeTypeName",

                    // These have to be set so the notification knows who sent it, where to go, and what to show.
                    Sender = new NotificationUser("jojoh"),
                    Recipients = new[]
                    {
                        new NotificationUser("jojoh"), // Note: For demo, so I always get notified.
                        new NotificationUser(page.ChangedBy)
                    },
                    Subject = "Site Announcement",
                    Content = $"{page.ChangedBy} published the page {e.Content.Name}!"
                });
            }
        }

        #region Not important for Notifications API demonstration

        private IContentEvents _contentEvents;

        public void Initialize(InitializationEngine context)
        {
            _notifier = context.Locate.Advanced.GetInstance<INotifier>();
            _contentEvents = context.Locate.Advanced.GetInstance<IContentEvents>();

            _contentEvents.PublishedContent += OnPublishedContent;
        }

        public void Uninitialize(InitializationEngine context)
        {
            _contentEvents.PublishedContent -= OnPublishedContent;
        }

        #endregion
    }
}
