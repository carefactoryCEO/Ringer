using System;
using Ringer.Models;
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
            var url = (BindingContext as MessageModel).Body;

            await Browser.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
        }
    }
}
