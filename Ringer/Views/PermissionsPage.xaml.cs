using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Plugin.Media.Abstractions;
using Ringer.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Ringer.Views
{
    public partial class PermissionsPage : ContentPage
    {
        private readonly ILocationService location;

        public PermissionsPage()
        {
            On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.FormSheet);
            InitializeComponent();
            location = DependencyService.Resolve<ILocationService>();

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        async void LocationRequested(object sender, EventArgs e)
        {
            if (await location.CheckAndRequestPermissionAsync<Permissions.LocationWhenInUse>() == PermissionStatus.Granted)
                await Navigation.PopModalAsync(true);
        }

        async void AppSettingRequested(object sender, EventArgs e)
        {
            AppInfo.ShowSettingsUI();
            await Navigation.PopModalAsync(true);
        }

        async void ImageButton_Clicked(System.Object sender, System.EventArgs e)
        {
            await Navigation.PopModalAsync(true);
        }
    }
}
