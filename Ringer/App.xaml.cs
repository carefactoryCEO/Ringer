using Xamarin.Forms;
using Ringer.Core;
using System.Diagnostics;
using System;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Push;
using Device = Xamarin.Forms.Device;
using Xamarin.Essentials;
using Ringer.Helpers;

namespace Ringer
{
    public partial class App : Application
    {
        public static string Token;

        #region private members
        string chatUrl;
        MessagingService messagingService;

        #endregion

        #region Constructor
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();

            #region prepare signalR
            DependencyService.Register<MessagingService>();

            messagingService = DependencyService.Resolve<MessagingService>();

            messagingService.Reconnected += async (s, e) => await messagingService.JoinRoomAsync("Xamarin", Settings.Name);

            //messagingService.Init(urlRoot: chatUrl, useHttps: https);

            //// Connection events
            //messagingService.Closed += (s, e) => messagingService.AddLocalMessage(e.Message, e.User);
            //messagingService.Reconnecting += (s, e) => messagingService.AddLocalMessage(e.Message, e.User);
            //messagingService.Reconnected += (s, e) => messagingService.AddLocalMessage(e.Message, e.User);

            //// Message events
            //messagingService.OnEntered += (s, e) => messagingService.AddLocalMessage(e.Message, e.User);
            //messagingService.OnLeft += (s, e) => messagingService.AddLocalMessage(e.Message, e.User);
            //messagingService.OnReceivedMessage += (s, e) => messagingService.AddLocalMessage(e.Message, e.User);
            #endregion
        }

        private void MessagingService_Reconnected(object sender, Core.EventArgs.SignalREventArgs e)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Life Cycle Methods
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Potential Code Quality Issues", "RECS0165:Asynchronous methods should return a Task instead of void", Justification = "<Pending>")]
        protected override async void OnStart()
        {
            base.OnStart();

            #region AppCenter
            AppCenter.Start(
            "android=776e5a61-2f89-48c3-95b6-5fa3dde1c708;" +
            "ios=b1b4c859-3d1a-4f7c-bf34-b4e45a2aad65",
            typeof(Analytics), typeof(Crashes), typeof(Push));

            if (await Push.IsEnabledAsync())
            {
                Guid? id = await AppCenter.GetInstallIdAsync();
                Debug.WriteLine("-------------------------");
                Debug.WriteLine(id);
                Debug.WriteLine("-------------------------");

                // Set Device Id
                Settings.DeviceId = id?.ToString();
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Potential Code Quality Issues", "RECS0165:Asynchronous methods should return a Task instead of void", Justification = "<Pending>")]
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
