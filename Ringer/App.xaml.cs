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

        #region Constructor
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();

            #region Register messagingService
            DependencyService.Register<MessagingService>();
            DependencyService.Register<IMessageRepository, MessageRepository>();

            var messagingService = DependencyService.Resolve<MessagingService>();
            var messageRepository = DependencyService.Resolve<IMessageRepository>();


            messagingService.Connecting += (s, e) =>
            {
                Conncting(e, messageRepository);
            };
            messagingService.ConnectionFailed += (s, e) =>
            {
                ConnectionFailed(e, messageRepository);
            };
            messagingService.Connected += (s, e) =>
            {
                Connected(e, messageRepository);
            };

            messagingService.Disconnecting += (s, e) =>
            {
                Disconnecting(e, messageRepository);
            };
            messagingService.DisconnectionFailed += (s, e) =>
            {
                DisconnectionFailed(e, messageRepository);
            };
            messagingService.Disconnected += (s, e) =>
            {
                Disconnected(e, messageRepository);
            };

            messagingService.Closed += (s, e) =>
            {
                Closed(e, messageRepository);
            };
            messagingService.Reconnecting += (s, e) =>
            {
                reconnecting(e, messageRepository);
            };
            messagingService.Reconnected += async (s, e) =>
            {
                await messagingService.JoinRoomAsync(RoomName, UserName);
                Reconnedted(e, messageRepository);
            };

            messagingService.MessageReceived += (s, e) =>
            {
                MessageReceived(e, messageRepository);
            };
            messagingService.SomeoneEntered += (s, e) =>
            {
                SomeoneEntered(e, messageRepository);
            };
            messagingService.SomeoneLeft += (s, e) =>
            {
                SomeoneLeft(e, messageRepository);
            };

            #endregion
        }

        private static void SomeoneLeft(Core.EventArgs.SignalREventArgs e, IMessageRepository messageRepository)
        {
            if (e.User != UserName)
                messageRepository.AddLocalMessage(new Message(e.Message));
            Trace(e.Message);
        }

        private void SomeoneEntered(Core.EventArgs.SignalREventArgs e, IMessageRepository messageRepository)
        {
            if (e.User != UserName)
                messageRepository.AddLocalMessage(new Message(e.Message));
            Trace(e.Message);
        }

        private void MessageReceived(Core.EventArgs.SignalREventArgs e, IMessageRepository messageRepository)
        {
            var name = e.User == UserName ? string.Empty : $"{e.User}: ";
            messageRepository.AddLocalMessage(new Message($"{name}{e.Message}", e.User));
            Trace(e.Message);
        }

        private void Reconnedted(ConnectionEventArgs e, IMessageRepository messageRepository)
        {
            //messageRepository.AddLocalMessage(new Message(e.Message));
            Trace(e.Message);
        }

        private void reconnecting(ConnectionEventArgs e, IMessageRepository messageRepository)
        {
            //messageRepository.AddLocalMessage(new Message(e.Message));
            Trace(e.Message);
        }

        private void Closed(ConnectionEventArgs e, IMessageRepository messageRepository)
        {
            //messageRepository.AddLocalMessage(new Message(e.Message));
            Trace(e.Message);
        }

        private void Disconnected(ConnectionEventArgs e, IMessageRepository messageRepository)
        {
            //messageRepository.AddLocalMessage(new Message(e.Message));
            Trace(e.Message);
        }

        private void DisconnectionFailed(ConnectionEventArgs e, IMessageRepository messageRepository)
        {
            //messageRepository.AddLocalMessage(new Message(e.Message));
            Trace(e.Message);
        }

        private void Disconnecting(ConnectionEventArgs e, IMessageRepository messageRepository)
        {
            //messageRepository.AddLocalMessage(new Message(e.Message));
            Trace(e.Message);
        }

        private void Connected(ConnectionEventArgs e, IMessageRepository messageRepository)
        {
            //messageRepository.AddLocalMessage(new Message(e.Message));
            Trace(e.Message);
        }

        private void ConnectionFailed(ConnectionEventArgs e, IMessageRepository messageRepository)
        {
            //messageRepository.AddLocalMessage(new Message(e.Message));
            Trace(e.Message);
        }

        private void Conncting(ConnectionEventArgs e, IMessageRepository messageRepository)
        {
            //messageRepository.AddLocalMessage(new Message(e.Message));
            Trace(e.Message);
        }
        #endregion

        public static void Trace(string message = "", [CallerMemberName] string name = "")
        {
            Debug.WriteLine($"{name}: {message}\n");
        }

        #region Life Cycle Methods
        protected override async void OnStart()
        {
            base.OnStart();

            #region AppCenter
            AppCenter.Start(Constants.AppCenterAndroid + Constants.AppCenteriOS, typeof(Analytics), typeof(Crashes), typeof(Push));

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

            if (DesignMode.IsDesignModeEnabled)
                return;
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
