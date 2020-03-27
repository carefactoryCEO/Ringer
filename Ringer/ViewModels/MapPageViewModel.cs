using System.Collections.ObjectModel;
using System.Diagnostics;
using Ringer.Models;
using Xamarin.Essentials;
using System.Linq;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Ringer.Services;
using System;
using Ringer.Core.Models;
using System.Collections.Generic;
using Ringer.Helpers;

namespace Ringer.ViewModels
{
    public class MapPageViewModel : INotifyPropertyChanged
    {
        private readonly ILocationService location;

        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<Infomation> Infomations { get; set; }
        public ObservableCollection<ConsulateModel> Consulates { get; set; } = new ObservableCollection<ConsulateModel>();
        public string CurrentAddress { get; set; } = "검색 중...";
        public double CurrentLatitude { get; set; } = double.NegativeInfinity;
        public double CurrentLongitude { get; set; } = double.NegativeInfinity;
        public string RingerPhoneNumber { get; set; } = Constants.RingerPhoneNumber;
        public string RingerEmergencyPhoneNumber { get; set; } = Constants.RingerEmergencyPhoneNumber;

        public ICommand GoToChatPageCommand { get; private set; }
        public ICommand SendEmailCommand { get; private set; }
        public ICommand OpenSiteCommand { get; private set; }
        public ICommand OpenMapCommand { get; private set; }
        public ICommand PhoneCallCommand { get; private set; }
        public ICommand EmergencyPhoneCallCommand { get; private set; }

        public MapPageViewModel()
        {
            location = DependencyService.Get<ILocationService>();
            location.LocationUpdated += (s, e) => SetConsulates();

            GoToChatPageCommand = new Command(() => GoToChatPage());
            SendEmailCommand = new Command<ConsulateModel>(async consulate => await SendEmail(consulate));
            OpenSiteCommand = new Command<ConsulateModel>(async consulate => await OpenSite(consulate));
            PhoneCallCommand = new Command<string>(number => PhoneCall(number));
            OpenMapCommand = new Command<ConsulateModel>(async consulate => await OpenMap(consulate));

            //Dummy();
        }

        public async Task RefreshConsulatesAsync()
        {
            //return;

            if (Consulates.Any())
                Consulates.Clear();

            await location.RefreshAsync();
        }

