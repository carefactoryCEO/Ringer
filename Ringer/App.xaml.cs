using Xamarin.Forms;
using Ringer.Core;
using System.Diagnostics;
using System;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Push;
using Ringer.Helpers;
using Xamarin.Essentials;
using Ringer.Models;
using System.Runtime.CompilerServices;
using Ringer.Services;
using Ringer.Data;
using System.Threading.Tasks;
using Ringer.Views;
using System.Collections.Generic;
using Ringer.Core.EventArgs;

namespace Ringer
{
    public partial class App : Application
    {
        #region private members
        private IMessageRepository _messageRepository;
        private IRESTService _restService;
        private static RingerDatabase _database;
        #endregion

        #region public static propertie
        public static bool IsChatPage => Shell.Current.CurrentState.Location.ToString().Contains("chatpage");
        public static RingerDatabase Database => _database ?? new RingerDatabase(Constants.DbPath);
        public static bool IsLoggedIn => Token != null && DeviceId != null && UserName != null && CurrentRoomId != null;
        public static string DeviceId // Appcenter에서 받음
        {
            get => Preferences.Get(nameof(DeviceId), null);
            set => Preferences.Set(nameof(DeviceId), value);
        }
        public static string Token // 로그인 후에 받음
        {
            get => Preferences.Get(nameof(Token), null);
            set => Preferences.Set(nameof(Token), value);
        }
        public static string UserName // 로그인 과정 중에 받음
        {
            get => Preferences.Get(nameof(UserName), null);
            set => Preferences.Set(nameof(UserName), value);
        }
        public static string CurrentRoomId
        {
            get => Preferences.Get(nameof(CurrentRoomId), null);
            set => Preferences.Set(nameof(CurrentRoomId), value);
        }

        #endregion

        #region Constructor
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();

            #region Register messagingService
            DependencyService.Register<MessagingService>();
            DependencyService.Register<IMessageRepository, MessageRepository>();
            DependencyService.Register<IRESTService, RESTService>();

            _restService = DependencyService.Resolve<IRESTService>();
            var messagingService = DependencyService.Resolve<MessagingService>();
            _messageRepository = DependencyService.Resolve<IMessageRepository>();

            messagingService.Connecting += (s, e) => Trace(e.Message);
            messagingService.Connected += (s, e) => Trace(e.Message);
            messagingService.ConnectionFailed += (s, e) => Trace(e.Message, true);

            messagingService.Disconnecting += (s, e) => Trace(e.Message);
            messagingService.Disconnected += (s, e) => Trace(e.Message);
            messagingService.DisconnectionFailed += (s, e) => Trace(e.Message, true);

            messagingService.Closed += (s, e) => Trace(e.Message);
            messagingService.Reconnecting += (s, e) => Trace(e.Message);
            messagingService.Reconnected += (s, e) => Trace(e.Message, true);

            messagingService.MessageReceived += MessageReceived;
            messagingService.SomeoneEntered += SomeoneEntered;
            messagingService.SomeoneLeft += SomeoneLeft;

            #endregion

            PageDisappearing += async (s, page) =>
            {
                if (page is ChatPage)
                {
                    await _restService.ReportDeviceStatusAsync(false);
                }
            };

            PageAppearing += App_PageAppearing;
        }

        private async void App_PageAppearing(object sender, Page page)
        {
            if (!(page is ChatPage))
                return;

            await _restService.ReportDeviceStatusAsync(true);
        }
        #endregion

        #region messaging handlers
        public static void Trace(string message = "", bool analyticsAlso = false, [CallerMemberName] string callerName = "")
        {
            message = $"\n[{DateTime.Now.ToString("yy-MM-dd HH:mm:ss")}]{callerName}: {message}";

            Debug.WriteLine(message);

            if (analyticsAlso)
                Analytics.TrackEvent(message);
        }
        private void SomeoneLeft(object sender, Core.EventArgs.SignalREventArgs e)
        {
            if (e.Sender != UserName)
                _messageRepository.AddLocalMessage(new Message { Body = e.Message, Sender = "system" });

            Trace(e.Message);
        }
        private void SomeoneEntered(object sender, Core.EventArgs.SignalREventArgs e)
        {
            if (e.Sender != UserName)
                _messageRepository.AddLocalMessage(new Message { Body = e.Message, Sender = "system" });

            Trace(e.Message);
        }
        private async void MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            var name = e.SenderName == UserName ? string.Empty : $"{e.SenderName}: ";

