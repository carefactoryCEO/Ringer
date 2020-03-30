using Xamarin.Forms;
using RingerStaff.Services;
using Plugin.LocalNotification;
using System.Diagnostics;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Push;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using Xamarin.Essentials;
using System.Collections.ObjectModel;
using RingerStaff.Models;

namespace RingerStaff
{
    public partial class App : Application
    {
        public static string BaseUrl = DeviceInfo.DeviceType == DeviceType.Physical ? "https://ringerhub.azurewebsites.net" : DeviceInfo.Platform == DevicePlatform.iOS ? "http://localhost:5000" : DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5000" : null;
        public static string Huburl = BaseUrl + "/hubs/chat";
        public static readonly string PendingUrl = BaseUrl + "/message/pending";
        public static string LoginUrl = BaseUrl + "/auth/staff-login";
        public static string Token
        {
            get => Preferences.Get(nameof(Token), null);
            set => Preferences.Set(nameof(Token), value);
        }
        public static string DeviceId
        {
            get => Preferences.Get(nameof(DeviceId), null);
            set => Preferences.Set(nameof(DeviceId), value);
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
        public static string RoomId;
        internal static string RoomTitle;

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

            // Intercept Push Notification
            if (!AppCenter.Configured)
            {
                Push.PushNotificationReceived += (sender, e) =>
                {
                    string body = null;
                    string pushSender = null;
                    // If there is custom data associated with the notification,
                    // print the entries
                    if (e.CustomData != null)
                    {
                        foreach (var key in e.CustomData.Keys)
                        {
                            Debug.WriteLine($"[{key}]{e.CustomData[key]}");

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
                            }
                        }
                    }

                    //if (CurrentRoomId != null)
                    //{
                    //    await Shell.Current.Navigation.PopToRootAsync(false);
                    //    await Shell.Current.GoToAsync($"//mappage/chatpage?room={CurrentRoomId}", false);
                    //}
                };
            }


            //"ios=0da55050-30d7-43a3-ba7c-1404af4ccba0;" +
            //"android=b4662d44-77f1-47f8-8256-dd1756a6f015;"
            AppCenter.Start(
                "ios=9573aacd-70c3-459f-aa6c-b841953e7f1d;" +
                "android=2468e092-6b08-4ce9-a777-cc06f2d20408;",
                typeof(Analytics),
                typeof(Crashes),
                typeof(Push));

            Analytics.TrackEvent("RingerStaff started");

            if (await Push.IsEnabledAsync())
            {
                Guid? id = await AppCenter.GetInstallIdAsync().ConfigureAwait(false);
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
                await RealTimeService.ConnectAsync(Huburl, Token).ConfigureAwait(false);
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
