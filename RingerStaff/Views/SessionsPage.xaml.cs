using System;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace RingerStaff.Views
{
    public partial class SessionsPage : ContentPage
    {
        public SessionsPage()
        {
            InitializeComponent();
            On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.FormSheet);
        }

        private async void CloseButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}
