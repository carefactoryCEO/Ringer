using Ringer.Views.Permission;
using Xamarin.Forms;

namespace Ringer.Views.Intro
{
    public partial class IntroPage : ContentPage
    {
        public IntroPage()
        {
            InitializeComponent();
        }

        async void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            await Navigation.PushModalAsync(new PermissionPage());
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}
