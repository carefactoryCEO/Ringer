using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using Xamarin.Forms.Maps;
using Ringer.Models;

namespace Ringer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        public MapPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            GetGeolocation();
        }

        private async void GetGeolocation()
        {
            try
            {
                var location = await Geolocation.GetLastKnownLocationAsync();
                

                if (location != null)
                {
                    var position = new Position(location.Latitude, location.Longitude);
                    var mapSpan = MapSpan.FromCenterAndRadius(position, Distance.FromMeters(175));
                    MyMap.MoveToRegion(mapSpan);

                    Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");                   
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
            }
            catch (Exception ex)
            {
                // Unable to get location
            }

            
        }

        private void MyMap_MapClicked(object sender, MapClickedEventArgs e)
        {
            Console.WriteLine($"{e.Position.Latitude}, {e.Position.Longitude}");
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            GetGeolocation();
        }

        private async void Button_Clicked_1Async(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("chatpage");
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

                    Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Feature not supported on device
            }
            catch (Exception ex)
            {
                // Handle exception that may have occurred in geocoding
            }
        }
    }
}