using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Ringer.Core.Models;
using Ringer.Helpers;
using Ringer.Services;
using Ringer.Views;
using Xamarin.Forms;

namespace Ringer.ViewModels
{
    [QueryProperty("RegisteredEmail", "RegisteredEmail")]
    public class LoginPageViewModel : INotifyPropertyChanged
    {
        private string registeredEmail;

        public event PropertyChangedEventHandler PropertyChanged;

        public string RegisteredEmail
        {
            get => registeredEmail;
            set
            {
                Email = value;
                registeredEmail = value;
            }
        }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsLoginButtonEnabled => Email?.Length > 0 && Password?.Length > 5;
        public bool IsEmailEntryEnabled => RegisteredEmail == null;

        public ICommand LoginCommand { get; set; }
        public ICommand ResetPasswordCommand { get; set; }
        public bool IsBusy { get; set; }

        public LoginPageViewModel()
        {
            LoginCommand = new Command(async () => await Login());
            ResetPasswordCommand = new Command(async () => await ResetPassword());
        }

        async Task Login()
        {
            var rest = DependencyService.Get<IRESTService>();

            var user = new User
            {
                Email = Email,
                Password = Password
            };

            var device = new Core.Models.Device
            {
                Id = App.DeviceId,
                DeviceType = Utility.iOS ? Core.Data.DeviceType.iOS : Core.Data.DeviceType.Android,
                IsOn = true
            };

            IsBusy = true;
            var result = await rest.LoginConsumerAsync(user, device);
            IsBusy = false;

            if (result == AuthResult.Succeed)
            {
                IsBusy = true;
                // init messaging
                var messaging = DependencyService.Get<IMessaging>();
                await messaging.InitAsync(Constants.HubUrl, App.Token);

                // record foot print
                var location = DependencyService.Get<ILocationService>();
                await location.RecordFootPrintAsync();

                IsBusy = false;

                await Shell.Current.GoToAsync($"//{nameof(MapPage)}/{nameof(ChatPage)}?from=LoginPage");
            }
            else if (result == AuthResult.ServerError || result == AuthResult.Unknown)
            {
                await Shell.Current.DisplayAlert(null, $"로그인에 실패했습니다.\n네트워크 환경이 이유일 수 있습니다.\n네트워크를 확인하고 다시 시도해보세요.", "확인");

            }
            else if (result == AuthResult.LoginFailed)
            {
                var errorMessage = _loginFailingCount > 2 ? "\n비밀번호가 생각나지 않으면 이메일로 임시 비밀번호를 받으세요." : "\n정확한 비밀번호로 로그인해주세요.";
                _loginFailingCount++;

                await Shell.Current.DisplayAlert(null, $"비밀번호가 틀렸습니다.{errorMessage}", "확인");
            }
        }

        int _loginFailingCount = 0;

        Task ResetPassword()
        {
            return Application.Current.MainPage.DisplayAlert(null, "reset", "확인");
        }
    }
}
