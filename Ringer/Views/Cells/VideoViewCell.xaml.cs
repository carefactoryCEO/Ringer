using System;
using Ringer.Models;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Ringer.Views.Cells
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
            //await Browser.OpenAsync(video, new BrowserLaunchOptions
            //{
            //    LaunchMode = BrowserLaunchMode.SystemPreferred,
            //    TitleMode = BrowserTitleMode.Hide
            //});

            await Shell.Current.Navigation.PushAsync(new VideoPlayerPage(video));
        }
    }
}
