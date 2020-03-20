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

        ~RoomsPage()
        {

        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var list = sender as ListView;

            vm.SetRoomId(list.SelectedItem);

            // TODO [Bug]null로 지정하면 itemselected가 두 번 실행된다. 이유를 알아내든가 자마린 팀에 제보하던가..콜렉션 뷰로 가면 될 거 같기도 하고...
            // list.SelectedItem = null;

            await Shell.Current.GoToAsync("chatpage").ConfigureAwait(false);

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
