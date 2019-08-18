using Ringer.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using Xamarin.Forms;

namespace Ringer.ViewModels
{
    class ChatPageViewModel : INotifyPropertyChanged
    {
        public bool ShowScrollTap { get; set; } = false;
        public bool LastMessageVisible { get; set; } = true;
        public int PendingMessageCount { get; set; } = 0;
        public bool PendingMessageCountVisible { get { return PendingMessageCount > 0; } }

        public Queue<Message> DelayedMessages { get; set; } = new Queue<Message>();
        public ObservableCollection<Message> Messages { get; set; } = new ObservableCollection<Message>();
        public string TextToSend { get; set; }
        public ICommand OnSendCommand { get; set; }
        public ICommand MessageAppearingCommand { get; set; }
        public ICommand MessageDisappearingCommand { get; set; }

        public ChatPageViewModel()
        {
            Messages.Insert(0, new Message() { Text = $"안녕하세요 링거입니다." });

            Messages.Insert(0, new Message() { Text = $"제가 사이판에 있는데 애기가 아파서요", User = App.User });
            Messages.Insert(0, new Message() { Text = $"어제부터 음식도 못 먹고 열이 났는데\n어제 저녁부터는 토하더니", User = App.User });
            Messages.Insert(0, new Message() { Text = $"이제는 배가 아프다고 하는데", User = App.User });

            Messages.Insert(0, new Message() { Text = $"사이판에 있으시고.. 아이가 많이 어린가요?" });

            Messages.Insert(0, new Message() { Text = $"6살요", User = App.User });
            Messages.Insert(0, new Message() { Text = $"해열제도 먹으면 토해서 못 먹이는데", User = App.User });

            Messages.Insert(0, new Message() { Text = $"배 아픈 양상이 어떠한가요? 지속적으로 아파하나요? 간간히 아파하나요? 아픈 위치는 배꼽 위인가요 아래인가요?", User = App.User });

            Messages.Insert(0, new Message() { Text = $"배꼽쯤요", User = App.User });

            Messages.Insert(0, new Message() { Text = $"그렇군요" });

            Messages.Insert(0, new Message() { Text = $"상담이 종료되었습니다." });
            Messages.Insert(0, new Message() { Text = @"<상담요약>
CC> abdomen pain(3 hour ago)
S & O> fever/chill -/-
        nausea/vomiting/diarrhea +/-/-
        epigastric pain +, burning sense
        other symptom:dny
        PMHx: HTN/DM -/+ Op Hx -
        Medication: metformin
        Drug allergy: none
A>r/o acute gastritis
  r/o GERD
P>위장약(큐란)과 타이레놀을 복용하세요.증상이 악화되거나 열이 나거나 호전 없으면 병원 진료를 고려하세요." });


            MessageAppearingCommand = new Command<Message>(OnMessageAppearing);
            MessageDisappearingCommand = new Command<Message>(OnMessageDisappearing);

            OnSendCommand = new Command(() =>
            {
                if (!string.IsNullOrEmpty(TextToSend))
                {
                    Messages.Insert(0, new Message() { Text = TextToSend, User = App.User });
                    TextToSend = string.Empty;
                }

            });

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

        }

        private int dummyCount;

        void OnMessageAppearing(Message message)
        {
            var idx = Messages.IndexOf(message);

            Debug.WriteLine($"{idx}/{Messages.Count} appearing : {message.Text}");

            if (idx <= 5)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    while (DelayedMessages.Count > 0)
                    {
                        Messages.Insert(0, DelayedMessages.Dequeue());
                    }
                    ShowScrollTap = false;
                    LastMessageVisible = true;
                    PendingMessageCount = 0;
                });
            }
        }

        void OnMessageDisappearing(Message message)
        {
            var idx = Messages.IndexOf(message);


            Debug.WriteLine($"\t{idx}/{Messages.Count} disppearing : {message.Text}");

            if (idx >= 5)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    ShowScrollTap = true;
                    LastMessageVisible = false;
                });


            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
