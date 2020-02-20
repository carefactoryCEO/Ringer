using RingerStaff.ViewModels;
using Xamarin.Forms;

namespace RingerStaff.Views
{
    public partial class LoginPage : ContentPage
    {
        private LoginPageViewModel vm;

        public LoginPage()
        {
            InitializeComponent();

            BindingContext = vm = new LoginPageViewModel();
        }

    }
}
