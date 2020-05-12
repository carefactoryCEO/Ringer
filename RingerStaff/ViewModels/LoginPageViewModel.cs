using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using RingerStaff.Services;
using Xamarin.Forms;

namespace RingerStaff.ViewModels
{
    public class LoginPageViewModel : BaseViewModel
    {
        public LoginPageViewModel()
        {
            LoginCommand = new Command(async () => await LoginAsync());
            EmailText = "test@test.com";
            PasswordText = "string";
        }

        private string _emailText;
        private string _passwordText;

        public ICommand LoginCommand { get; set; }

        private async Task LoginAsync()
        {
            IsBusy = true;

            try
            {
                var token = await ApiService.LogInAsync(EmailText, PasswordText);

                if (!string.IsNullOrEmpty(token))
                {
                    App.Token = token;
                    await RealTimeService.ConnectAsync(App.Huburl, App.Token);
                    await Shell.Current.Navigation.PopModalAsync();
                    MessagingCenter.Send(this, "LoggedIn");
                }
                else
                    Debug.WriteLine("Token is empty");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public string EmailText { get => _emailText; set => SetProperty(ref _emailText, value); }
        public string PasswordText { get => _passwordText; set => SetProperty(ref _passwordText, value); }

        ~LoginPageViewModel()
        {
            Debug.WriteLine("terminated");
        }
    }
}
