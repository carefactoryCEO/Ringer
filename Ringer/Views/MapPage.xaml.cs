using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Maps;
using Ringer.ViewModels;
using Ringer.Models;
using Ringer.Services;
using Xamarin.Essentials;
using System.Threading.Tasks;

namespace Ringer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        private readonly MapPageViewModel vm;

        public MapPage()
        {
            InitializeComponent();
            BindingContext = vm = new MapPageViewModel();
            MessagingCenter.Subscribe<App>(this, "Resumed", app => OnAppearing());
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            if (await vm.RefreshConsulatesAsync())
            {
                MoveMap(vm.CurrentLatitude, vm.CurrentLongitude);
            }
            else if (Device.RuntimePlatform is Device.Android)
            {
                if (Navigation.ModalStack.LastOrDefault() is PermissionsPage)
                    return;

                await Task.Delay(500);
                await Navigation.PushModalAsync(new PermissionsPage(), true);
            }
        }

        private void MoveMap(double lat, double lon, string labelString = null, string address = null)
        {
            Position position = new Position(lat, lon);
            var mapSpan = MapSpan.FromCenterAndRadius(position, Distance.FromMeters(175));

            RingerMap.MoveToRegion(mapSpan);
            RingerMap.IsShowingUser = true;

            if (labelString != null)
            {
                RingerMap.Pins.Add(new Pin
                {
                    Label = labelString,
                    Address = address,
                    Type = PinType.Place,
                    Position = position
                });
            }
        }
        private void MapCurrentButton_Clicked(object sender, EventArgs e)
        {
            MoveMap(vm.CurrentLatitude, vm.CurrentLongitude);
        }
        private void ConsulateCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var consulate = e.CurrentSelection.FirstOrDefault() as ConsulateModel;

            if (consulate.IsHeader)
            {
                MoveMap(consulate.Latitude, consulate.Longitude);
                return;
            }

            if (consulate.IsFooter)
                return;

            MoveMap(consulate.Latitude, consulate.Longitude, consulate.KoreanName, consulate.Address);
        }
        private async void PermissionButton_Clicked(object sender, EventArgs e)
        {
            if (Device.RuntimePlatform is Device.iOS)
                AppInfo.ShowSettingsUI();

            if (Device.RuntimePlatform is Device.Android)
                await Navigation.PushModalAsync(new PermissionsPage());
        }
    }
}