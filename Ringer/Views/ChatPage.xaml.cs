using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Plugin.LocalNotification;
using Ringer.Helpers;
using Ringer.Models;
using Ringer.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Xamarin.Forms.Xaml;

namespace Ringer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [QueryProperty("Room", "room")]
    public partial class ChatPage : ContentPage
    {
        #region private fields
        private readonly ChatPageViewModel vm;
        private Thickness _insets;
        private string _room;
        #endregion

        #region constructor
        public ChatPage()
        {
            InitializeComponent();

            BindingContext = vm = new ChatPageViewModel();
        }
        #endregion

        #region public properties
        public string Room
        {
            get => _room;
            set
            {
                _room = value;
                Debug.WriteLine(_room);
            }
        }
        #endregion

        #region override methods
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            MessagingCenter.Subscribe<ChatPageViewModel, MessageModel>(this, "MessageAdded", (sender, message) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    MessageFeed.ScrollTo(message, ScrollToPosition.End, animated: false);
                });
            });
            MessagingCenter.Subscribe<ChatPageViewModel, object>(this, "MessageLoaded", (sender, message) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    MessageFeed.ScrollTo(message, position: ScrollToPosition.Start, animated: false);
                    MessageFeed.IsLoading = false;

                    Utilities.Trace(((MessageModel)message).Body);

                });
            });

            await vm.ExcuteLogInProcessAsync();
            MessageFeed.ScrollToLast();
        }
        protected override void OnDisappearing()
        {
            MessagingCenter.Unsubscribe<ChatPageViewModel, MessageModel>(this, "MessageAdded");

            MessagingCenter.Unsubscribe<ChatPageViewModel, object>(this, "MessageLoaded");

            vm.ResetMessages();
            base.OnDisappearing();
        }
        protected override void OnSizeAllocated(double width, double height)
        {
            if (Device.RuntimePlatform == Device.iOS)
                _insets = On<iOS>().SafeAreaInsets();

            if (vm != null)
            {
                vm.NavBarHeight = _insets.Top + 44;
                vm.BottomPadding = new Thickness(0, 0, 0, _insets.Bottom);
            }

            base.OnSizeAllocated(width, height);
        }
        #endregion

        #region private methods (include event handlers)
        private void OnListShouldBeScrolled(object sender, EventArgs e)
        {
            if (Device.RuntimePlatform == Device.iOS)
                MessageFeed.ScrollToLast();
        }
        private void Button_Clicked(object sender, EventArgs e)
        {
            // date picker
            //chatInputBarView.IsVisible = false;
            //datePicker.Focus();

            // local notification
            ShowLocalNotification();
        }

        private int notificationId = -1;

        private void ShowLocalNotification()
        {
            var notification = new NotificationRequest
            {
                BadgeNumber = 1,
                NotificationId = ++notificationId,
                Title = "Test",
                Description = "Test Description",
                ReturningData = "Dummy data", // Returning data when tapped on notification.
                NotifyTime = DateTime.Now.AddSeconds(5), // Used for Scheduling local notification, if not specified notification will show immediately.
                //Sound = Device.RuntimePlatform == Device.Android ? "filling_your_inbox" : "filling_your_inbox.m4r",
                //Sound = Device.RuntimePlatform == Device.Android ? "good_things_happen" : "good_things_happen.mp3",
            };

            NotificationCenter.Current.Show(notification);

            // TODO: iOS이고 CurrentRoom이 아니면 알람 사운드
            //if (Device.RuntimePlatform == Device.iOS)
            //{
            //    var player = CrossSimpleAudioPlayer.Current;
            //    player.Load("filling_your_inbox.m4r");
            //    player.Play();
            //}

            Vibration.Vibrate();
        }

        private void DatePicker_DateSelected(object sender, DateChangedEventArgs e)
        {
            chatInputBarView.IsVisible = true;
        }
        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
        #endregion
    }
}