using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaManager;
using MediaManager.Forms;
using RingerStaff.Models;
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

        async void VideoButton_Clicked(object sender, EventArgs e)
        {
            var video = (BindingContext as MessageModel).Body;

            //await Shell.Current.GoToAsync($"videopage?video={video}");
            await Shell.Current.Navigation.PushModalAsync(new VideoPage(video));
        }

        protected override async void OnBindingContextChanged()
        {
            //var video = (BindingContext as MessageModel).Body;

            //var mediaItem = await CrossMediaManager.Current.Extractor.CreateMediaItem(video);
            //var image = await CrossMediaManager.Current.Extractor.GetVideoFrame(mediaItem, TimeSpan.FromSeconds(0));
            //ImageSource imageSource = image.ToImageSource();
            //Thumbnail.Source = imageSource;

            base.OnBindingContextChanged();
        }
    }
}
