using System;
using System.Collections.Generic;
using System.Diagnostics;
using MediaManager;
using Xamarin.Forms;

namespace RingerStaff.Views
{
    [QueryProperty("Video", "video")]
    public partial class VideoPage : ContentPage
    {
        private string video;

        public VideoPage()
        {
            InitializeComponent();
        }

        public VideoPage(string url) : this()
        {
            VideoView.Source = url;
        }

        public string Video { get => video; set => video = value.Replace("https:/", "https://"); }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            Debug.WriteLine(Video);

            //await CrossMediaManager.Current.Play(Video);
        }

        async void PlayStop_Clicked(object sender, EventArgs e)
        {
            await CrossMediaManager.Current.PlayPause();
        }

        async void Close_Clicked(object sender, EventArgs e)
        {
            await CrossMediaManager.Current.Stop();
            await Navigation.PopModalAsync();
        }
    }
}
