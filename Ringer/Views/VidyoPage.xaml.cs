using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Essentials;

using Xamarin.Forms;

namespace Ringer.Views
{
    [QueryProperty("VidyoRoom", "vidyoRoom")]
    public partial class VidyoPage : ContentPage
    {
        private string vidyoRoom;

        public VidyoPage()
        {
            InitializeComponent();
        }

        public string VidyoRoom
        {
            get => vidyoRoom;
            set
            {
                vidyoRoom = value;
                Debug.WriteLine(vidyoRoom);
            }
        }

        async void Button_Clicked(object sender, EventArgs e)
        {
            //await Shell.Current.GoToAsync("//chatpage?room=fromVidyo");
            await Shell.Current.Navigation.PopToRootAsync();
        }

        private async void Button_Clicked_1(object sender, EventArgs e)
        {
            await Browser.OpenAsync("https://appr.tc");
        }
    }
}