            await _messageRepository.AddMessageAsync(new Message
            {
                Id = e.MessageId,
                SenderId = e.SenderId,
                CreatedAt = e.CreatedAt,
                Body = $"{name}{e.Body}",
                Sender = e.SenderName
            });

            // 
            //Preferences.Set(CurrentRoomId, e.MessageId);

            Debug.WriteLine(Preferences.Get(CurrentRoomId, 0));

            Trace(e.Body);
        }
        #endregion

        #region Life Cycle Methods
        protected override async void OnStart()
        {
            Debug.WriteLine("App.OnStart");
            base.OnStart();

            if (DesignMode.IsDesignModeEnabled)
                return;

            #region AppCenter
            // Intercept Push Notification
            if (!AppCenter.Configured)
            {
                Push.PushNotificationReceived += async (sender, e) =>
                {
                    string body = null;
                    string pushSender = null;
                    // If there is custom data associated with the notification,
                    // print the entries
                    if (e.CustomData != null)
                    {
                        foreach (var key in e.CustomData.Keys)
                        {
                            switch (key)
                            {
                                case "room":
                                    CurrentRoomId = e.CustomData[key];
                                    break;

                                case "body":
                                    body = e.CustomData[key];
                                    break;

                                case "sender":
                                    pushSender = e.CustomData[key];
                                    break;

                                default:
                                    break;
                            }
                        }
                    }

                    _messageRepository.AddLocalMessage(new Message { Body = pushSender + ": fdhdhdhf" + body, Sender = pushSender });

                    if (CurrentRoomId != null && !Shell.Current.CurrentState.Location.ToString().Contains("chatpage"))
                    {
                        await Shell.Current.Navigation.PopToRootAsync(false);
                        await Shell.Current.GoToAsync($"chatpage?room={CurrentRoomId}");
                    }
                };
            }

            AppCenter.Start(Constants.AppCenterAndroid + Constants.AppCenteriOS, typeof(Analytics), typeof(Crashes), typeof(Push));

            Analytics.TrackEvent("Ringer started");

            if (await Push.IsEnabledAsync())
            {
                Guid? id = await AppCenter.GetInstallIdAsync();
                Debug.WriteLine("-------------------------");
                Debug.WriteLine(id);
                Debug.WriteLine("-------------------------");

                // Set Device Id
                DeviceId = id?.ToString();
            }
            #endregion

            #region Message

            // DB에 있는 건 messages에 추가하고
            await _messageRepository.LoadMessagesAsync();

            // 서버에 있는 건 가져온다.
            if (CurrentRoomId == null)
                return;

            var lastMessage = await Database.GetLastMessage(CurrentRoomId);
            int lastIndex = lastMessage?.Id ?? 0;


            var pendingMessages = await _restService.PullPendingMessages(CurrentRoomId, lastIndex);
            foreach (var pm in pendingMessages)
                await _messageRepository.AddMessageAsync(new Message
                {
                    Id = pm.Id,
                    Body = pm.Body,
                    Sender = pm.SenderName,
                    SenderId = pm.SenderId,
                    CreatedAt = pm.CreatedAt
                });

            await _restService.ReportDeviceStatusAsync();

            #endregion

        }

        protected override async void OnSleep()
        {
            Debug.WriteLine("OnSleep");

            // TODO Study why Task.Run(async () => await SomethingAsync()); fix the "A Tast was canceled" exception.
            await Task.Run(async () => await _restService.ReportDeviceStatusAsync());

            //try
            //{
            //    await _restService.ReportDeviceStatusAsync();
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine(ex.Message);
            //}
        }

        protected override async void OnResume()
        {
            Debug.WriteLine("OnResume");
            base.OnResume();

            if (IsChatPage)
                await _restService.ReportDeviceStatusAsync(true);

            if (CurrentRoomId == null)
                return;

            var lastMessage = await Database.GetLastMessage(CurrentRoomId);
            int lastIndex = lastMessage?.Id ?? 0;

            var pendingMessages = await _restService.PullPendingMessages(CurrentRoomId, lastIndex);
            foreach (var pm in pendingMessages)
                await _messageRepository.AddMessageAsync(new Message
                {
                    Id = pm.Id,
                    Body = pm.Body,
                    Sender = pm.SenderName,
                    SenderId = pm.SenderId,
                    CreatedAt = pm.CreatedAt
                });


        }
        #endregion
    }
}
