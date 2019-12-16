using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using RingerStaff.Models;
using RingerStaff.Types;
using Xamarin.Forms;

namespace RingerStaff.ViewModels
{
    public class ChatPageViewModel : BaseViewModel
    {
        private ObservableCollection<MessageModel> messages;
        private string textToSend;
        private bool addMore = true;
        private int count = 1;
        private MessageModel lastMessage;

        public ChatPageViewModel()
        {
            Title = "신모범 44M 미국";

            messages = new ObservableCollection<MessageModel>
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
                    //Body = "https://clips.vorwaerts-gmbh.de/big_buck_bunny.mp4",
                    // https://ringerstoragekr.blob.core.windows.net/ringer/test.mp4
                    Body = "https://ringerstoragekr.blob.core.windows.net/ringer/test.mp4",
                    Sender = "video",
                    MessageTypes = MessageTypes.Incomming | MessageTypes.Leading | MessageTypes.Trailing | MessageTypes.Video,
                    UnreadCount = 1
                },

                //new MessageModel
                //{
                //    //Body = "https://clips.vorwaerts-gmbh.de/big_buck_bunny.mp4",
                //    //      https://ringerstoragekr.blob.core.windows.net/ringer/test2.MOV
                //    Body = "https://ringerstoragekr.blob.core.windows.net/ringer/test2.MOV",
                //    Sender = "video",
                //    MessageTypes = MessageTypes.Outgoing | MessageTypes.Leading | MessageTypes.Trailing | MessageTypes.Video,
                //    UnreadCount = 1
                //},

                new MessageModel
                {
                    //Body = "https://clips.vorwaerts-gmbh.de/big_buck_bunny.mp4",
                    //      https://ringerstoragekr.blob.core.windows.net/ringer/test2.MOV
                    Body = "https://ringerstoragekr.blob.core.windows.net/ringer/android.mp4",
                    Sender = "video",
                    MessageTypes = MessageTypes.Outgoing | MessageTypes.Leading | MessageTypes.Trailing | MessageTypes.Video,
                    UnreadCount = 1
                },

                new MessageModel
                {
                    //Body = "https://clips.vorwaerts-gmbh.de/big_buck_bunny.mp4",
                    //      https://ringerstoragekr.blob.core.windows.net/ringer/test2.MOV
                    Body = "https://clips.vorwaerts-gmbh.de/big_buck_bunny.mp4",
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

            };

            //Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            //{
            //    var message = new MessageModel { Body = (count++).ToString(), Sender = "Timer", MessageTypes = MessageTypes.Outgoing | MessageTypes.Text, UnreadCount = 2 };

            //    if (count == 2)
            //    {
            //        message.MessageTypes |= MessageTypes.Leading;
            //    }

            //    message.MessageTypes |= MessageTypes.Trailing;

            //    if (lastMessage != null)
            //    {
            //        lastMessage.MessageTypes &= ~MessageTypes.Trailing;
            //        lastMessage.UnreadCount -= 1;

            //    }

            //    lastMessage = message;

            //    Messages.Add(message);

            //    MessagingCenter.Send<ChatPageViewModel, MessageModel>(this, "MessageAdded", message);

            //    addMore = (count >= 6) ? false : true;
            //    return addMore;
            //});

            StopCommand = new Command(() => addMore = !addMore);
            MessageTappedCommand = new Command<MessageModel>(messageModel => Debug.WriteLine($"{messageModel.Body} tapped"));
        }

        public string TextToSend { get => textToSend; set => SetProperty(ref textToSend, value); }
        public ObservableCollection<MessageModel> Messages { get => messages; set => SetProperty(ref messages, value); }

        public ICommand SendCommand { get; set; }
        public ICommand StopCommand { get; set; }
        public ICommand MessageTappedCommand { get; set; }
    }
}