        private void SetConsulates()
        {
            CurrentAddress = location.CurrentAddress;
            CurrentLatitude = location.CurrentLatitude;
            CurrentLongitude = location.CurrentLongitude;

            Consulates.Add(new ConsulateModel
            {
                IsHeader = true,
                KoreanName = "현재 위치",
                Address = CurrentAddress,
                Latitude = CurrentLatitude,
                Longitude = CurrentLongitude,
            });

            foreach (var model in location.Consulates.Take(15))
                Consulates.Add(model);

            Consulates.Add(new ConsulateModel
            {
                IsFooter = true,
                KoreanName = "링거 서포트팀",
                Address = "불편 사항, 접속 장애 등 링거와 긴급히 연락해야 할 때 아래의 연락처를 이용하세요.",
                PhoneNumber = Constants.RingerPhoneNumber,
                EmergencyPhoneNumber = Constants.RingerEmergencyPhoneNumber,
            });
        }
        private void PhoneCall(string number)
        {
            try
            {
                PhoneDialer.Open(number);
            }
            catch (ArgumentNullException anEx)
            {
                // Number was null or white space
            }
            catch (FeatureNotSupportedException ex)
            {
                // Phone Dialer is not supported on this device.
            }
            catch (Exception ex)
            {
                // Other error has occurred.
            }
        }
        private void GoToChatPage()
        {
            Shell.Current.GoToAsync("chatpage?room=fromMap");
        }
        private async Task OpenMap(Consulate consulate)
        {
            var location = new Location(consulate.Latitude, consulate.Longitude);
            var options = new MapLaunchOptions { Name = consulate.KoreanName };

            await Map.OpenAsync(location, options);
        }
        private async Task OpenSite(Consulate consulate)
        {
            await Browser.OpenAsync(consulate.Homepage, new BrowserLaunchOptions
            {
                LaunchMode = BrowserLaunchMode.SystemPreferred,
                TitleMode = BrowserTitleMode.Show
            });
        }
        private async Task SendEmail(Consulate consulate)
        {
            try
            {
                var message = new EmailMessage
                {
                    Subject = null,
                    Body = null,
                    To = new List<string> { consulate.Email },
                    //Cc = ccRecipients,
                    //Bcc = bccRecipients
                };
                await Email.ComposeAsync(message);
            }
            catch (FeatureNotSupportedException fbsEx)
            {
                Utility.Trace(fbsEx.Message, true);
            }
            catch (Exception ex)
            {
                Utility.Trace(ex.Message, true);
            }

        }
        private void Dummy()
        {
            var con = new ConsulateModel[]
            {
                new ConsulateModel
                {
                    Id = 91,
                    ConsulateType = "출장소",
                    KoreanName = "주 앵커리지 대한민국 출장소",
                    LocalName = "Consular Office of the Republic of Korea in Anchorage",
                    PhoneNumber = "+1-907-339-7955",
                    EmergencyPhoneNumber = "+1-907-331-7135",
                    Email = "anchorage@mofa.go.kr",
                    Address = "800 E. Dimond Blvd., STE 3-695, ANCHORAGE, AK 99515, U.S.A",
                    Latitude = 61.143544,
                    Longitude = -149.868567,
                    Homepage = "http://overseas.mofa.go.kr/us-anchorage-ko/index.do,61.143544,-149.868567",
                    GoogleMap = "https://www.google.com/maps/place/800+E+Dimond+Blvd+STE+3-695,+Anchorage,+AK+99515+%EB%AF%B8%EA%B5%AD/@61.1433707,-149.8685129,17z/data=!3m1!4b1!4m5!3m4!1s0x56c899d0febf4903:0xca560edaca613cca!8m2!3d61.1433707!4d-149.8685129?hl=ko",
                    Distance = 3.4
                },
                new ConsulateModel
                {
                    Distance = 5.2,
                    Id = 92,
                    ConsulateType = "출장소",
                    KoreanName = "주 하갓냐 대한민국 출장소",
                    LocalName = "Consulate Agency of the Republic of Korea in Hagatna",
                    PhoneNumber = "+1-671-647-6488",
                    EmergencyPhoneNumber = "+1-671-688-5886",
                    Email = "kconsul_guam@mofa.go.kr",
                    Latitude = 13.499326,
                    Longitude = 144.777286,
                    Address = "153 Zoilo St., Tamuning, Guam 96913 U.S.A.",
                    Homepage = "http://overseas.mofa.go.kr/us-hagatna-ko/index.do,13.499326,144.777286",
                    GoogleMap = "https://www.google.com/maps/place/KOREAN+CONSULATE+AGENCY(%EB%8C%80%ED%95%9C%EB%AF%BC%EA%B5%AD+%EC%B4%9D%EC%98%81%EC%82%AC%EA%B4%80(%EC%B6%9C%EC%9E%A5%EC%86%8C))/@13.498935,144.777307,17z/data=!3m1!4b1!4m5!3m4!1s0x671f828ca1b8716d:0x884a3192f65241f4!8m2!3d13.498935!4d144.777307?hl=ko"

                },
                new ConsulateModel
                {
                    Distance = 114.5,
                    Id = 115,
                    ConsulateType = "총영사관",
                    KoreanName = "주 뉴욕 대한민국 총영사관",
                    LocalName = "Consulate General of the Republic of Korea in New York",
                    PhoneNumber = "+1-646-674-6000",
                    EmergencyPhoneNumber = "+1-646-965-3639",
                    Email = "kcg_ny@mofa.go.kr",
                    Address = "460 Park Ave.(bet.57th & 58th St.) New York, NY 10022",
                    Homepage = "http://overseas.mofa.go.kr/us-newyork-ko/index.do,40.76227,-73.970984",
                    GoogleMap = "https://www.google.com/maps/place/%EC%A3%BC+%EB%89%B4%EC%9A%95+%EB%8C%80%ED%95%9C%EB%AF%BC%EA%B5%AD+%EC%B4%9D%EC%98%81%EC%82%AC%EA%B4%80/@40.7620224,-73.971005,17z/data=!3m1!4b1!4m5!3m4!1s0x89c258e53588393f:0x995641481cd802c8!8m2!3d40.7620224!4d-73.971005?hl=ko",

                    Latitude = 40.76227,
                    Longitude = -73.970984,

                },
                new ConsulateModel
                {
                    Distance = 160.8,
                    Id = 116,
                    ConsulateType = "총영사관",
                    KoreanName = "주 애틀랜타 대한민국 총영사관",
                    LocalName = "Consulate General of the Republic of Korea in Atlanta",
                    PhoneNumber = "+404-522-1611",
                    EmergencyPhoneNumber = "+404-295-2807",
                    Email = "atlanta@mofa.go.kr",
                    Address = "229 Peachtree Street NE, Suite 2100, International Tower, Atlanta, GA 30303",
                    Homepage = "http://overseas.mofa.go.kr/us-atlanta-ko/index.do,33.760121,-84.386501",
                    GoogleMap = "https://www.google.com/maps/place/%EC%A3%BC+%EC%95%A0%ED%8B%80%EB%9E%9C%ED%83%80+%EB%8C%80%ED%95%9C%EB%AF%BC%EA%B5%AD+%EC%B4%9D%EC%98%81%EC%82%AC%EA%B4%80/@33.7598307,-84.3864363,17z/data=!3m1!4b1!4m5!3m4!1s0x88f5047818876a2b:0x256cf6046e161f9e!8m2!3d33.7598307!4d-84.3864363?hl=ko",
                    Latitude = 33.760121,
                    Longitude = -84.386501,
                },
                new ConsulateModel
                {
                    Distance = 2078.5,
                    Id = 117,
                    ConsulateType = "총영사관",
                    KoreanName = "주 시카고 대한민국 총영사관",
                    LocalName = "Consulate General of the Republic of Korea in Chicago",
                    PhoneNumber = "+312-822-9485",
                    EmergencyPhoneNumber = "+312-405-4425",
                    Email = "chicago@mofa.go.kr",
                    Address = "Korean Consulate General NBC Tower Suite 2700, 455 N. Cityfront Plaza Dr. Chicago, IL 60611",
                    Homepage = "https://overseas.mofa.go.kr/us-chicago-ko/index.do,41.890154,-87.621363,",
                    GoogleMap = "https://www.google.com/maps/place/%EC%A3%BC+%EC%8B%9C%EC%B9%B4%EA%B3%A0+%EB%8C%80%ED%95%9C%EB%AF%BC%EA%B5%AD+%EC%B4%9D%EC%98%81%EC%82%AC%EA%B4%80/@41.8899579,-87.6213417,17z/data=!3m1!4b1!4m5!3m4!1s0x880e2ca97c123f17:0x2b6a7fddc2b2de6!8m2!3d41.8899579!4d-87.6213417?hl=ko",
                    Latitude = 41.890154,
                    Longitude = -87.621363,
                },

            };

            Consulates.Add(new ConsulateModel
            {
                IsHeader = true,
                KoreanName = "현재 위치",
                Address = "Fake Address here, 123-4, KR",
                Latitude = 0,
                Longitude = 0
            });

            foreach (var c in con)
                Consulates.Add(c);

            Consulates.Add(new ConsulateModel
            {
                IsFooter = true,
                KoreanName = "링거 서포트팀",
                Address = "불편 사항, 접속 장애 등 링거와 긴급히 연락해야 할 때 아래의 연락처를 이용하세요.",
                PhoneNumber = Constants.RingerPhoneNumber,
                EmergencyPhoneNumber = Constants.RingerEmergencyPhoneNumber,
            });
        }
    }
}