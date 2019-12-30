using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Ringer.Core.Data;
using Xamarin.Forms;

namespace RingerStaff.ViewModels
{
    public class LoginPageViewModel : BaseViewModel
    {
        public LoginPageViewModel()
        {
            _client = new HttpClient();

            LoginCommand = new Command(async () => await LoginAsync());
            CancelCommand = new Command(async () => await CancelAsync());
        }

        private HttpClient _client;
        private string _emailText;
        private string _passwordText;

        public ICommand LoginCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        private async Task LoginAsync()
        {
            IsBusy = true;

            //

            var loginInfo = JsonSerializer.Serialize(new LoginInfo
            {
                Email = EmailText,
                Password = PasswordText,
                DeviceId = "device id here",
                DeviceType = Device.RuntimePlatform == Device.iOS ? DeviceType.iOS : DeviceType.Android
            });

            HttpResponseMessage response = await _client.PostAsync("http://localhost:5000/auth/staff-login", new StringContent(loginInfo, Encoding.UTF8, "application/json"));

            // 로그인 실패
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                Debug.WriteLine(await response.Content.ReadAsStringAsync());

            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<ResponseJson>(responseString);

            Debug.WriteLine(responseObject.token);
            Debug.WriteLine(responseObject.roomId);

            //
            IsBusy = false;

            await Shell.Current.Navigation.PopModalAsync();
        }

        private async Task CancelAsync()
        {
            await Shell.Current.Navigation.PopModalAsync();
        }

        public string EmailText { get => _emailText; set => SetProperty(ref _emailText, value); }
        public string PasswordText { get => _passwordText; set => SetProperty(ref _passwordText, value); }
    }
}
