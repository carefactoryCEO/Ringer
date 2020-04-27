using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Ringer.Views.Permission
{
    public partial class PermissionPage : ContentPage
    {
        public PermissionPage()
        {
            InitializeComponent();
            On<iOS>().SetUseSafeArea(true);
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}
