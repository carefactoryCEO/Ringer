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

        protected override void OnBindingContextChanged()
        {
            cachedImage.Source = null;
            var message = BindingContext as MessageModel;

            if (message == null)
                return;

            cachedImage.Source = message.Body;

            base.OnBindingContextChanged();
        }

        async void Image_Tapped(object sender, EventArgs e)
        {
            var url = (BindingContext as MessageModel).Body;

            //await Browser.OpenAsync(url, new BrowserLaunchOptions
            //{
            //    LaunchMode = BrowserLaunchMode.SystemPreferred,
            //    TitleMode = BrowserTitleMode.Hide

            //});

            await Shell.Current.Navigation.PushAsync(new ImageViewerPage(url));
        }
    }
}
