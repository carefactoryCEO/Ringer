using System;
using Ringer.Helpers;
using Ringer.Models;
using Ringer.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Xamarin.Forms.Xaml;

namespace Ringer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [QueryProperty("From", "from")]
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

            MessageFeed.ItemAppearing += MessageFeed_ItemAppearing;
        }

        private void MessageFeed_ItemAppearing(object sender, ItemVisibilityEventArgs e) // e.Item, e.ItemIndex
        {

        }
        #endregion

        #region Query properties
        public string From
        {
            set
            {
                if (value == Constants.PushNotificationString)
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        vm.IsBusy = true;
                        await vm.EnsureMessageLoaded()
                            .ContinueWith(t => MessageFeed.ScrollToLast());
                        TitleLabel.Focus();
                        vm.IsBusy = false;
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

            MessagingCenter.Subscribe<ChatPageViewModel, object>(this, "MessageAdded", (sender, message) =>
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

                    Utilities.Trace(((MessageModel)message).Body);
                });
            });

            await vm.ExcuteLogInProcessAsync();
            MessageFeed.ScrollToLast();
        }
        protected override void OnDisappearing()
        {
            MessagingCenter.Unsubscribe<ChatPageViewModel, object>(this, "MessageAdded");

            MessagingCenter.Unsubscribe<ChatPageViewModel, object>(this, "MessageLoaded");

            vm.ResetMessages();

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
        private void Button_Clicked(object sender, EventArgs e)
        {
            // date picker
            //chatInputBarView.IsVisible = false;
            //datePicker.Focus();
            //return;
            // local notification
            //Device.StartTimer(TimeSpan.FromSeconds(3), () =>
            //{
            //    if (App.IsOn && App.IsChatPage)
            //    {
            //        Xamarin.Forms.Application.Current.MainPage.DisplayAlert("chat", "chatting", "닫기");
            //    }
            //    else
            //    {
            //        ShowLocalNotification();
            //    }

            //    return true;
            //});
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