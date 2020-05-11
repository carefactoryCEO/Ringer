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
        private readonly ChatPageViewModel vm;
        private Thickness _insets;

        public ChatPage()
        {
            InitializeComponent();

            BindingContext = vm = new ChatPageViewModel();

            MessagingCenter.Subscribe<ChatPageViewModel, MessageModel>(this, "MessageAdded", (sender, message) =>
            {
                MessageFeed.ScrollTo(message, ScrollToPosition.MakeVisible, true);
            });

            MessagingCenter.Subscribe<ChatPageViewModel, string>(this, "ConnectionEvent", (sender, message) =>
            {
                //Device.BeginInvokeOnMainThread(async () =>
                //{
                //    await DisplayAlert("reconnecting", message, "닫기");
                //});
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

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (!App.IsCameraActivated)
                App.RoomId = null;
        }
    }
}
