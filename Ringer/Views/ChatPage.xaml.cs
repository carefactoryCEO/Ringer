using System;
using System.Diagnostics;
using Ringer.Models;
using Ringer.Services;
using Ringer.ViewModels;
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
        private readonly ChatPageViewModel vm;
        private Thickness _insets;
        private string _room;

        public ChatPage()
        {
            InitializeComponent();

            BindingContext = vm = new ChatPageViewModel();
        }

        public string Room
        {
            get => _room;
            set
            {
                _room = value;
                Debug.WriteLine(_room);
            }
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

            MessagingCenter.Subscribe<MessageRepository, MessageModel>(this, "MessageAdded", (sender, message) =>
            {
                // 0.2초간 기다린다
                Device.StartTimer(TimeSpan.FromMilliseconds(200), () =>
                {
                    MessageFeed.ScrollToLast();
                    return false;
                });
            });
            MessagingCenter.Subscribe<ChatPageViewModel, string>(this, "ConnectionEvent", (sender, message) =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("reconnecting", message, "닫기");
                });
            });

            await vm.ExcuteLogInProcessAsync();

            await vm.OnAppearingAsync().ConfigureAwait(false);

            MessageFeed.ScrollToLast();
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();

            MessagingCenter.Unsubscribe<MessageRepository, MessageModel>(this, "MessageAdded");
            MessagingCenter.Unsubscribe<ChatPageViewModel, string>(this, "ConnectionEvent");

            await vm.OnDisappearingAsync().ConfigureAwait(false);
        }

        void Button_Clicked(object sender, EventArgs e)
        {
            chatInputBarView.IsVisible = false;
            datePicker.Focus();
        }

        void datePicker_DateSelected(object sender, DateChangedEventArgs e)
        {
            chatInputBarView.IsVisible = true;
        }

        void datePicker_Unfocused(object sender, FocusEventArgs e)
        {

        }

        async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}