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

namespace Ringer
{
    public partial class App : Application
    {
        #region private members
        private readonly IMessaging _messaging;
        #endregion

        #region public static propertie
        public static bool IsChatPage => Shell.Current.CurrentState.Location.ToString()
                                        .EndsWith("chatpage", StringComparison.CurrentCultureIgnoreCase);
        public static bool IsLoggedIn => Token != null &&
                                         DeviceId != null &&
                                         UserName != null &&
                                         RoomId != null;
        public static int LastServerMessageId
        {
            get => Preferences.Get(nameof(LastServerMessageId), 0);
            set => Preferences.Set(nameof(LastServerMessageId), value);
        }
        public static string DeviceId
        {
            get => Preferences.Get(nameof(DeviceId), null);
            set => Preferences.Set(nameof(DeviceId), value);
        }
        public static string Token
        {
            get => Preferences.Get(nameof(Token), null);
            set => Preferences.Set(nameof(Token), value);
        }
        public static string UserName
        {
            get => Preferences.Get(nameof(UserName), null);
            set => Preferences.Set(nameof(UserName), value);
        }
        public static int UserId
        {
            get => Preferences.Get(nameof(UserId), -1);
            set => Preferences.Set(nameof(UserId), value);
        }
        public static string ConnectionId
        {
            get => Preferences.Get(nameof(ConnectionId), null);
            set => Preferences.Set(nameof(ConnectionId), value);
        }
        public static string RoomId
        {
            get => Preferences.Get(nameof(RoomId), null);
            set => Preferences.Set(nameof(RoomId), value);
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
            NotificationCenter.Current.NotificationTapped += e => Utilities.Trace(e.Data);

            // essaging event
            _messaging.Connecting += (s, e) => Utilities.Trace(e.Message);
            _messaging.Connected += (s, e) => Utilities.Trace(e.Message);
            _messaging.ConnectionFailed += (s, e) => Utilities.Trace(e.Message);

            _messaging.Disconnecting += (s, e) => Utilities.Trace(e.Message);
            _messaging.Disconnected += (s, e) => Utilities.Trace(e.Message);
            _messaging.DisconnectionFailed += (s, e) => Utilities.Trace(e.Message);

            _messaging.Closed += (s, e) => Utilities.Trace(e.Message);
            _messaging.Reconnecting += (s, e) => Utilities.Trace(e.Message);
            _messaging.Reconnected += (s, e) => Utilities.Trace(e.Message);
        }
        #endregion


        #region Life Cycle Methods
        protected override async void OnStart()
        {
            base.OnStart();

            if (DesignMode.IsDesignModeEnabled)
                return;

            #region AppCenter
            // Intercept Push Notification
            if (!AppCenter.Configured)
            {
                Push.PushNotificationReceived += async (sender, e) =>
                {
                    string body = null;
                    string pushSender = null;
                    // If there is custom data associated with the notification,
                    // print the entries
                    if (e.CustomData != null)
                    {
                        foreach (var key in e.CustomData.Keys)
                        {
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

                    if (RoomId != null && !IsChatPage)
                    {
                        await Shell.Current.Navigation.PopToRootAsync(false);
                        await Shell.Current.GoToAsync($"//mappage/chatpage?room={RoomId}", false);
                    }
                };
            }

            AppCenter.Start(Constants.AppCenterAndroid + Constants.AppCenteriOS, typeof(Analytics), typeof(Crashes), typeof(Push));

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
                ConnectionId = await _messaging.InitAsync(Constants.HubUrl, Token);

            #endregion
        }

        protected override void OnSleep()
        {
            Debug.WriteLine($"{DateTime.Now.Millisecond}:OnSleep");
            //await _restService.ReportDeviceStatusDebouncedAsync(false);
            //await _restService.ReportDeviceStatusDebouncedAsync(false, 1000);
            //_restService.ReportDeviceStatus(false);
            base.OnSleep();

            Utilities.Trace(IsChatPage.ToString());
        }
        protected override async void OnResume()
        {
            //try
            //{
            Debug.WriteLine($"{DateTime.Now.Millisecond}:OnResume");

            //_restService.ReportDeviceStatus(IsChatPage);
            //await _restService.ReportDeviceStatusDebouncedAsync(IsChatPage);

            if (IsLoggedIn)
            {
                if (!_messaging.IsReconnecting)
                    await _messaging.ConnectAsync().ConfigureAwait(false);//OnResume

            }
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine(ex.Message);
            //}


            Utilities.Trace(IsChatPage.ToString());
        }
        #endregion
    }
}
