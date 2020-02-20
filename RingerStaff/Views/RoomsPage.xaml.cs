using System.Diagnostics;
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

            IsBusy = true;

            try
            {
                if (App.IsLoggedIn)
                {
                    // subscribe new message event of Realtime service

                    // get Rooms from MessageRepository
                    var roomsPopulated = await vm.LoadRoomsAsync();

                    if (!roomsPopulated)
                        await DisplayAlert(null, "방이 없습니다", "닫기");
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
    }
}
