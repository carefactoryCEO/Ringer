using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Distribute;
using Microsoft.AppCenter.Push;
using Ringer.Helpers;
using Ringer.Services;
using Ringer.Views;
using Xamarin.Forms;

namespace Ringer
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(ChatPage), typeof(ChatPage));
            Routing.RegisterRoute("permissionspage", typeof(PermissionsPage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        }

        public static string RoomId { get; private set; }

        public static async Task<AppShell> GetInstance()
        {
            var _messaging = DependencyService.Get<IMessaging>();

            #region AppCenter
            if (!AppCenter.Configured)
            {
                // Intercept Push Notification
                Push.PushNotificationReceived += async (sender, e) =>
                {
                    string body = null;
                    string pushSender = null;

                    if (e.CustomData != null)
                    {
                        foreach (var key in e.CustomData.Keys)
                        {
                            Utility.Trace($"------(push){key}: {e.CustomData[key]}");
                            switch (key)
                            {
                                case "room":
                                    RoomId = e.CustomData[key];
                                    break;

                                case "body":
                                    body = e.CustomData[key];
                                    break;

                                case "sender":
                                    pushSender = e.CustomData[key];
                                    break;

                                default:
                                    break;
                            }
                        }
                    }

                    await _messaging.FetchRemoteMessagesAsync();
                    await Shell.Current.GoToAsync(Constants.ChatPageUriFromPushNotification);
                };
            }

            AppCenter.Start(Constants.AppCenterAndroid + Constants.AppCenteriOS,
                typeof(Analytics), typeof(Crashes), typeof(Push), typeof(Distribute));

            Analytics.TrackEvent("Ringer started");

            if (await Push.IsEnabledAsync())
            {
                Guid? id = await AppCenter.GetInstallIdAsync();

                Debug.WriteLine("-------------------------");
                Debug.WriteLine($"device id: {id}");
                Debug.WriteLine("-------------------------");

                if (id != null)
                    App.DeviceId = id?.ToString();
            }
            #endregion

            return new AppShell();
        }
    }
}
