using RingerStaff.ViewModels;
using Xamarin.Forms;

namespace RingerStaff.Views
{
    public partial class RoomsPage : ContentPage
    {
        private RoomPageViewModel vm;

        public RoomsPage()
        {
            InitializeComponent();

            BindingContext = vm = new RoomPageViewModel();
        }

        void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var list = sender as ListView;

            Shell.Current.GoToAsync("chatpage");

            list.SelectedItem = null;
        }
    }
}
