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
using Ringer.Core.Models;
using Ringer.Models;
using System.Runtime.CompilerServices;

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
        public static bool IsLoggedIn => Token != null;

        public static string CurrentRoomId
        {
            get => Preferences.Get(nameof(CurrentRoomId), null);
            set => Preferences.Set(nameof(CurrentRoomId), value);
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
            get => Preferences.Get(nameof(UserName), "링거");
            set => Preferences.Set(nameof(UserName), value);
        }

        public static string RoomName
        {
            get => Preferences.Get(nameof(RoomName), Constants.ChattingRoom);
            set => Preferences.Set(nameof(RoomName), value);
        }
        #endregion

        IMessageRepository _messageRepository;

        #region Constructor
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();

            #region Register messagingService
            DependencyService.Register<MessagingService>();
            DependencyService.Register<IMessageRepository, MessageRepository>();

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
            if (e.User != UserName)
                _messageRepository.AddLocalMessage(new Message(e.Message));

            Trace(e.Message);
        }

        private void SomeoneEntered(object sender, Core.EventArgs.SignalREventArgs e)
        {
            if (e.User != UserName)
                _messageRepository.AddLocalMessage(new Message(e.Message));

            Trace(e.Message);
        }

        private void MessageReceived(object sender, Core.EventArgs.SignalREventArgs e)
        {
            var name = e.User == UserName ? string.Empty : $"{e.User}: ";
            _messageRepository.AddLocalMessage(new Message($"{name}{e.Message}", e.User));

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
                Push.PushNotificationReceived += (sender, e) =>
                {
                    // Add the notification message and title to the message
                    var summary = $"Push notification received:" +
                                        $"\n\tNotification title: {e.Title}" +
                                        $"\n\tMessage: {e.Message}";

                    // If there is custom data associated with the notification,
                    // print the entries
                    if (e.CustomData != null)
                    {
                        summary += "\n\tCustom data:\n";
                        foreach (var key in e.CustomData.Keys)
                        {
                            summary += $"\t\t{key} : {e.CustomData[key]}\n";
                        }
                    }

                    // Send the notification summary to debug output
                    Debug.WriteLine(summary);
                    _messageRepository.AddLocalMessage(new Message(summary));
                    Debug.WriteLine(_messageRepository.Messages.Count);

                    //Shell.Current.GoToAsync("chatpage");
                    //Shell.Current.Navigation.PopAsync();

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
        }

        protected override void OnSleep()
        {
            base.OnSleep();

            Debug.WriteLine("OnSleep");

            // TODO: 이 디바이스의 IsConnected를 false로 만든다.

        }

        protected override void OnResume()
        {
            base.OnResume();

            Debug.WriteLine("OnResume");

            // TODO: 1. connection을 확인하고
            //       2. pending message를 다운받는다.
        }
        #endregion
    }
}
