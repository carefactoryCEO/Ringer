using System.Diagnostics;
using System.Linq;
using RingerStaff.Models;
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

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            IsBusy = true;

            try
            {
                if (App.IsLoggedIn)
                {
                    var roomsPopulated = await vm.LoadRoomsAsync();
                }
                else
                {
                    if (Navigation.ModalStack.Count == 0 || !(Navigation.ModalStack.Last() is LoginPage))
                        await Navigation.PushModalAsync(new LoginPage());
                }
            }
            catch (System.Exception ex)
            {
                Debug.Write(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // unsubscribe new message event of Realtime service
        }

        async void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RoomModel selectedRoom = e.CurrentSelection.FirstOrDefault() as RoomModel;

            vm.SetRoomId(selectedRoom);

            await Shell.Current.GoToAsync("chatpage").ConfigureAwait(false);
        }
    }
}
