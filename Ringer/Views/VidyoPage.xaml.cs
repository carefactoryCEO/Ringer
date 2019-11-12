using System;
using System.Collections.Generic;
using Xamarin.Essentials;

using Xamarin.Forms;

namespace Ringer.Views
{
    public partial class VidyoPage : ContentPage
    {
        public VidyoPage()
        {
            InitializeComponent();
        }

        async void Button_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PopAsync();
        }

        private async void Button_Clicked_1(object sender, EventArgs e)
        {
            await Browser.OpenAsync("https://appr.tc");
        }
    }
}
