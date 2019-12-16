using System;
using RingerStaff.Models;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RingerStaff.Views.Cells
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
            var image = (BindingContext as MessageModel).Body;

            await Browser.OpenAsync(image, BrowserLaunchMode.SystemPreferred);
        }
    }
}
