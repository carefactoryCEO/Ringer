using System;
using System.Collections.Generic;
using Plugin.LocalNotification;
using Plugin.SimpleAudioPlayer;
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
                Sound = Device.RuntimePlatform == Device.Android ? "filling_your_inbox" : "filling_your_inbox.m4r",
            };

            NotificationCenter.Current.Show(notification);

            if (Device.RuntimePlatform == Device.iOS)
            {
                var player = CrossSimpleAudioPlayer.Current;
                player.Load("filling_your_inbox.m4r");
                player.Play();
            }
        }
    }
}
