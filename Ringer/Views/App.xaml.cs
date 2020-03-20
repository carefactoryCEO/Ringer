using System;
using System.Diagnostics;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Push;
using Xamarin.Essentials;
using Xamarin.Forms;
using Ringer.Services;
using Ringer.Helpers;
using Plugin.LocalNotification;
using System.Threading.Tasks;

namespace Ringer
{
    public partial class App : Application
    {
        #region private members
        private readonly IMessaging _messaging;
        #endregion

        #region public static propertie
        public static bool IsOn { get; set; }
        public static bool IsChatPage { get; set; }
        public static bool IsCameraActivated { get; set; }

        public static bool IsLoggedIn => Token != null &&
                                         DeviceId != null &&
                                         UserName != null &&
                                         RoomId != null;
        public static int LocalNotificationId
        {
            get => Preferences.Get(nameof(LocalNotificationId), -1);
            set => Preferences.Set(nameof(LocalNotificationId), value);
        }
        public static string Token
        {
            get => Preferences.Get(nameof(Token), null);
            set => Preferences.Set(nameof(Token), value);
        }

        public static int UserId
        {
            get => Preferences.Get(nameof(UserId), -1);
            set => Preferences.Set(nameof(UserId), value);
        }
        public static string UserName
        {
            get => Preferences.Get(nameof(UserName), null);
            set => Preferences.Set(nameof(UserName), value);
        }

        public static string DeviceId
        {
            get => Preferences.Get(nameof(DeviceId), null);
            set => Preferences.Set(nameof(DeviceId), value);
        }
        public static string LastConnectionId
        {
            get => Preferences.Get(nameof(LastConnectionId), null);
            set => Preferences.Set(nameof(LastConnectionId), value);
        }
        public static string RoomId
        {
            get => Preferences.Get(nameof(RoomId), null);
            set => Preferences.Set(nameof(RoomId), value);
        }
        public static int LastServerMessageId
        {
            get => Preferences.Get(nameof(LastServerMessageId), 0);
            set => Preferences.Set(nameof(LastServerMessageId), value);
        }
        #endregion

        #region Constructor
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();

            // Services
            DependencyService.Register<ILocalDbService, LocalDbService>();
            DependencyService.Register<IRESTService, RESTService>();
            DependencyService.Register<IMessaging, Messaging>();
            _messaging = DependencyService.Get<IMessaging>();

            // Local Notification
            NotificationCenter.Current.NotificationTapped += Current_NotificationTapped;

            // essaging event
            _messaging.Connecting += (s, e) => Utilities.Trace(e.Message);
            _messaging.Connected += (s, e) => Utilities.Trace(e.Message);
            _messaging.ConnectionFailed += (s, e) => Utilities.Trace(e.Message, true);

            _messaging.Disconnecting += (s, e) => Utilities.Trace(e.Message);
            _messaging.Disconnected += (s, e) => Utilities.Trace(e.Message);
            _messaging.DisconnectionFailed += (s, e) => Utilities.Trace(e.Message, true);

            _messaging.Reconnecting += (s, e) => Utilities.Trace(e.Message);
            _messaging.Reconnected += (s, e) => Utilities.Trace(e.Message);
            _messaging.Closed += (s, e) => Utilities.Trace(e.Message, true);
        }
        #endregion

        #region private methods
        private void Current_NotificationTapped(NotificationTappedEventArgs e)
        {
            Utilities.Trace($"------local: {e.Data}");

            Shell.Current.GoToAsync(Constants.ChatPageUriFromLocalNotification);
        }
        #endregion

        #region Life Cycle Methods
        protected override async void OnStart()
        {
            base.OnStart();

            IsOn = true;

            if (DesignMode.IsDesignModeEnabled)
                return;

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
                            Utilities.Trace($"------(push){key}: {e.CustomData[key]}");
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

                    //await Shell.Current.GoToAsync(Constants.ChatPageUriFromPushNotification);
                };
            }

            AppCenter.Start(Constants.AppCenterAndroid + Constants.AppCenteriOS,
                typeof(Analytics), typeof(Crashes), typeof(Push));

            Analytics.TrackEvent("Ringer started");

            if (await Push.IsEnabledAsync())
            {
                Guid? id = await AppCenter.GetInstallIdAsync();

                Debug.WriteLine("-------------------------");
                Debug.WriteLine($"device id: {id}");
                Debug.WriteLine("-------------------------");

                if (id != null)
                    DeviceId = id?.ToString();
            }
            #endregion

            #region Connect and load messages
            if (IsLoggedIn)
                LastConnectionId = await _messaging.InitAsync(Constants.HubUrl, Token);
            #endregion
        }
        protected override void OnSleep()
        {
            Utilities.Trace("-------------OnSleep-------------");

            IsOn = false;

            LastConnectionId = _messaging.ConnectionId;

            //if (Utilities.iOS)
            //{
            //    Xamarin.Forms.Device.StartTimer(TimeSpan.FromSeconds(2), () =>
            //    {
            //        if (!IsOn)
            //            _messaging.DisconnectAsync();

            //        return false;
            //    });
            //}

            base.OnSleep();
        }
        protected override async void OnResume()
        {
            Utilities.Trace("------------OnResume------------");
            Utilities.Trace($"Kept Connecting: {LastConnectionId == _messaging.ConnectionId} last connection id: {LastConnectionId}, current: {_messaging.ConnectionId}");

            base.OnResume();

            IsOn = true;

            if (IsLoggedIn)
            {
                await _messaging.EnsureConnected().ConfigureAwait(false);

                if (LastConnectionId != _messaging.ConnectionId) // sleep 도중에 끊어져서 재연결했음 -> 그 사이 서버에 있었던 거 긁어와야지.
                    await _messaging.FetchRemoteMessagesAsync();

            }
        }
        #endregion
    }
}
