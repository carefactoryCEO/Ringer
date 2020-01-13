using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
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
            SendCommand = new Command(() => ExcuteSendCommand());
            GoBackCommand = new Command(async () => await ExcuteGoBackCommand());
            OpenSessionsPageCommand = new Command(async () => await ExcuteOpenSessionsPageCommand());
            OpenProfilePageCommand = new Command(async () => await ExcuteOpenProfilePageCommand());
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

        private void ExcuteSendCommand()
        {
            var message = new MessageModel { Body = TextToSend, Sender = "", UnreadCount = 2, MessageTypes = MessageTypes.Text | MessageTypes.Outgoing | MessageTypes.Trailing };
            Messages.Add(message);
            TextToSend = string.Empty;
            MessagingCenter.Send<ChatPageViewModel, MessageModel>(this, "MessageAdded", message);
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
}
