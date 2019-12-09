using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using Xamarin.Forms.Maps;
using Ringer.Models;
using System.Threading.Tasks;
using Ringer.ViewModels;
using Map = Xamarin.Forms.Maps.Map;
using System.Diagnostics;

namespace Ringer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        #region constructor
        public MapPage()
        {
            InitializeComponent();
        }
        #endregion

        #region override methods
        protected async override void OnAppearing()
        {
            base.OnAppearing();

            await Task.WhenAll
            (
                PushAlert.TranslateTo(0, 0, 200, Easing.SinIn),
                GetGeolocationAsync()
            );

            await Task.Delay(5000);

            await PushAlert.TranslateTo(0, -80, 200, Easing.SinInOut);
        }
        #endregion

        #region private methods
        private async Task GetGeolocationAsync()
        {
            try
            {
                // TODO: Location permission check here
                var request = new GeolocationRequest(GeolocationAccuracy.Medium);
                var location = await Geolocation.GetLastKnownLocationAsync() ?? await Geolocation.GetLocationAsync(request);

                if (location != null)
                {
                    var position = new Position(location.Latitude, location.Longitude);
                    var mapSpan = MapSpan.FromCenterAndRadius(position, Distance.FromMeters(175));

                    MyMap.MoveToRegion(mapSpan);
                    MyMap.IsShowingUser = true;

                    await (BindingContext as MapPageViewModel).InsertCurrentLocationAsync(location);

                    Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                Console.WriteLine(fnsEx.Message);
            }
            catch (FeatureNotEnabledException fneEx)
            {
                Console.WriteLine(fneEx.Message);
            }
            catch (PermissionException pEx)
            {
                Console.WriteLine(pEx.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void MyMap_MapClicked(object sender, MapClickedEventArgs e)
        {
            Console.WriteLine($"{e.Position.Latitude}, {e.Position.Longitude}");
        }

        private async void MapCurrentButton_Clicked(object sender, EventArgs e)
        {
            await GetGeolocationAsync();
        }

        private async void ItemsListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            try
            {
                var address = (e.SelectedItem as Infomation).Location;
                var locations = await Geocoding.GetLocationsAsync(address);

                var location = locations?.FirstOrDefault();
                if (location != null)
                {
                    var position = new Position(location.Latitude, location.Longitude);
                    var mapSpan = MapSpan.FromCenterAndRadius(position, Distance.FromMeters(175));
                    MyMap.MoveToRegion(mapSpan);

                    Debug.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Feature not supported on device
                Debug.WriteLine(fnsEx.Message);
            }
            catch (Exception ex)
            {
                // Handle exception that may have occurred in geocoding
                Debug.WriteLine(ex.Message);
            }
        }
        #endregion
    }
}