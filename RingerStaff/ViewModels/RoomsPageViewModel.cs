using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ringer.Core.Data;
using Ringer.Core.Models;
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
            GoChatCommand = new Command<object>(async room => await GoChat(room));

            RealTimeService.MessageReceived += RealTimeService_MessageReceived;

        }

        private async Task GoChat(object room)
        {
            RoomModel model = (RoomModel)room;
            //App.UnreadCounts[model.Id] = 0;
            await Shell.Current.GoToAsync($"{nameof(ChatPage)}?RoomId={model.Id}");
        }

        private void RealTimeService_MessageReceived(object sender, Ringer.Core.EventArgs.MessageReceivedEventArgs e)
        {
            if (App.RoomId == e.RoomId)
                return;

            var room = Rooms.FirstOrDefault(r => r.Id == e.RoomId);

            room.LastMessage = e.Body;
            room.LastMessageArrivedAt = e.CreatedAt;
            room.UnreadMessagesCount++;

            App.UnreadCounts[e.RoomId] = room.UnreadMessagesCount;

            // 도착 역순으로 재배치
            var orderedRooms = Rooms.OrderByDescending(r => r.LastMessageArrivedAt).ToArray();
            Rooms.Clear();
            foreach (var roomModel in orderedRooms)
            {
                Rooms.Add(roomModel);
            }
        }

        public async Task<bool> LoadRoomsAsync()
        {
            if (!App.IsLoggedIn)
                return false;

            IsBusy = true;

            try
            {
                List<RoomInformation> roomInfors = await ApiService.LoadRoomsAsync();

                if (roomInfors.Count == 0)
                    return false;

                Rooms.Clear();

                var orderedRoomsInfos = roomInfors.Where(r => r.LastMessage != null).OrderByDescending(r => r.LastMessage.CreatedAt);

                foreach (var roomInfo in orderedRoomsInfos)
                {
                    if (!App.UnreadCounts.ContainsKey(roomInfo.Room.Id))
                        App.UnreadCounts[roomInfo.Room.Id] = 0;

                    var room = new RoomModel
                    {
                        Title = roomInfo.Room.Name,
                        Id = roomInfo.Room.Id,
                        LastMessage = (roomInfo.LastMessage is null) ? default : roomInfo.LastMessage.Body,
                        LastMessageArrivedAt = (roomInfo.LastMessage is null) ? default : roomInfo.LastMessage.CreatedAt,
                        UnreadMessagesCount = App.UnreadCounts[roomInfo.Room.Id]
                    };

                    Rooms.Add(room);
                }

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
            Rooms.Clear();

            await RealTimeService.DisconnectAsync();

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
        public ICommand GoChatCommand { get; set; }
    }
}
