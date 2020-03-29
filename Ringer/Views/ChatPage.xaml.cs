using System;
using System.Threading.Tasks;
using Ringer.Helpers;
using Ringer.Models;
using Ringer.Services;
using Ringer.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Xamarin.Forms.Xaml;

namespace Ringer.Views
{
    [QueryProperty("From", "from")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatPage : ContentPage
    {
        #region private fields
        private readonly ChatPageViewModel vm;
        private Thickness _insets;
        private bool notificationCalled;
        #endregion

        #region constructor
        public ChatPage()
        {
            InitializeComponent();

            BindingContext = vm = new ChatPageViewModel();
        }
        #endregion

        #region Query properties
        public string From
        {
            set
            {
                if (value == Constants.PushNotificationString || value == Constants.LocalNotificationString)
                {
                    notificationCalled = true;
                }
            }
        }
        #endregion

        #region override methods
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            App.IsChatPage = true;

            MessagingCenter.Subscribe<ChatPageViewModel, bool>(this, "ShowOrHideKeyboard", (sender, showing) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    chatInputBarView.FindByName<Grid>("InputGrid").IsVisible = showing;
                    //chatInputBarView.IsVisible = showing;
                    MessageFeed.ScrollToLast();
                });
            });

            MessagingCenter.Subscribe<ChatPageViewModel, object>(this, "MessageAdded", (sender, message) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (Device.RuntimePlatform == Device.Android)
                        InvalidateMeasure();

                    MessageFeed.ScrollTo(message, ScrollToPosition.End, animated: false);
                });
            });

            MessagingCenter.Subscribe<ChatPageViewModel, object>(this, "MessageLoaded", (sender, message) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    MessageFeed.ScrollTo(message, position: ScrollToPosition.Start, animated: false);

                    Utility.Trace(((MessageModel)message).Body);
                });
            });

            if (App.IsLoggedIn)
                vm.OnAppearing();
            else
                await vm.LogInProcessAsync();
        }
        protected override void OnDisappearing()
        {
            MessagingCenter.Unsubscribe<ChatPageViewModel, bool>(this, "HideKeyboard");
            MessagingCenter.Unsubscribe<ChatPageViewModel, object>(this, "MessageAdded");
            MessagingCenter.Unsubscribe<ChatPageViewModel, object>(this, "MessageLoaded");

            App.IsChatPage = false;

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
            MessageFeed.ScrollToLast();
        }
        private void DatePicker_DateSelected(object sender, DateChangedEventArgs e)
        {
            chatInputBarView.IsVisible = true;
        }
        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }
        #endregion
    }
}