using System;
using Xamarin.Forms;

namespace Ringer.Views
{
    public partial class VideoPlayerPage : ContentPage
    {
        public VideoPlayerPage()
        {
            InitializeComponent();
        }

        public VideoPlayerPage(string url) : this()
        {
            webView.Source = url;
        }
    }
}
