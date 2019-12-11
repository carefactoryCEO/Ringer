using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using RingerStaff.Models;
using Xamarin.Forms;

namespace RingerStaff.ViewModels
{
    public class RoomPageViewModel : BaseViewModel
    {
        public RoomPageViewModel()
        {
            Title = "채팅";

            Rooms = new ObservableCollection<RoomModel>
            {
                new RoomModel
                {
                    Description = "신모범 43M 시카고(미국) 두통",
                    LastMessage = "어제부터 오른쪽 관자놀이가 아프더라구요.",
                    LastMessageArrivedAt = DateTime.Now.Subtract(TimeSpan.FromHours(1)),
                    UnreadMessagesCount = 5
                },
                new RoomModel
                {
                    Description = "김순용 39M 방콕(태국) 무좀",
                    LastMessage = "어렸을 때부터 오른발에 무좀이 심했어요.",
                    LastMessageArrivedAt = DateTime.Now.Subtract(TimeSpan.FromMinutes(58)),
                    UnreadMessagesCount = 4
                }
            };

            RefreshCommand = new Command(() =>
            {
                Rooms.Add(new RoomModel
                {
                    Description = "김은미 38F 발리(인도네시아) 복통",
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

                Rooms[0].Description += "!";
                Rooms[1].Description += "!!";
                Rooms[2].Description += "!!!";
            });


        }

        private ObservableCollection<RoomModel> rooms;
        public ObservableCollection<RoomModel> Rooms
        {
            get => rooms;
            set => SetProperty(ref rooms, value);
        }

        public ICommand RefreshCommand { get; set; }
    }
}
