using Xamarin.Forms;
using Ringer.Core;
using System.Diagnostics;
using System;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Push;
using Ringer.Helpers;

namespace Ringer
{
    /*
     *
     * TODO: [production][ios] info.plist 에서 NSAppTransportSecurity 제거 https://docs.microsoft.com/en-us/xamarin/ios/app-fundamentals/ats#opting-out-of-ats
     * 
     */
    public partial class App : Application
    {
        public static string Token;

        #region Constructor
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();

            #region Register messagingService
            DependencyService.Register<MessagingService>();
            var messagingService = DependencyService.Resolve<MessagingService>();
            messagingService.Reconnected += async (s, e) => await messagingService.JoinRoomAsync("Xamarin", Constants.UserName);
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
                Constants.DeviceId = id?.ToString();
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
