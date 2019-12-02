using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Ringer.Core.Data;
using Ringer.Helpers;
using Ringer.Services;
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
            await ChatPageVM.CheckLogInAsync();

            // report device status "on"
            _ = await _restService.ReportDeviceStatusAsync(App.DeviceId, true);

            base.OnAppearing();
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();

            // report device status "off"
            _ = await _restService.ReportDeviceStatusAsync(App.DeviceId, false);

        }
    }
}