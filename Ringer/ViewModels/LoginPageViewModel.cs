using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
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

        public LoginPageViewModel()
        {
            RegisteredEmail = "jhylmb@gmail.com";
            LoginCommand = new Command(async () => await Login());
            ResetPasswordCommand = new Command(async () => await ResetPassword());
        }

        Task Login()
        {
            return Application.Current.MainPage.DisplayAlert(null, "login", "확인");
        }

        Task ResetPassword()
        {
            return Application.Current.MainPage.DisplayAlert(null, "reset", "확인");
        }
    }
}
