using System;
using System.Threading.Tasks;
using Ringer.Helpers;
using Ringer.Models;
using Ringer.ViewModels;
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
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await vm.EnsureMessageLoaded()
                            .ContinueWith(t => MessageFeed.ScrollToLast());
                        TitleLabel.Focus();
                    });
                }
            }
        }
        #endregion

        #region override methods
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            App.IsChatPage = true;

            MessagingCenter.Subscribe<ChatPageViewModel, bool>(this, "KeyboardShow", (sender, KeyboardShow) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (!KeyboardShow)
                        Task.Run(() => TitleLabel.Focus()).ContinueWith(t => MessageFeed.ScrollToLast());

                });
            });

            MessagingCenter.Subscribe<ChatPageViewModel, object>(this, "MessageAdded", (sender, message) =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Task.Delay(100);

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

            MessageFeed.ScrollToLast();

            await vm.LogInProcessAsync();
        }
        protected override void OnDisappearing()
        {
            MessagingCenter.Unsubscribe<ChatPageViewModel, object>(this, "MessageAdded");
            MessagingCenter.Unsubscribe<ChatPageViewModel, object>(this, "MessageLoaded");

            vm.InitializeMessages();

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
            if (Device.RuntimePlatform == Device.iOS)
                MessageFeed.ScrollToLast();
        }
        private void Reset_Clicked(object sender, EventArgs e)
        {

        }
        private void Button_Clicked(object sender, EventArgs e)
        {
            //chatInputBarView.IsVisible = false;
            //datePicker.Focus();

            //await Navigation.PushModalAsync(new MediaPage());
        }
        private void DatePicker_DateSelected(object sender, DateChangedEventArgs e)
        {
            chatInputBarView.IsVisible = true;
        }
        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

        void ChatInputBarView_KeyboardShuldBeShown(System.Object sender, bool e)
        {
            DisplayAlert($"{e}", "keyboard should be shown invoked", "close");
        }
        #endregion
    }
}