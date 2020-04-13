using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Ringer.Views
{
    public partial class RegisterPage : ContentPage
    {
        private Thickness _inset;

        public RegisterPage()
        {
            InitializeComponent();
            On<iOS>().SetUseSafeArea(true);

        }

        async void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            _inset = On<iOS>().SafeAreaInsets();

            On<iOS>().SetUseSafeArea(false);
            ScrollContainer.Margin = _inset;

            await PermissionView.TranslateTo(0, 0, 250);
        }

        async void Button_Clicked_1(System.Object sender, System.EventArgs e)
        {
            On<iOS>().SetUseSafeArea(true);

            ScrollContainer.Margin = default;
            await PermissionView.TranslateTo(0, 1000, 250);
        }
    }
}
