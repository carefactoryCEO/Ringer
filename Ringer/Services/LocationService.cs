using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Ringer.Core.Models;
using Ringer.Helpers;
using Ringer.Models;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Ringer.Services
{
    public interface ILocationService
    {
        double CurrentLatitude { get; }
        double CurrentLongitude { get; }
        string CurrentAddress { get; }
        List<ConsulateModel> Consulates { get; }

        event EventHandler LocationUpdated;

        Task RefreshAsync();
    }

    public class LocationService : ILocationService
    {
        private readonly IRESTService api;
        private Location lastLocation;

        public event EventHandler LocationUpdated;

        public LocationService()
        {
            api = DependencyService.Get<IRESTService>();
        }

        public double CurrentLatitude { get; private set; }
        public double CurrentLongitude { get; private set; }
        public string CurrentAddress { get; private set; }
        public List<ConsulateModel> Consulates { get; private set; }

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
                lastLocation = location;

                //if (lastLocation != null && location.CalculateDistance(lastLocation, DistanceUnits.Kilometers) < 100)
                //    return;

                CurrentLatitude = location.Latitude;
                CurrentLongitude = location.Longitude;

                var placemarks = await Geocoding.GetPlacemarksAsync(location.Latitude, location.Longitude);

                var placemark = placemarks?.FirstOrDefault();

                if (placemark != null)
                {
                    var geocodeAddress =
                        $"{placemark.SubThoroughfare}, " +
                        $"{placemark.Thoroughfare}, " +
                        $"{placemark.Locality ?? placemark.SubLocality}, " +
                        $"{placemark.AdminArea}, " +
                        $"{placemark.CountryName}({placemark.CountryCode})";

                    CurrentAddress = geocodeAddress;
                }

                Consulates = await api.GetConsulatesAsync(CurrentLatitude, CurrentLongitude);

                LocationUpdated?.Invoke(this, new EventArgs());
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
        }
    }
}
