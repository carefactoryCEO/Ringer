using Ringer.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Xamarin.Forms.Xaml;

namespace Ringer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatPage : ContentPage
    {
        #region Private Members
        bool initial = true;

        #endregion

        #region Constructor
        public ChatPage()
        {
            InitializeComponent();
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
            base.OnAppearing();
            await ChatPageVM.CheckLogInAsync();
        }
    }
}