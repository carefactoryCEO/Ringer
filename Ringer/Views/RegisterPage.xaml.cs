using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Ringer.Views
{
    public partial class RegisterPage : ContentPage
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        async void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            On<iOS>().SetUseSafeArea(false);
            await PermissionView.TranslateTo(0, 0, 250);
        }

        async void Button_Clicked_1(System.Object sender, System.EventArgs e)
        {
            On<iOS>().SetUseSafeArea(true);
            await PermissionView.TranslateTo(0, 1000, 250);
        }
    }
}
