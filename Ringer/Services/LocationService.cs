using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Ringer.Core.Models;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Ringer.Services
{
    public interface ILocationService
    {
        double CurrentLatitude { get; }
        double CurrentLongitude { get; }
        string CurrentAddress { get; }
        List<Consulate> Consulates { get; }

        event EventHandler LocationUpdated;

        Task RefreshAsync();
    }

    public class LocationService : ILocationService
    {
        private readonly IRESTService api;

        public event EventHandler LocationUpdated;

        public LocationService()
        {
            api = DependencyService.Get<IRESTService>();
        }

        public double CurrentLatitude { get; private set; }
        public double CurrentLongitude { get; private set; }
        public string CurrentAddress { get; private set; }
        public List<Consulate> Consulates { get; private set; }

        public async Task RefreshAsync()
        {
            // check location permission
            // if not
            // go to permission page

            // get current location and display it
            try
            {
                // TODO: Location permission check here
                var request = new GeolocationRequest(GeolocationAccuracy.Medium);
                var location = await Geolocation.GetLastKnownLocationAsync() ?? await Geolocation.GetLocationAsync(request);

                CurrentLatitude = location.Latitude;
                CurrentLongitude = location.Longitude;

                var placemarks = await Geocoding.GetPlacemarksAsync(location.Latitude, location.Longitude);
                var placemark = placemarks?.FirstOrDefault();

                if (placemark != null)
                {
                    var geocodeAddress =
                        $"{placemark.SubThoroughfare}, " +  // 825-21 | 1 2 
                        $"{placemark.Thoroughfare}, " +  // 역삼동 | 역삼1동
                        $"{placemark.Locality ?? placemark.SubLocality}, " + //강남구 | 강남구 
                        $"{placemark.AdminArea}, " +     // 서울특별시,       서울특별시
                        $"{placemark.CountryName}({placemark.CountryCode})";     // KR,              KR

                    CurrentAddress = geocodeAddress;
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                Debug.WriteLine(fnsEx.Message);
            }
            catch (FeatureNotEnabledException fneEx)
            {
                Debug.WriteLine(fneEx.Message);
            }
            catch (PermissionException pEx)
            {
                Debug.WriteLine(pEx.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            // initialize consulates which ordered by distance from current location
            Consulates = await api.GetConsulatesAsync(CurrentLatitude, CurrentLongitude);

            LocationUpdated?.Invoke(this, new EventArgs());
        }
    }
}
