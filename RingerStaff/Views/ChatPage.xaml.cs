using System;
using RingerStaff.Models;
using RingerStaff.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace RingerStaff.Views
{
    public partial class ChatPage : ContentPage
    {
        private ChatPageViewModel vm;
        private Thickness _insets;

        public ChatPage()
        {
            InitializeComponent();

            BindingContext = vm = new ChatPageViewModel();

            MessagingCenter.Subscribe<ChatPageViewModel, MessageModel>(this, "MessageAdded", (sender, message) =>
            {
                MessageFeed.ScrollTo(message, ScrollToPosition.End, false);
            });
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            if (Device.RuntimePlatform == Device.iOS)
                _insets = On<iOS>().SafeAreaInsets();

            vm.NavBarHeight = _insets.Top + 44;
            vm.BottomPadding = new Thickness(0, 0, 0, _insets.Bottom);

            base.OnSizeAllocated(width, height);
        }

        private void OnListShouldBeScrolled(object sender, EventArgs e)
        {
            if (Device.RuntimePlatform == Device.iOS)
                MessageFeed.ScrollToLast();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();


            // App.ChatPageIsOn = true;
            // App.CurrentRoomId = roomId;

            // subscribe MessageRepository's new message event

            await vm.LoadMessagesAsync();
        }

        protected override void OnDisappearing()
        {
            // App.ChatPageIsOn = false;
            // App.CurrentRoomId = null;

            // subscribe MessageRepository's new message event

            base.OnDisappearing();
        }
    }
}
