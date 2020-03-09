using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Ringer.Views.Cells
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ImageViewCell : ViewCell
    {
        public ImageViewCell()
        {
            InitializeComponent();
        }

        async void Image_Tapped(object sender, EventArgs e)
        {
            //var image = (BindingContext as MessageModel).Body;

            //await Browser.OpenAsync(image, BrowserLaunchMode.SystemPreferred);
        }
    }
}
