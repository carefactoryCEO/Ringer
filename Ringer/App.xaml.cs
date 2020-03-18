using System;
using System.Diagnostics;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Push;
using Xamarin.Essentials;
using Xamarin.Forms;
using Ringer.Core;
using Ringer.Core.EventArgs;
using Ringer.Services;
using Ringer.Helpers;
using Ringer.Models;
using System.Collections.ObjectModel;
using System.Linq;
using Plugin.LocalNotification;

namespace Ringer
{
    public partial class App : Application
    {
        #region private members
        private readonly IMessaging _messaging;
        #endregion

        #region public static propertie
        public static bool IsChatPage => Shell.Current.CurrentState.Location.ToString().EndsWith("chatpage", StringComparison.CurrentCultureIgnoreCase);
        public static bool IsLoggedIn => Token != null && DeviceId != null && UserName != null && RoomId != null;
        public static bool DeviceIsOn
        {
            get => Preferences.Get(nameof(DeviceIsOn), false);
            set => Preferences.Set(nameof(DeviceIsOn), value);
        }
        public static int LastServerMessageId
        {
            get => Preferences.Get(nameof(LastServerMessageId), 0);
            set => Preferences.Set(nameof(LastServerMessageId), value);
        }
        public static string DeviceId // Appcenter에서 받음
        {
            get => Preferences.Get(nameof(DeviceId), null);
            set => Preferences.Set(nameof(DeviceId), value);
        }
        public static string Token // 로그인 후에 받음
        {
            get => Preferences.Get(nameof(Token), null);
            set => Preferences.Set(nameof(Token), value);
        }
        public static string UserName // 로그인 과정 중에 받음
        {
            get => Preferences.Get(nameof(UserName), null);
            set => Preferences.Set(nameof(UserName), value);
        }
        public static int UserId
        {
            get => Preferences.Get(nameof(UserId), -1);
            set => Preferences.Set(nameof(UserId), value);
        }
        public static string RoomId
        {
            get => Preferences.Get(nameof(RoomId), null);
            set => Preferences.Set(nameof(RoomId), value);
        }
        public static ObservableCollection<MessageModel> Messages = new ObservableCollection<MessageModel>();
        #endregion

        #region Constructor
        public App()
        {
            InitializeComponent();

            // Local Notification
            NotificationCenter.Current.NotificationTapped += Current_NotificationTapped;

            // Main page
            MainPage = new AppShell();

            // Services
            DependencyService.Register<IMessaging, Messaging>();
            DependencyService.Register<ILocalDbService, LocalDbService>();
            DependencyService.Register<IMessageRepository, MessageRepository>();
            DependencyService.Register<IRESTService, RESTService>();

            _messaging = DependencyService.Resolve<IMessaging>();

            #region register Messaging service event handlers
            _messaging.Connecting += Trace_ConnectionStatus;
            _messaging.Connected += Trace_ConnectionStatus;
            _messaging.ConnectionFailed += ConnectionFailed;

            _messaging.Disconnecting += Trace_ConnectionStatus;
            _messaging.Disconnected += ConnectionFailed;
            _messaging.DisconnectionFailed += Trace_ConnectionStatus;

            _messaging.Closed += ConnectionFailed;
            _messaging.Reconnecting += Trace_ConnectionStatus;
            _messaging.Reconnected += Trace_ConnectionStatus;

            //_messagingService.SomeoneEntered += SomeoneEntered;
            //_messagingService.SomeoneLeft += SomeoneLeft;
            //_messagingService.MessageReceived += MessageReceived;

            #endregion
        }

        private void Current_NotificationTapped(NotificationTappedEventArgs e)
        {
            Debug.WriteLine($"noti data: {e.Data}");
        }
        #endregion

        #region Connection handlers
        private void ConnectionFailed(object sender, ConnectionEventArgs e)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
            {
                Shell.Current.DisplayAlert("이럴수가", e.Message, "닫기");
            });
        }
        private void Trace_ConnectionStatus(object sender, ConnectionEventArgs e)
        {
            Utilities.Trace(e.Message);
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

            //await _restService.ReportDeviceStatusDebouncedAsync(false);
            //_restService.ReportDeviceStatus(false);

            #region Connect and load messages
            if (IsLoggedIn)
            {
                _messaging.Init(Constants.HubUrl, Token);
                await _messaging.ConnectAsync().ConfigureAwait(false);//OnStart
            }
            #endregion


            //#region prepare messages
            //if (IsLoggedIn)
            //{
            //    var messages = await _messageRepository.GetMessagesAsync();
            //    foreach (var message in messages.OrderBy(m => m.Id))
            //    {
            //        Messages.Add(message);
            //    }
            //}
            //#endregion
        }
        protected override void OnSleep()
        {
            Debug.WriteLine($"{DateTime.Now.Millisecond}:OnSleep");
            //await _restService.ReportDeviceStatusDebouncedAsync(false);
            //await _restService.ReportDeviceStatusDebouncedAsync(false, 1000);
            //_restService.ReportDeviceStatus(false);
            base.OnSleep();
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
                //await _messageRepository.LoadMessagesAsync().ConfigureAwait(false);

                if (!_messaging.IsReconnecting)
                    await _messaging.ConnectAsync().ConfigureAwait(false);//OnResume

            }
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine(ex.Message);
            //}
        }
        #endregion
    }
}
