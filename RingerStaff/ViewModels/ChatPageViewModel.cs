using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Ringer.Core.Data;
using Ringer.Core.EventArgs;
using Ringer.Core.Models;
using RingerStaff.Helpers;
using RingerStaff.Models;
using RingerStaff.Services;
using RingerStaff.Types;
using RingerStaff.Views;
using Xamarin.Forms;

namespace RingerStaff.ViewModels
{
    public class ChatPageViewModel : BaseViewModel
    {
        private ObservableCollection<MessageModel> messages;
        private string textToSend;
        private double navBarHeight;
        private Thickness bottomPadding;
        private bool messagesLoading;

        public string TextToSend { get => textToSend; set => SetProperty(ref textToSend, value); }
        public ObservableCollection<MessageModel> Messages { get => messages; set => SetProperty(ref messages, value); }
        public double NavBarHeight { get => navBarHeight; set => SetProperty(ref navBarHeight, value); }
        public Thickness BottomPadding { get => bottomPadding; set => SetProperty(ref bottomPadding, value); }

        public ChatPageViewModel()
        {
            Title = App.RoomTitle;
            messages = new ObservableCollection<MessageModel>();

            MessageTappedCommand = new Command<MessageModel>(messageModel => Debug.WriteLine($"{messageModel.Body} tapped"));
            LoadMessagesCommand = new Command(async () => await LoadMessagesAsync());
            SendCommand = new Command(async () => await SendMessageAsync());
            GoBackCommand = new Command(async () => await ExcuteGoBackCommand());
            OpenSessionsPageCommand = new Command(async () => await ExcuteOpenSessionsPageCommand());
            OpenProfilePageCommand = new Command(async () => await ExcuteOpenProfilePageCommand());

            RealTimeService.MessageReceived += RealTimeService_MessageReceived;
            RealTimeService.Reconnecting += RealTimeService_Reconnecting;
            RealTimeService.Reconnected += RealTimeService_Reconnected;
            RealTimeService.SomeoneEntered += RealTimeService_SomeoneEntered;
            RealTimeService.SomeoneLeft += RealTimeService_SomeoneLeft;

            messages.CollectionChanged += Messages_CollectionChanged;
        }

        private void RealTimeService_SomeoneLeft(object sender, SignalREventArgs e)
        {
            if (e.Sender == App.UserName)
                return;

            MessageModel message = new MessageModel
            {
                Body = $"{e.Sender}가 나갔습니다.",
                MessageTypes = MessageTypes.EntranceNotice | MessageTypes.Leading | MessageTypes.Trailing
            };

            messages.Add(message);
        }
        private void RealTimeService_SomeoneEntered(object sender, SignalREventArgs e)
        {
            if (e.Sender == App.UserName)
                return;

            MessageModel message = new MessageModel
            {
                Body = $"{e.Sender}가 들어왔습니다.",
                MessageTypes = MessageTypes.EntranceNotice | MessageTypes.Leading | MessageTypes.Trailing

            };

            Messages.Add(message);
        }
        private void RealTimeService_Reconnecting(object sender, ConnectionEventArgs e)
        {
            Debug.WriteLine(e.Message);
            MessagingCenter.Send(this, "ConnectionEvent", e.Message);
        }
        private void RealTimeService_Reconnected(object sender, ConnectionEventArgs e)
        {
            Debug.WriteLine(e.Message);
        }
        private void RealTimeService_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.RoomId != App.RoomId)
                return;

            if (e.SenderId == App.UserId)
                return;

            var lastMessage = Messages.LastOrDefault();
            MessageModel message = new MessageModel
            {
                Body = e.Body,
                Sender = e.SenderName,
                CreatedAt = e.CreatedAt
            };

            Utility.SetMessageTypes(ref message, ref lastMessage, App.UserId);

            Messages.Add(message);
        }

        private async Task SendMessageAsync()
        {
            var body = TextToSend.Trim();

            if (string.IsNullOrEmpty(body))
                return;

            TextToSend = string.Empty;

            var lastMessage = messages.LastOrDefault();
            var message = new MessageModel
            {
                Body = body,
                SenderId = App.UserId,
                Sender = App.UserName,
                CreatedAt = DateTime.UtcNow
            };

            Utility.SetMessageTypes(ref message, ref lastMessage, App.UserId);

            Messages.Add(message);

            await RealTimeService.SendMessageAsync(message, App.RoomId);
        }

        private void Messages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                MessagingCenter.Send(this, "MessageAdded", messages.Last());
            }
        }

        private Task ExcuteOpenProfilePageCommand()
        {
            return Shell.Current.GoToAsync("profilepage");
        }
        private Task ExcuteOpenSessionsPageCommand()
        {
            return Shell.Current.Navigation.PushModalAsync(new SessionsPage());
        }
        private Task ExcuteGoBackCommand()
        {
            return Shell.Current.Navigation.PopAsync();
        }

        public async Task OnAppearingAsync()
        {
            await RealTimeService.EnterRoomAsync(App.RoomId, "staff");
            await LoadMessagesAsync();
        }
        public async Task OnDisappearingAsync()
        {
            await Task.Delay(0);
        }
        public async Task LoadMessagesAsync()
        {
            IsBusy = true;

            Messages.Clear();

            List<PendingMessage> pendingMessages = await ApiService.PullPendingMessagesAsync(App.RoomId, 0, App.Token);

            if (!pendingMessages.Any())
                return;

            MessageModel[] messages = pendingMessages
                .OrderBy(p => p.CreatedAt)
                .TakeLast(50)
                .Select(pm => new MessageModel
                {
                    ServerId = pm.Id,
                    Body = pm.Body,
                    Sender = pm.SenderName,
                    SenderId = pm.SenderId,
                    CreatedAt = pm.CreatedAt,
                    UnreadCount = 0
                })
                .ToArray();

            messagesLoading = true;

            for (int i = 0; i < messages.Length; i++)
            {
                MessageModel lastMessage = i > 0 ? messages[i - 1] : null;
                Utility.SetMessageTypes(ref messages[i], ref lastMessage, App.UserId);
                Messages.Add(messages[i]);
            }

            messagesLoading = false;

            if (Messages.Any())
                MessagingCenter.Send(this, "MessageAdded", Messages.Last());

            IsBusy = false;
        }

        public ICommand LoadMessagesCommand { get; set; }
        public ICommand SendCommand { get; set; }
        public ICommand StopCommand { get; set; }
        public ICommand MessageTappedCommand { get; set; }
        public ICommand GoBackCommand { get; set; }
        public ICommand OpenSessionsPageCommand { get; set; }
        public ICommand OpenProfilePageCommand { get; set; }

    }
}
