using Ringer.Core;
using Ringer.Helpers;
using Ringer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Ringer.ViewModels
{
    class ChatPageViewModel : INotifyPropertyChanged
    {
        // from chatuix
        public ObservableCollection<Message> Messages { get; set; } = new ObservableCollection<Message>(); // 중복
        public string TextToSend { get; set; }        
        public ICommand OnSendCommand { get; }

        // from xamchat
        ChatService signalR;
        public bool IsConnected = false;
        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand SendMessageCommand { get; }


        public ChatPageViewModel()
        {
            if (DesignMode.IsDesignModeEnabled)
                return;

            IsConnected = false;

            SendMessageCommand = new Command(async () => await SendMessage());
            ConnectCommand     = new Command(async () => await Connect());
            DisconnectCommand  = new Command(async () => await Disconnect());

            signalR = DependencyService.Resolve<ChatService>();
            signalR.Init(App.ChatURL, true);

            signalR.OnConnectionClosed += (s, e) => SendLocalMessage(e.Message, e.User);
            signalR.OnReceivedMessage  += (s, e) => SendLocalMessage(e.Message, e.User);

            #region dummin insert

//            Messages.Insert(0, new Message() { Text = $"안녕하세요 링거입니다." });

//            Messages.Insert(0, new Message() { Text = $"제가 사이판에 있는데 애기가 아파서요", User = App.User });
//            Messages.Insert(0, new Message() { Text = $"어제부터 음식도 못 먹고 열이 났는데\n어제 저녁부터는 토하더니", User = App.User });
//            Messages.Insert(0, new Message() { Text = $"이제는 배가 아프다고 하는데", User = App.User });

//            Messages.Insert(0, new Message() { Text = $"사이판에 있으시고.. 아이가 많이 어린가요?" });

//            Messages.Insert(0, new Message() { Text = $"6살요", User = App.User });
//            Messages.Insert(0, new Message() { Text = $"해열제도 먹으면 토해서 못 먹이는데", User = App.User });

//            Messages.Insert(0, new Message() { Text = $"배 아픈 양상이 어떠한가요? 지속적으로 아파하나요? 간간히 아파하나요? 아픈 위치는 배꼽 위인가요 아래인가요?", User = App.User });

//            Messages.Insert(0, new Message() { Text = $"배꼽쯤요", User = App.User });

//            Messages.Insert(0, new Message() { Text = $"그렇군요" });

//            Messages.Insert(0, new Message() { Text = $"상담이 종료되었습니다." });
//            Messages.Insert(0, new Message() { Text = @"<상담요약>
//CC> abdomen pain(3 hour ago)
//S & O> fever/chill -/-
//        nausea/vomiting/diarrhea +/-/-
//        epigastric pain +, burning sense
//        other symptom:dny
//        PMHx: HTN/DM -/+ Op Hx -
//        Medication: metformin
//        Drug allergy: none
//A>r/o acute gastritis
//  r/o GERD
//P>위장약(큐란)과 타이레놀을 복용하세요.증상이 악화되거나 열이 나거나 호전 없으면 병원 진료를 고려하세요." });




            //Code to simulate reveing a new message procces
            //Device.StartTimer(TimeSpan.FromSeconds(5), () =>
            //{
            //    if (LastMessageVisible)
            //    {
            //        Messages.Insert(0, new Message(){ Text = $"New message {++dummyCount}", User="Mario"});
            //    }
            //    else
            //    {
            //        DelayedMessages.Enqueue(new Message() { Text = $"delayed message {++dummyCount}" , User = "Mario"});
            //        PendingMessageCount++;
            //    }

            //    Debug.WriteLine($"------{DateTime.Now}------");
            //    return true;
            //});

            #endregion
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
                IsConnected = true;
                //await Task.Delay(200); // why? 보여주려고??
                //SendLocalMessage("Connected...", App.User);
            }
            catch (Exception ex)
            {
                SendLocalMessage($"Connection error: {ex.Message}", App.User);
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
                SendLocalMessage($"Send failed: {ex.Message}", App.User);
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

            App.Repository.AddRange(Messages);

            await signalR.LeaveChannelAsync(App.Group, App.User);
            await signalR.DisconnectAsync();            

            IsConnected = false;
            SendLocalMessage("Disconnected...", App.User);
        }


        private void SendLocalMessage(string message, string user)
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
