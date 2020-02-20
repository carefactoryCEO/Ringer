using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using RingerStaff.Models;
using RingerStaff.Services;
using RingerStaff.Views;
using Xamarin.Forms;

namespace RingerStaff.ViewModels
{
    public class RoomsPageViewModel : BaseViewModel
    {
        public RoomsPageViewModel()
        {
            Title = "채팅";

            SearchCommand = new Command(() => Debug.WriteLine("Searching...."));

            rooms = new ObservableCollection<RoomModel>();

            RefreshCommand = new Command(() =>
            {
                Rooms.Add(new RoomModel
                {
                    Title = "김은미 38F 발리(인도네시아) 복통",
                    LastMessage = "오늘 아침에 갑자기 배가 아파지더니 지금은 일어나지도 못하겠어요.",
                    LastMessageArrivedAt = DateTime.Now,
                    UnreadMessagesCount = 1
                });

                var tempList = Rooms.OrderByDescending(r => r.LastMessageArrivedAt).ToList();

                foreach (var temp in tempList)
                {
                    int oldIndex = Rooms.IndexOf(temp);

                    int newIndex = tempList.IndexOf(temp);

                    Rooms.Move(oldIndex, newIndex);
                }

                Rooms[0].Title += "!";
                Rooms[1].Title += "!!";
                Rooms[2].Title += "!!!";
            });

            LogoutCommand = new Command(async () => await ExcuteLogoutCommandAsync());

        }

        public async Task<bool> LoadRoomsAsync()
        {
            IsBusy = true;

            try
            {
                var roomModels = await ApiService.LoadRoomsAsync();

                if (roomModels.Count == 0)
                    return false;

                Rooms.Clear();

                foreach (var roomModel in roomModels)
                    Rooms.Add(roomModel);

                IsBusy = false;

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }

            return false;
        }

        private async Task ExcuteLogoutCommandAsync()
        {
            App.Token = null;
            await Shell.Current.Navigation.PushModalAsync(new LoginPage());
        }

        private ObservableCollection<RoomModel> rooms;
        public ObservableCollection<RoomModel> Rooms
        {
            get => rooms;
            set => SetProperty(ref rooms, value);
        }

        public ICommand RefreshCommand { get; set; }
        public ICommand LogoutCommand { get; set; }
        public ICommand SearchCommand { get; set; }
    }
}
