using System.Collections.Generic;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Notification;

namespace Ascend2016.Business.Twitter
{
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class TwitterInitialize : IInitializableModule
    {
        private readonly Dictionary<string, string> _userIftttKeys;

        public TwitterInitialize()
        {
            _userIftttKeys = new Dictionary<string, string>
            {
                {"jojoh", "bXuSaOmXdNZDpbg4GZrmcZ"},
                {"bemc", "_RljrWAUh2O4x_0lP56tk"}
            };
        }

        public void Initialize(InitializationEngine context)
        {
            var preferencesRegister = context.Locate.Advanced.GetInstance<INotificationPreferenceRegister>();

            // register the IftttNotificationProvider to handle all notifications created on the "twitter" channel
            preferencesRegister.RegisterDefaultPreference(
                TwitterNotificationFormatter.ChannelName,
                IftttNotificationProvider.Name,
                x => _userIftttKeys.ContainsKey(x) ? _userIftttKeys[x] : null);
        }

        public void Uninitialize(InitializationEngine context)
        {
        }
    }
}
