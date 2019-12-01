using Xamarin.Forms;
using Ringer.Core;
using System.Diagnostics;
using System;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Push;
using Ringer.Helpers;
using Xamarin.Essentials;
using Ringer.Models;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Ringer.Services;

namespace Ringer
{
    /*
     *
     * TODO: [production][ios] info.plist 에서 NSAppTransportSecurity 제거
     * https://docs.microsoft.com/en-us/xamarin/ios/app-fundamentals/ats#opting-out-of-ats
     * 
     */
    public partial class App : Application
    {
        #region public static properties

        // TODO 로그인 상태라면 토큰, 디바이스아이디, CurrentRoomId 등이 모두 null이 아니어야 한다.
        public static bool IsLoggedIn => Token != null && DeviceId != null && UserName != null && CurrentRoomId != null;

        public static List<string> RoomIds = new List<string>();

        // Appcenter에서 받음
        public static string DeviceId
        {
            get => Preferences.Get(nameof(DeviceId), null);
            set => Preferences.Set(nameof(DeviceId), value);
        }

        // 로그인 후에 받음
        public static string Token
        {
            get => Preferences.Get(nameof(Token), null);
            set => Preferences.Set(nameof(Token), value);
        }

        // 로그인 과정 중에 받음
        public static string UserName
        {
            get => Preferences.Get(nameof(UserName), null);
            set => Preferences.Set(nameof(UserName), value);
        }

        public static string CurrentRoomId
        {
            get => Preferences.Get(nameof(CurrentRoomId), null);
            set => Preferences.Set(nameof(CurrentRoomId), value);
        }
        #endregion

        IMessageRepository _messageRepository;
        private IRESTService _restService;

        #region Constructor
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();

            #region Register messagingService
            DependencyService.Register<MessagingService>();
            DependencyService.Register<IMessageRepository, MessageRepository>();
            DependencyService.Register<IRESTService, RESTService>();

            _restService = DependencyService.Resolve<IRESTService>();
            var messagingService = DependencyService.Resolve<MessagingService>();
            _messageRepository = DependencyService.Resolve<IMessageRepository>();

            messagingService.Connecting += (s, e) => Trace(e.Message);
            messagingService.Connected += (s, e) => Trace(e.Message);
            messagingService.ConnectionFailed += (s, e) => Trace(e.Message, true);

            messagingService.Disconnecting += (s, e) => Trace(e.Message);
            messagingService.Disconnected += (s, e) => Trace(e.Message);
            messagingService.DisconnectionFailed += (s, e) => Trace(e.Message, true);

            messagingService.Closed += (s, e) => Trace(e.Message);
            messagingService.Reconnecting += (s, e) => Trace(e.Message);
            messagingService.Reconnected += (s, e) => Trace(e.Message, true);

            messagingService.MessageReceived += MessageReceived;
            messagingService.SomeoneEntered += SomeoneEntered;
            messagingService.SomeoneLeft += SomeoneLeft;

            #endregion
        }
        #endregion

        #region messaging handlers
        public static void Trace(string message = "", bool analyticsAlso = false, [CallerMemberName] string callerName = "")
        {
            message = $"\n[{DateTime.Now.ToString("yy-MM-dd HH:mm:ss")}]{callerName}: {message}";

            Debug.WriteLine(message);

            if (analyticsAlso)
                Analytics.TrackEvent(message);
        }
        private void SomeoneLeft(object sender, Core.EventArgs.SignalREventArgs e)
        {
            if (e.Sender != UserName)
                _messageRepository.AddLocalMessage(new Message { Content = e.Message, Sender = "system" });

            Trace(e.Message);
        }

        private void SomeoneEntered(object sender, Core.EventArgs.SignalREventArgs e)
        {
            if (e.Sender != UserName)
                _messageRepository.AddLocalMessage(new Message { Content = e.Message, Sender = "system" });

            Trace(e.Message);
        }

        private void MessageReceived(object sender, Core.EventArgs.SignalREventArgs e)
        {
            var name = e.Sender == UserName ? string.Empty : $"{e.Sender}: ";
            _messageRepository.AddLocalMessage(new Message { Content = $"{name}{e.Message}", Sender = e.Sender });

            Trace(e.Message);
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
                    string roomId = null;
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
                                    CurrentRoomId = e.CustomData[key];
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

                    _messageRepository.AddLocalMessage(new Message { Content = body, Sender = pushSender });

                    if (roomId != null && Shell.Current.CurrentState.Location.ToString().Contains("chatpage"))
                    {
                        await Shell.Current.Navigation.PopToRootAsync(false);
                        await Shell.Current.GoToAsync($"chatpage?room={roomId}");
                    }
                };
            }

            AppCenter.Start(Constants.AppCenterAndroid + Constants.AppCenteriOS, typeof(Analytics), typeof(Crashes), typeof(Push));

            Analytics.TrackEvent("Ringer started");

            if (await Push.IsEnabledAsync())
            {
                Guid? id = await AppCenter.GetInstallIdAsync();
                Debug.WriteLine("-------------------------");
                Debug.WriteLine(id);
                Debug.WriteLine("-------------------------");

                // Set Device Id
                DeviceId = id?.ToString();
            }
            #endregion

            Debug.WriteLine("App.OnStart");

            _ = await _restService.ReportDeviceStatusAsync(DeviceId, false);
        }

        protected override void OnSleep()
        {
            _restService.ReportDeviceStatusAsync(DeviceId, false);

            Debug.WriteLine("OnSleep");

            // 디바이스의 IsConnected를 false로 만든다.
            base.OnSleep();

        }

        protected override async void OnResume()
        {
            base.OnResume();

            Debug.WriteLine("OnResume");
            Debug.WriteLine(Shell.Current.CurrentState.Location);

            if (Shell.Current.CurrentState.Location.ToString().Contains("chatpage"))
                await _restService.ReportDeviceStatusAsync(DeviceId, true);

            var pendingMessages = await _restService.PullPendingMessages(App.CurrentRoomId, 0);


            // TODO: 1. connection을 확인하고
            //       2. pending message를 다운받는다.

            Debug.WriteLine(pendingMessages.Count);

            foreach (var pendingMessage in pendingMessages)
            {
                Debug.WriteLine(pendingMessage.Body);
            }

        }
        #endregion
    }
}
