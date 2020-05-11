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
using Microsoft.AppCenter.Distribute;
using Plugin.SimpleAudioPlayer;
using RingerStaff.Views;
using System.Collections.Generic;

namespace RingerStaff
{
    public partial class App : Application
    {
        public static readonly string BaseUrl = DeviceInfo.DeviceType ==
            DeviceType.Physical ? "https://ringerhub.azurewebsites.net" :
            DeviceInfo.Platform == DevicePlatform.iOS ? "http://localhost:5000" :
            DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5000" : null;
        public static readonly string Huburl = BaseUrl + "/hubs/chat";
        public static readonly string PendingUrl = BaseUrl + "/message/pending";
        public static readonly string LoginUrl = BaseUrl + "/auth/staff-login";

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
        private static int id;

        public static bool IsLoggedIn => !string.IsNullOrEmpty(Token);
        public static Dictionary<string, int> UnreadCounts = new Dictionary<string, int>();

        public static bool IsForeground = false;

        public App()
        {
            InitializeComponent();

            NotificationCenter.Current.NotificationTapped += OnLocalNotificationTapped;

            DependencyService.Register<MockDataStore>();

            MainPage = new AppShell();

            id = 0;

            RealTimeService.MessageReceived += RealTimeService_MessageReceived;
        }

        private void RealTimeService_MessageReceived(object sender, Ringer.Core.EventArgs.MessageReceivedEventArgs e)
        {
            if (e.SenderId == App.UserId)
                return;

            if (e.RoomId != RoomId)
            {
                var currentState = Shell.Current.CurrentState;
                var vibeOnly = currentState.Location.ToString().EndsWith(nameof(RoomsPage)) && App.IsForeground;

                NotifyLocally(e.SenderName, e.Body, e.RoomId, vibeOnly);
            }
            else if (!App.IsForeground && Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android)
            {
                NotifyLocally(e.SenderName, e.Body, e.RoomId);
            }
        }

        private static void NotifyLocally(string sender, string body, string roomId, bool vibeOnly = false)
        {
            if (!vibeOnly)
            {
                var notification = new NotificationRequest
                {
                    NotificationId = id++,
                    Title = sender,
                    Description = body,
                    ReturningData = roomId,
                    Sound = Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android ? "good_things_happen" : "good_things_happen.mp3",
                };

                NotificationCenter.Current.Show(notification);
            }

            // TODO: iOS이고 CurrentRoom이 아니면 알람 사운드
            if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.iOS)
            {
                var player = CrossSimpleAudioPlayer.Current;
                player.Load("good_things_happen.mp3");
                player.Play();
            }

            Vibration.Vibrate();
        }

        private void OnLocalNotificationTapped(NotificationTappedEventArgs e)
        {
            Debug.WriteLine($"noti data: {e.Data}");
            var uri = $"//{nameof(RoomsPage)}/{nameof(ChatPage)}?RoomId={e.Data}";
            Shell.Current.GoToAsync(uri);
        }

        protected override async void OnStart()
        {
            App.IsForeground = true;

            #region AppCenter
            // Intercept Push Notification
            if (!AppCenter.Configured)
            {
                Push.PushNotificationReceived += async (sender, e) =>
                {
                    string roomIdKey = "room";

                    if (e.CustomData != null)
                    {
                        if (e.CustomData.ContainsKey(roomIdKey))
                        {
                            await Shell.Current.GoToAsync($"//{nameof(RoomsPage)}/{nameof(ChatPage)}?RoomId={e.CustomData[roomIdKey]}");
                        }
                    }
                };
            }

            AppCenter.Start(
                "ios=9573aacd-70c3-459f-aa6c-b841953e7f1d;" +
                "android=2468e092-6b08-4ce9-a777-cc06f2d20408;",
                typeof(Analytics),
                typeof(Crashes),
                typeof(Push),
                typeof(Distribute));

            Analytics.TrackEvent("RingerStaff started");

            if (await Push.IsEnabledAsync())
            {
                Guid? id = await AppCenter.GetInstallIdAsync().ConfigureAwait(false);
                DeviceId = id?.ToString();

                Debug.WriteLine("-------------------------");
                Debug.WriteLine($"device id: {DeviceId}");
                Debug.WriteLine("-------------------------");
            }
            #endregion

            if (IsLoggedIn)
                await RealTimeService.ConnectAsync(Huburl, Token).ConfigureAwait(false);
        }

        protected override async void OnSleep()
        {
            App.IsForeground = false;

            if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.iOS)
                await RealTimeService.DisconnectAsync();
        }

        protected override async void OnResume()
        {
            App.IsForeground = true;

            if (IsLoggedIn)
                await RealTimeService.ConnectAsync(Huburl, Token).ConfigureAwait(false);
        }
    }
}
