using System;
using Xamarin.Forms;

namespace Ringer.Views
{
    public partial class VideoPlayerPage : ContentPage
    {
        public VideoPlayerPage()
        {
            InitializeComponent();
            //mediaPlayer.Source = MediaSource.FromUri(new Uri($"https://ringerstoragekr.blob.core.windows.net/ringer/video-2-20200321-155813-682.mp4", UriKind.RelativeOrAbsolute));
        }

        public VideoPlayerPage(string url) : this()
        {
            webView.Source = url;
        }
    }
}
