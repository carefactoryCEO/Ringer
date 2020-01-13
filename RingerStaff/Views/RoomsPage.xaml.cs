using System.Linq;
using RingerStaff.ViewModels;
using Xamarin.Forms;

namespace RingerStaff.Views
{
    public partial class RoomsPage : ContentPage
    {
        private RoomsPageViewModel vm;

        public RoomsPage()
        {
            InitializeComponent();

            BindingContext = vm = new RoomsPageViewModel();
        }

        void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var list = sender as ListView;
            list.SelectedItem = null;

            Shell.Current.GoToAsync("chatpage");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // subscribe new message event of Realtime service

            // get Rooms from MessageRepository
            await vm.LoadRoomsAsync();

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // unsubscribe new message event of Realtime service
        }
    }
}
