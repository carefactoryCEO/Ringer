using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace RingerStaff.ViewModels
{
    public class LoginPageViewModel : BaseViewModel
    {
        public LoginPageViewModel()
        {
            LoginCommand = new Command(async () => await LoginAsync());
            CancelCommand = new Command(async () => await CancelAsync());
        }

        public ICommand LoginCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        private async Task LoginAsync()
        {
            IsBusy = true;
            await Task.Delay(1000);
            IsBusy = false;

            await Shell.Current.Navigation.PopModalAsync();
        }

        private async Task CancelAsync()
        {
            await Shell.Current.Navigation.PopModalAsync();
        }
    }
}
