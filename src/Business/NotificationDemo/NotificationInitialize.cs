using System.Collections.Generic;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Notification;
using Tweetinvi;

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
        private Dictionary<string, string> _iftttUsers;

        /// <summary>
        /// Registers default preference.
        /// Our demo uses the NotificationFormatter with the IftttNotificationProvider.
        /// </summary>
        /// <param name="context">Context to get a <see cref="INotificationPreferenceRegister"/></param>
        public void Initialize(InitializationEngine context)
        {
            SetupTwitterAppAccount();

            // Our IFTTT users
            _iftttUsers = new Dictionary<string, string>
            {
                {"jojoh", "bXuSaOmXdNZDpbg4GZrmcZ"},
                {"bemc", "_RljrWAUh2O4x_0lP56tk"}
            };


            // Get preference register
            var preferencesRegister = context.Locate.Advanced
                .GetInstance<INotificationPreferenceRegister>();

            // Register a provider to handle the "twitter" channel
            preferencesRegister.RegisterDefaultPreference(
                NotificationFormatter.ChannelName,
                IftttNotificationProvider.Name,
                // Fetch IFTTT keys or filter out the user
                username => 
                    _iftttUsers.ContainsKey(username) ?
                    _iftttUsers[username] :
                    null);
        }

        #region Not important for Notifications API demonstration

        public void Uninitialize(InitializationEngine context)
        {
        }

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



        #endregion
    }
}
