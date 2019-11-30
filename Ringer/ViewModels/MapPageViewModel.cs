using System.Collections.ObjectModel;
using System.Diagnostics;
using Ringer.Models;
using Xamarin.Essentials;
using System.Linq;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Ringer.ViewModels
{
    public class MapPageViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Infomation> Infomations { get; set; }

        public MapPageViewModel()
        {
            #region dummy information
            Infomations = new ObservableCollection<Infomation>
            {
                new Infomation
                {
                    Title = "Korean Consulate Agency",
                    Location = "153 Zoilo St, Tamuning, Guam 96913 US",
                    PhoneNumber = "+1 671-647-6488",
                    Extras = "대한민국 총영사관(출장소)"
                },

                new Infomation
                {
                    Title = "한인병원",
                    Location = "Tamuning, 96913, Guam",
                    PhoneNumber = "+1-646-2984"
                },

                new Infomation
                {
                    Title = "OKA Pharmacy",
                    Location = "241 Farenholt Ave. Suite 106",
                    PhoneNumber = "671-647-1193",
                    Extras = "한국인 약사"
                },

                new Infomation
                {
                    Title = "American Pediatric Clinic",
                    Location = "241 Farenholt Ave. Suite 106",
                    PhoneNumber = "671-647-2722",
                    Extras = "소아과, 필리핀 여의사"
                },

                new Infomation
                {
                    Title = "PMC Isla Health System",
                    Location = "ADO 177-C Chaian Pasaheru, Tamuning",
                    PhoneNumber = "671-646-6160",
                    Extras = "내과, 한국인 의사"
                },
                new Infomation
                {
                    Title = "Express Care Clinic",
                    Location = "ADO Micronesia Mall, West marine Drive, Suite 239",
                    PhoneNumber = "671-637-2873",
                    Extras = "가정의학과, 한국인 의사"
                },
                 new Infomation
                {
                    Title = "OKA Pharmacy",
                    Location = "ADO 241, Farenholt Ave. Suite 106",
                    PhoneNumber = "671-647-1193",
                    Extras = "한국인 약사"
                },

                new Infomation
                {
                    Title = "American Pediatric Clinic",
                    Location = "ADO 241, Farenholt Ave. Suite 106",
                    PhoneNumber = "671-647-2722",
                    Extras = "소아과, 필리핀 여의사"
                },

                new Infomation
                {
                    Title = "PMC Isla Health System",
                    Location = "ADO 177-C Chaian Pasaheru, Tamuning",
                    PhoneNumber = "671-646-6160",
                    Extras = "내과, 한국인 의사"
                },
                new Infomation
                {
                    Title = "Express Care Clinic",
                    Location = "ADO Micronesia Mall, West marine Drive, Suite 239",
                    PhoneNumber = "671-637-2873",
                    Extras = "가정의학과, 한국인 의사"
                }
            };
            #endregion

            // GetLocationAsync().ConfigureAwait(false);

            GoToChatPageCommand = new Command(async () =>
            {
                await Shell.Current.GoToAsync("chatpage?room=fromMap");
            });
        }

        public ICommand GoToChatPageCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        bool isCurrentLocationInserted;

        public async Task InsertCurrentLocationAsync(Location location)
        {
            if (isCurrentLocationInserted || location == null)
                return;

            var placemarks = await Geocoding.GetPlacemarksAsync(location.Latitude, location.Longitude);
            var placemark = placemarks?.FirstOrDefault();

            if (placemark != null)
            {
                var geocodeAddress =
                    $"{placemark.FeatureName}, " +
                    $"{placemark.Locality}, " +
                    $"{placemark.AdminArea} " +
                    $"{placemark.PostalCode} " +
                    $"{placemark.CountryCode}";

                // Walt Disney World Resort, Orlando, FL 32830 US
                // 10600 N Tantau Ave, Cupertino, CA 95014 미국

                var geocodeAddress1 =
                    $"AdminArea:       {placemark.AdminArea}\n" +
                    $"CountryCode:     {placemark.CountryCode}\n" +
                    $"CountryName:     {placemark.CountryName}\n" +
                    $"FeatureName:     {placemark.FeatureName}\n" +
                    $"Locality:        {placemark.Locality}\n" +
                    $"PostalCode:      {placemark.PostalCode}\n" +
                    $"SubAdminArea:    {placemark.SubAdminArea}\n" +
                    $"SubLocality:     {placemark.SubLocality}\n" +
                    $"SubThoroughfare: {placemark.SubThoroughfare}\n" +
                    $"Thoroughfare:    {placemark.Thoroughfare}\n";

                Debug.WriteLine(geocodeAddress1);

                var infomation = new Infomation
                {
                    Title = "현위치",
                    Location = geocodeAddress
                };

                Infomations.Insert(0, infomation);

                isCurrentLocationInserted = true;
            }

        }
    }
}