using Ringer.Core;
using Ringer.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Ringer.ViewModels
{
    class ChatPageViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Message> Messages { get; set; } = new ObservableCollection<Message>();
        public string TextToSend { get; set; }

        private readonly ChatService signalR;
        public bool IsConnected = false;
        public bool IsBusy { get; set; } = false;

        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand SendMessageCommand { get; }


        public ChatPageViewModel()
        {
            if (DesignMode.IsDesignModeEnabled)
                return;

            IsConnected = false;

            Messages.Add(new Message { Text = "dummy", User = "dummy" });
            

            SendMessageCommand = new Command(async () => await SendMessage());
            ConnectCommand     = new Command(async () => await Connect());
            DisconnectCommand  = new Command(async () => await Disconnect());

            signalR = DependencyService.Resolve<ChatService>();
            signalR.Init(urlRoot: App.ChatURL, useHttps: true);

            signalR.OnConnectionClosed += (s, e) => SendLocalMessage(e.Message, e.User);
            signalR.OnReceivedMessage  += (s, e) => SendLocalMessage(e.Message, e.User);
            signalR.OnReconnected += async (s, e) =>
            {
                SendLocalMessage(e.Message, e.User);
                await signalR.JoinChannelAsync(App.Group, App.User);
            };
        }

        async Task Connect()
        {
            if (IsConnected)
                return;
            try
            {
                IsBusy = true;

                App.Repository.ForEach(m => Messages.Add(m));
                App.Repository.Clear();

                await signalR.ConnectAsync();
                await signalR.JoinChannelAsync(App.Group, App.User);

                SendLocalMessage($"vm.Connet:Connected! {DateTime.Now}", string.Empty);

                IsConnected = true;
                //await Task.Delay(200); // why? 보여주려고??
                //SendLocalMessage("Connected...", App.User);
            }
            catch (Exception ex)
            {
                SendLocalMessage($"vm.Connect:Connection error: {ex.Message}", App.User);
            }
            finally
            {
                IsBusy = false;
            }
        }

        async Task SendMessage()
        {
            if (string.IsNullOrEmpty(TextToSend))
                return;

            if (!IsConnected)
            {
                await Shell.Current.DisplayAlert("Not connected", "Please connect to the server and try again.", "OK");
                return;
            }
            try
            {
                IsBusy = true;
                await signalR.SendMessageAsync(
                    App.Group,
                    App.User,
                    TextToSend);      
            }
            catch (Exception ex)
            {
                SendLocalMessage($"vs.SendMessage:Send failed: {ex.Message}", App.User);
            }
            finally
            {

                
                IsBusy = false;
            }
        }

        async Task Disconnect()
        {
            if (!IsConnected)
                return;

            //SendLocalMessage($"Disconnected...{DateTime.UtcNow}", string.Empty);

            Messages.Insert(0, new Message
            {
                Text = $"vm.Disconnect:Disconnected...{DateTime.Now}",
                User = string.Empty
            });
            App.Repository.AddRange(Messages);

            await signalR.LeaveChannelAsync(App.Group, App.User);
            await signalR.DisconnectAsync();            

            IsConnected = false;
            
        }


        public void SendLocalMessage(string message, string user)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Messages.Insert(0, new Message
                {
                    Text = message,
                    User = user
                });

                TextToSend = string.Empty;

                Console.WriteLine(Messages.Count);
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
