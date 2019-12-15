using System;
using System.Collections.Generic;
using MediaManager;
using Xamarin.Forms;

namespace RingerStaff.Views
{
    public partial class SchedulePage : ContentPage
    {
        public SchedulePage()
        {
            InitializeComponent();
        }

        async void PlayStop_Clicked(object sender, EventArgs e)
        {
            await CrossMediaManager.Current.PlayPause();
        }
    }
}
