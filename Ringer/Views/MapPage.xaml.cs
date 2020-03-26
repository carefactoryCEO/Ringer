using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Maps;
using Ringer.ViewModels;
using Ringer.Core.Models;

namespace Ringer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        private readonly MapPageViewModel vm;

        public MapPage()
        {
            InitializeComponent();
            BindingContext = vm = new MapPageViewModel();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await vm.RefreshConsulatesAsync();
            MoveMap(vm.CurrentLatitude, vm.CurrentLongitude);
        }

        private void MoveMap(double lat, double lon, string labelString = null, string address = null)
        {
            Position position = new Position(lat, lon);
            var mapSpan = MapSpan.FromCenterAndRadius(position, Distance.FromMeters(175));

            RingerMap.MoveToRegion(mapSpan);
            RingerMap.IsShowingUser = true;

            if (labelString != null)
            {
                RingerMap.Pins.Add(new Pin
                {
                    Label = labelString,
                    Address = address,
                    Type = PinType.Place,
                    Position = position
                });
            }
        }

        private void MapCurrentButton_Clicked(object sender, EventArgs e)
        {
            MoveMap(vm.CurrentLatitude, vm.CurrentLongitude);
        }

        void ConsulateCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var consulate = e.CurrentSelection.FirstOrDefault() as Consulate;

            MoveMap(consulate.Latitude, consulate.Longitude, consulate.KoreanName, consulate.Address);
        }
    }
}