using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Ringer.Views
{
    public partial class VidyoPage : ContentPage
    {
        public VidyoPage()
        {
            InitializeComponent();
        }

        async void Button_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PopModalAsync();
        }
    }
}
