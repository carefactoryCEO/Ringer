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
            get => Preferences.Get(nameof(UserName), "링거식");
            set => Preferences.Set(nameof(UserName), value);
        }

        public static string RoomName
        {
            get => Preferences.Get(nameof(UserName), "Xamarin");
            set => Preferences.Set(nameof(UserName), value);
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


            messagingService.Connecting += (s, e) => messageRepository.AddLocalMessage(new Message(e.Message));
            messagingService.ConnectionFailed += (s, e) => messageRepository.AddLocalMessage(new Message(e.Message));
            messagingService.Connected += (s, e) => messageRepository.AddLocalMessage(new Message(e.Message));

            messagingService.Disconnecting += (s, e) => messageRepository.AddLocalMessage(new Message(e.Message));
            messagingService.DisconnectionFailed += (s, e) => messageRepository.AddLocalMessage(new Message(e.Message));
            messagingService.Disconnected += (s, e) => messageRepository.AddLocalMessage(new Message(e.Message));

            messagingService.Closed += (s, e) => messageRepository.AddLocalMessage(new Message(e.Message));
            messagingService.Reconnecting += (s, e) => messageRepository.AddLocalMessage(new Message(e.Message));
            messagingService.Reconnected += async (s, e) =>
            {
                messageRepository.AddLocalMessage(new Message(e.Message));
                await messagingService.JoinRoomAsync(RoomName, UserName);
            };

            messagingService.MessageReceived += (s, e) => messageRepository.AddLocalMessage(new Message(e.Message, e.User));
            messagingService.SomeoneEntered += (s, e) => messageRepository.AddLocalMessage(new Message(e.Message, e.User));
            messagingService.SomeoneLeft += (s, e) => messageRepository.AddLocalMessage(new Message(e.Message, e.User));

            #endregion
        }
        #endregion

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
