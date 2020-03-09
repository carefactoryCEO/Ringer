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

            MessagingCenter.Subscribe<ChatPageViewModel, MessageModel>(this, "MessageAdded", (sender, message) =>
            {
                MessageFeed.ScrollTo(message, ScrollToPosition.End, true);
            });

            MessagingCenter.Subscribe<ChatPageViewModel, string>(this, "ConnectionEvent", (sender, message) =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("reconnecting", message, "닫기");
                });
            });
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

            await vm.ExcuteLogInProcessAsync();

            await vm.OnAppearingAsync().ConfigureAwait(false);
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();

            await vm.OnDisappearingAsync().ConfigureAwait(false);
        }
    }
    /**
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [QueryProperty("Room", "room")]
    public partial class ChatPage : ContentPage
    {
        #region Private Members
        bool initial = true;

        private string _room;
        private IRESTService _restService;

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

        #region Constructor
        public ChatPage()
        {
            InitializeComponent();
            _restService = DependencyService.Resolve<IRESTService>();
        }
        #endregion

        #region Life Cycle Methods
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (!initial)
                return;

            initial = false;

            var topInset = (Device.RuntimePlatform == Device.iOS) ? On<iOS>().SafeAreaInsets().Top : 0;

            NavBarRow.Height = topInset + 40;
        }
        #endregion

        protected override async void OnAppearing()
        {
            await ChatPageVM.ExcuteLogInProcessAsync();

            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }
    }
    **/
}