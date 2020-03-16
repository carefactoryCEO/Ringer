using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Ringer.Models;
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
                // 0.2초간 기다린다
                //Device.StartTimer(TimeSpan.FromMilliseconds(200), () =>
                Device.BeginInvokeOnMainThread(() =>
                {
                    MessageFeed.ScrollTo(message, ScrollToPosition.End, animated: false);
                });
            });


            MessagingCenter.Subscribe<ChatPageViewModel, MessageModel>(this, "MessageLoaded", (s, m) =>
            {
                Debug.WriteLine(m.Body);
                Device.BeginInvokeOnMainThread(() =>
                {
                    MessageFeed.ScrollTo(m as object, position: ScrollToPosition.Start, animated: false);
                });

                MessageFeed.IsLoading = false;
            });

            await vm.ExcuteLogInProcessAsync();

            await vm.OnAppearingAsync().ConfigureAwait(false);
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();

            MessagingCenter.Unsubscribe<MessageRepository, MessageModel>(this, "MessageAdded");
            MessagingCenter.Unsubscribe<ChatPageViewModel, string>(this, "ConnectionEvent");

            await vm.OnDisappearingAsync().ConfigureAwait(false);
        }
        protected override void OnSizeAllocated(double width, double height)
        {
            if (Device.RuntimePlatform == Device.iOS)
                _insets = On<iOS>().SafeAreaInsets();

            vm.NavBarHeight = _insets.Top + 44;
            vm.BottomPadding = new Thickness(0, 0, 0, _insets.Bottom);

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
            chatInputBarView.IsVisible = false;
            datePicker.Focus();
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