using System;
using System.Collections.Generic;
using Plugin.LocalNotification;
using Plugin.SimpleAudioPlayer;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace RingerStaff.Views
{
    public partial class SchedulePage : ContentPage
    {
        int id = 0;

        public SchedulePage()
        {
            InitializeComponent();
        }

        void Notification_Clicked(object sender, EventArgs e)
        {
            var notification = new NotificationRequest
            {
                NotificationId = id++,
                Title = "Test",
                Description = "Test Description",
                ReturningData = "Dummy data", // Returning data when tapped on notification.
                //NotifyTime = DateTime.Now.AddSeconds(5), // Used for Scheduling local notification, if not specified notification will show immediately.
                //Sound = Device.RuntimePlatform == Device.Android ? "filling_your_inbox" : "filling_your_inbox.m4r",
                Sound = Device.RuntimePlatform == Device.Android ? "good_things_happen" : "good_things_happen.mp3",
            };

            NotificationCenter.Current.Show(notification);

            // TODO: iOS이고 CurrentRoom이 아니면 알람 사운드
            if (Device.RuntimePlatform == Device.iOS)
            {
                var player = CrossSimpleAudioPlayer.Current;
                player.Load("filling_your_inbox.m4r");
                player.Play();
            }

            Vibration.Vibrate();
        }
    }
}
