using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RingerStaff.Models;
using RingerStaff.Types;

namespace RingerStaff.Services
{
    public static class MessageRepository
    {
        public static Task<List<RoomModel>> LoadRoomsAsync()
        {
            var result = new List<RoomModel>
            {
                new RoomModel
                {
                    Title = "신모범 43M 시카고(미국) 두통",
                    LastMessage = "어제부터 오른쪽 관자놀이가 아프더라구요.",
                    LastMessageArrivedAt = DateTime.Now.Subtract(TimeSpan.FromHours(1)),
                    UnreadMessagesCount = 5
                },
                new RoomModel
                {
                    Title = "김순용 39M 방콕(태국) 무좀",
                    LastMessage = "어렸을 때부터 오른발에 무좀이 심했어요.",
                    LastMessageArrivedAt = DateTime.Now.Subtract(TimeSpan.FromMinutes(58)),
                    UnreadMessagesCount = 4
                }
            };

            return Task.FromResult(result);
        }

        public static Task<List<MessageModel>> LoadMessageAsync()
        {
            var messageModels = new List<MessageModel>
            {
                new MessageModel { Body = "안녕하세요 링거입니다.✊ 무엇을 도와드릴까요?", Sender = "Ringer", MessageTypes = MessageTypes.Incomming | MessageTypes.Leading | MessageTypes.Text },
                new MessageModel { Body = "편안하게 말씀하세요. 듣고 있습니다.", Sender = "Ringer",  MessageTypes = MessageTypes.Incomming | MessageTypes.Trailing | MessageTypes.Text },

                new MessageModel { Body = "미국에 사는 44세 남성입니다.", Sender = "p", MessageTypes = MessageTypes.Outgoing | MessageTypes.Leading | MessageTypes.Text},
                new MessageModel { Body = "어제부터 머리가 자꾸 빠지는군요.", Sender = "p", MessageTypes = MessageTypes.Outgoing | MessageTypes.Text },
                new MessageModel { Body = "아주 숭덩숭덩 빠짐 ㅠㅠ", Sender = "p", MessageTypes = MessageTypes.Outgoing | MessageTypes.Trailing | MessageTypes.Text },

                //new MessageModel { Body = "자라나라 머리머리 👊", Sender = "Ringer", MessageTypes = MessageTypes.Incomming | MessageTypes.Leading },
                new MessageModel { Body = "자라나라 머리머리", Sender = "Ringer", MessageTypes = MessageTypes.Incomming | MessageTypes.Leading | MessageTypes.Text },
                new MessageModel { Body = "농담입니다.😎", Sender = "Ringer", MessageTypes = MessageTypes.Incomming | MessageTypes.Trailing | MessageTypes.Text },

                new MessageModel { Body = "이새기가", Sender = "p", MessageTypes = MessageTypes.Outgoing | MessageTypes.Leading | MessageTypes.Trailing | MessageTypes.Text, UnreadCount = 0 },

                new MessageModel { Body = "iz*one이 들어왔습니다.👋", Sender = "system", MessageTypes = MessageTypes.EntranceNotice },


                new MessageModel
                {
                    Body = "https://pbs.twimg.com/media/Dni8buZVAAAZwwG.jpg:large",
                    Sender = "iz*one",
                    MessageTypes = MessageTypes.Incomming | MessageTypes.Leading | MessageTypes.Image
                },

                new MessageModel
                {
                    Body = "https://i.pinimg.com/474x/03/bb/53/03bb53ce02e277befc8fa0353da31748.jpg",
                    Sender = "iz*one",
                    MessageTypes = MessageTypes.Incomming | MessageTypes.Trailing | MessageTypes.Image
                },

                new MessageModel
                {
                    Body = "https://ringerstoragekr.blob.core.windows.net/ringer/test.mp4",
                    Sender = "video",
                    MessageTypes = MessageTypes.Incomming | MessageTypes.Leading | MessageTypes.Trailing | MessageTypes.Video,
                    UnreadCount = 1
                },

                new MessageModel
                {
                    Body = "https://ringerstoragekr.blob.core.windows.net/ringer/android.mp4",
                    Sender = "video",
                    MessageTypes = MessageTypes.Outgoing | MessageTypes.Leading | MessageTypes.Trailing | MessageTypes.Video,
                    UnreadCount = 1
                },

                new MessageModel
                {
                    Body = "https://ringerstoragekr.blob.core.windows.net/ringer/BigBuckBunny.mp4",
                    Sender = "video",
                    MessageTypes = MessageTypes.Outgoing | MessageTypes.Leading | MessageTypes.Trailing | MessageTypes.Video,
                    UnreadCount = 1
                },

                new MessageModel
                {
                    Body = "https://i.pinimg.com/474x/03/bb/53/03bb53ce02e277befc8fa0353da31748.jpg",
                    Sender = "wizone",
                    MessageTypes = MessageTypes.Outgoing | MessageTypes.Trailing | MessageTypes.Image,
                    UnreadCount = 1
                },

                new MessageModel
                {
                    Body = "아이즈원 라비앙로즈",
                    Sender = "p",
                    MessageTypes = MessageTypes.Outgoing | MessageTypes.Leading | MessageTypes.Trailing| MessageTypes.Text,
                    UnreadCount = 2

                },


            };

            return Task.FromResult(messageModels);
        }
    }
}
