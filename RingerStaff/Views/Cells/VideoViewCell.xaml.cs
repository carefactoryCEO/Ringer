using System;
using RingerStaff.Models;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RingerStaff.Views.Cells
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VideoViewCell : ViewCell
    {
        public object FormsImage { get; private set; }

        public VideoViewCell()
        {
            InitializeComponent();
        }

        async void Video_Clicked(object sender, EventArgs e)
        {
            var video = (BindingContext as MessageModel).Body;

            await Browser.OpenAsync(video, BrowserLaunchMode.SystemPreferred);
        }
    }
}
