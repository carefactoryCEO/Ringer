using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Plugin.LocalNotification;
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
        // Shell.Current.GoToAsync("//mappage/chatpage?from={from}");
        public string From
        {
            set
            {
                vm.IsBusy = value == Constants.PushNotificationString;
                if (value == Constants.PushNotificationString || value == Constants.LocalNotificationString)
                {
                    MessageFeed.ScrollToLast();
                    TitleLabel.Focus();
                }
            }
        }
        #endregion

        #region override methods
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // 안드에서 메시지 로딩이 안 된 상태에서 빈 페이지가 뜨는 문제 해결
            if (!vm.Messages.Any())
            {
                var messaging = DependencyService.Resolve<IMessaging>();

                Device.BeginInvokeOnMainThread(async () =>
                {
                    vm.IsBusy = true;
                    await Task.Delay(200);
                    vm.Messages = new ObservableCollection<MessageModel>(messaging.Messages.Take(Constants.MessageCount));
                    MessageFeed.ScrollToLast();
                    vm.IsBusy = false;
                });
            }

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
            return;
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

        private void ShowLocalNotification()
        {
            var notification = new NotificationRequest
            {
                //BadgeNumber = 1,
                NotificationId = ++App.LocalNotificationId,
                Title = $"Test ##{App.LocalNotificationId}",
                Description = $"Test Description ##{App.LocalNotificationId}",
                ReturningData = "Dummy data ##{App.LocalNotificationId}", // Returning data when tapped on notification.
                //NotifyTime = DateTime.Now.AddSeconds(5), // Used for Scheduling local notification, if not specified notification will show immediately.
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