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
using static Xamarin.Essentials.Permissions;

namespace Ringer.Services
{
    public interface ILocationService
    {
        double CurrentLatitude { get; }
        double CurrentLongitude { get; }
        string CurrentAddress { get; }
        List<ConsulateModel> Consulates { get; }
        string CurrentCountryCode { get; }

        event EventHandler LocationUpdated;

        Task RefreshAsync();
        Task<PermissionStatus> CheckAndRequestPermissionAsync<T>() where T : BasePermission, new();
        Task<bool> CheckPermissionAsync<T>() where T : BasePermission, new();
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
        public string CurrentCountryCode { get; private set; }
        public List<ConsulateModel> Consulates { get; private set; }

        public async Task RefreshAsync()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium);

                var location = await Geolocation.GetLastKnownLocationAsync() ?? await Geolocation.GetLocationAsync(request);
                lastLocation = location;

                //if (lastLocation != null && location.CalculateDistance(lastLocation, DistanceUnits.Kilometers) < 100)
                //    return;

                CurrentLatitude = location.Latitude;
                CurrentLongitude = location.Longitude;

                IEnumerable<Placemark> placemarks = await Geocoding.GetPlacemarksAsync(location.Latitude, location.Longitude);

                var placemark = placemarks?.FirstOrDefault();

                if (placemark != null)
                {
                    string geocodeAddress = string.Empty;

                    if (placemark.SubThoroughfare != null)
                        geocodeAddress += $"{placemark.SubThoroughfare}, ";

                    if (placemark.Thoroughfare != null)
                        geocodeAddress += $"{placemark.Thoroughfare}, ";

                    if (placemark.Locality != null || placemark.SubAdminArea != null)
                        geocodeAddress += $"{placemark.Locality ?? placemark.SubLocality}, ";

                    if (placemark.AdminArea != null)
                        geocodeAddress += $"{placemark.AdminArea}, ";

                    if (placemark.CountryName != null)
                        geocodeAddress += placemark.CountryName;

                    if (placemark.CountryCode != null)
                        geocodeAddress += $"({placemark.CountryCode})";

                    CurrentAddress = geocodeAddress;
                    CurrentCountryCode = placemark.CountryCode;
                }



                Consulates = await api.GetConsulatesByCoordinateAsync(CurrentLatitude, CurrentLongitude);

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

        public async Task<PermissionStatus> CheckAndRequestPermissionAsync<T>()
            where T : BasePermission, new()
        {
            T permission = new T();

            var status = await permission.CheckStatusAsync();

            if (status != PermissionStatus.Granted)
            {
                status = await permission.RequestAsync();
            }

            // Additionally could prompt the user to turn on in settings

            return status;
        }

        public async Task<bool> CheckPermissionAsync<T>() where T : BasePermission, new()
        {
            T permission = new T();
            return (await permission.CheckStatusAsync() == PermissionStatus.Granted) ? true : false;
        }
    }
}
