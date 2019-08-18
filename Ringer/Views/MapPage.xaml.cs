using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using Xamarin.Forms.Maps;
using Ringer.ViewModels;
using Ringer.Models;

namespace Ringer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        public MapPage()
        {
            InitializeComponent();

            //GetGeolocation();
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

        private void MyMap_MapClicked(object sender, Xamarin.Forms.Maps.MapClickedEventArgs e)
        {
            Console.WriteLine(e.Position);
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            GetGeolocation();

            //GetGeolocation();
            //37.6109008789063, Longitude: 127.039388325429

            //var location = await Geolocation.GetLastKnownLocationAsync();

            //var position = new Position(37.6109008789063, 127.039388325429);

            //var position = new Position(location.Latitude, location.Longitude);

            //MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromKilometers(0.1)));            //var position = new Position(location.Latitude, location.Longitude);

            //MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromKilometers(0.1)));
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