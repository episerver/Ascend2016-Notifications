using System.Collections.Generic;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Notification;

namespace Ascend2016.Business.NotificationDemo
{
    /// <summary>
    /// Initialize custom Notification preferences.
    /// Our demo will use IFTTT.com to send notifications through various channels.
    /// </summary>
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class NotificationInitialize : IInitializableModule
    {
        // Simple storage to lookup a user's alternative address.
        // It could be an email or other, but our demo uses the IFTTT Maker channel.
        private readonly Dictionary<string, string> _iftttUsers;

        /// <summary>
        /// Registers default preference.
        /// Our demo uses the NotificationFormatter with the IftttNotificationProvider.
        /// </summary>
        /// <param name="context">Context to get a <see cref="INotificationPreferenceRegister"/></param>
        public void Initialize(InitializationEngine context)
        {
            var preferencesRegister = context.Locate.Advanced
                .GetInstance<INotificationPreferenceRegister>();

            // register the IftttNotificationProvider to handle all notifications created on the "twitter" channel
            preferencesRegister.RegisterDefaultPreference(
                NotificationFormatter.ChannelName,
                IftttNotificationProvider.Name,
                // Fetch IFTTT keys for users that have one. Others will be filtered out and not sent to the formatter as receivers.
                // Could be used as user preferences so only certain users that wants this provider.
                username => _iftttUsers.ContainsKey(username) ? _iftttUsers[username] : null);
        }

        #region Not important for Notifications API demonstration

        public NotificationInitialize()
        {
            _iftttUsers = new Dictionary<string, string>
            {
                {"jojoh", "bXuSaOmXdNZDpbg4GZrmcZ"},
                {"bemc", "_RljrWAUh2O4x_0lP56tk"}
            };
        }

        public void Uninitialize(InitializationEngine context)
        {
        }

        #endregion
    }
}
