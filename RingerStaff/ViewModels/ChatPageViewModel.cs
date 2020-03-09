using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ringer.Core.EventArgs;
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

        public ChatPageViewModel()
        {
            Title = "신모범 44M 워싱턴(미국)";
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
        }

        private async void RealTimeService_Reconnecting(object sender, ConnectionEventArgs e)
        {
            Debug.WriteLine(e.Message);
            await App.Current.MainPage.DisplayAlert("reconnecting", "reconnecting...", "닫기");
        }

        private void RealTimeService_Reconnected(object sender, ConnectionEventArgs e)
        {
            Debug.WriteLine(e.Message);
        }

        private void RealTimeService_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            MessageModel message = new MessageModel
            {
                Body = e.Body,
                Sender = e.SenderName,
                CreatedAt = e.CreatedAt.ToLocalTime(),
                MessageTypes = MessageTypes.Incomming | MessageTypes.Leading | MessageTypes.Trailing
            };

            // text, image, video (media type)
            message.MessageTypes |= MessageTypes.Text;

            MessageModel lastMessage = Messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();

            if (lastMessage != null)
            {
                if (lastMessage.Sender == message.Sender && InSameMinute(message.CreatedAt, lastMessage.CreatedAt))
                {
                    lastMessage.MessageTypes ^= MessageTypes.Trailing;
                    message.MessageTypes ^= MessageTypes.Leading;
                }
            }

            Messages.Add(message);
            MessagingCenter.Send<ChatPageViewModel, MessageModel>(this, "MessageAdded", message);
        }

        private bool InSameMinute(DateTime current, DateTime last)
        {
            return current - last < TimeSpan.FromMinutes(1) && current.Minute == last.Minute;
        }

        private async Task SendMessageAsync()
        {
            if (TextToSend == string.Empty)
                return;

            var message = new MessageModel
            {
                Body = TextToSend,
                Sender = "",
                UnreadCount = 2,
                MessageTypes = MessageTypes.Outgoing | MessageTypes.Leading | MessageTypes.Trailing,
                CreatedAt = DateTime.UtcNow.ToLocalTime()
            };

            // text, image, video (media type)
            message.MessageTypes |= MessageTypes.Text;

            MessageModel lastMessage = Messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();

            if (lastMessage != null)
            {
                if (lastMessage.Sender == message.Sender && InSameMinute(message.CreatedAt, lastMessage.CreatedAt))
                {
                    lastMessage.MessageTypes ^= MessageTypes.Trailing;
                    message.MessageTypes ^= MessageTypes.Leading;
                }
            }

            Messages.Add(message);
            TextToSend = string.Empty;
            MessagingCenter.Send(this, "MessageAdded", message);

            await RealTimeService.SendMessageAsync(message, App.CurrentRoomId);
        }


        private Task ExcuteOpenProfilePageCommand()
        {
            return Shell.Current.GoToAsync("profilepage");
        }

        private Task ExcuteOpenSessionsPageCommand()
        {
            return Shell.Current.Navigation.PushModalAsync(new SessionsPage());
        }

        internal async Task OnAppearingAsync()
        {
            await RealTimeService.EnterRoomAsync(App.CurrentRoomId, "staff");
        }

        private Task ExcuteGoBackCommand()
        {
            return Shell.Current.Navigation.PopAsync();
        }


        public async Task LoadMessagesAsync()
        {
            IsBusy = true;

            var messageModels = await MessageRepository.LoadMessageAsync();

            Messages.Clear();

            foreach (var messageModel in messageModels)
                Messages.Add(messageModel);

            MessagingCenter.Send<ChatPageViewModel, MessageModel>(this, "MessageAdded", messageModels.Last());

            IsBusy = false;
        }

        public string TextToSend { get => textToSend; set => SetProperty(ref textToSend, value); }
        public ObservableCollection<MessageModel> Messages { get => messages; set => SetProperty(ref messages, value); }
        public double NavBarHeight { get => navBarHeight; set => SetProperty(ref navBarHeight, value); }
        public Thickness BottomPadding { get => bottomPadding; set => SetProperty(ref bottomPadding, value); }


        public ICommand LoadMessagesCommand { get; set; }
        public ICommand SendCommand { get; set; }
        public ICommand StopCommand { get; set; }
        public ICommand MessageTappedCommand { get; set; }
        public ICommand GoBackCommand { get; set; }
        public ICommand OpenSessionsPageCommand { get; set; }
        public ICommand OpenProfilePageCommand { get; set; }

    }

    public static class FlagExtension
    {
        public static MessageTypes Add(this MessageTypes flag, MessageTypes toAdd)
        {
            return flag | toAdd;
        }

        public static MessageTypes Remove(this MessageTypes flag, MessageTypes toAdd)
        {
            var newflag = flag & ~toAdd;
            return newflag;
        }
    }
}
