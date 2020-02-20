using Xamarin.Forms;
using RingerStaff.Services;
using Plugin.LocalNotification;
using System.Diagnostics;
using RingerStaff.Views;
using System.Linq;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Push;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using Xamarin.Essentials;

namespace RingerStaff
{
    public partial class App : Application
    {
        public static string BaseUrl = DeviceInfo.Platform == DevicePlatform.iOS ? "http://localhost:5000" : DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5000" : null;
        public static string Huburl = BaseUrl + "/hubs/chat";
        public static string LoginUrl = BaseUrl + "/auth/staff-login";
        public static string Token
        {
            get => Preferences.Get(nameof(Token), null);
            set => Preferences.Set(nameof(Token), value);
        }
        public static string DeviceId;
        public static string CurrentRoomId;

        public static bool IsLoggedIn => !string.IsNullOrEmpty(Token);


        public App()
        {
            InitializeComponent();

            NotificationCenter.Current.NotificationTapped += OnLocalNotificationTapped;

            DependencyService.Register<MockDataStore>();

            MainPage = new AppShell();

            PageAppearing += App_PageAppearing;
        }

        private void App_PageAppearing(object sender, Page page)
        {
            //if (!IsLoggedIn)
            //    if (page.Navigation.ModalStack.Count == 0 || !(page.Navigation.ModalStack.Last() is LoginPage))
            //        page.Navigation.PushModalAsync(new LoginPage());
        }

        private void OnLocalNotificationTapped(NotificationTappedEventArgs e)
        {
            Debug.WriteLine($"noti data: {e.Data}");
        }

        protected override async void OnStart()
        {
            #region AppCenter

            /** // Intercept Push Notification
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
                                    CurrentRoomId = e.CustomData[key];
                                    break;

                                case "body":
                                    body = e.CustomData[key];
                                    break;

                                case "sender":
                                    pushSender = e.CustomData[key];
                                    break;
                            }
                        }
                    }

                    if (CurrentRoomId != null && !IsChatPage)
                    {
                        await Shell.Current.Navigation.PopToRootAsync(false);
                        await Shell.Current.GoToAsync($"//mappage/chatpage?room={CurrentRoomId}", false);
                    }
                };
            }
            **/

            AppCenter.Start(
                "ios=0da55050-30d7-43a3-ba7c-1404af4ccba0;" +
                "android=b4662d44-77f1-47f8-8256-dd1756a6f015;",
                typeof(Analytics),
                typeof(Crashes),
                typeof(Push));

            Analytics.TrackEvent("RingerStaff started");

            if (await Push.IsEnabledAsync())
            {
                Guid? id = await AppCenter.GetInstallIdAsync();
                // Set Device Id
                DeviceId = id?.ToString();

                Debug.WriteLine("-------------------------");
                Debug.WriteLine($"device id: {DeviceId}");
                Debug.WriteLine("-------------------------");
            }
            #endregion

            #region SignalR Connection
            if (IsLoggedIn)
            {
                await RealTimeService.ConnectAsync(Huburl, Token);
            }
            #endregion
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
