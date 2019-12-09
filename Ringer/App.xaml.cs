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
using Ringer.Views;
using Ringer.Core.EventArgs;

namespace Ringer
{
    public partial class App : Application
    {
        #region private members
        private LocalDbService _localDbService;
        private IMessageRepository _messageRepository;
        private IRESTService _restService;
        private MessagingService _messagingService;
        #endregion

        #region public static propertie
        public static bool IsChatPage => Shell.Current.CurrentState.Location.ToString().EndsWith("chatpage", StringComparison.CurrentCultureIgnoreCase);
        public static bool IsLoggedIn => Token != null && DeviceId != null && UserName != null && CurrentRoomId != null;
        public static bool DeviceIsOn
        {
            get => Preferences.Get(nameof(DeviceIsOn), false);
            set => Preferences.Set(nameof(DeviceIsOn), value);
        }
        public static int LastMessageId
        {
            get => Preferences.Get(nameof(LastMessageId), 0);
            set => Preferences.Set(nameof(LastMessageId), value);
        }
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
            DependencyService.Register<LocalDbService>();
            DependencyService.Register<IMessageRepository, MessageRepository>();
            DependencyService.Register<IRESTService, RESTService>();

            _localDbService = DependencyService.Resolve<LocalDbService>();
            _restService = DependencyService.Resolve<IRESTService>();
            _messagingService = DependencyService.Resolve<MessagingService>();
            _messageRepository = DependencyService.Resolve<IMessageRepository>();

            _messagingService.Connecting += Trace_ConnectionStatus;
            _messagingService.Connected += Trace_ConnectionStatus;
            _messagingService.ConnectionFailed += Trace_ConnectionStatus;

            _messagingService.Disconnecting += Trace_ConnectionStatus;
            _messagingService.Disconnected += MessagingService_Disconnected;
            _messagingService.DisconnectionFailed += Trace_ConnectionStatus;

            _messagingService.Closed += Trace_ConnectionStatus;
            _messagingService.Reconnecting += Trace_ConnectionStatus;
            _messagingService.Reconnected += Trace_ConnectionStatus;

            _messagingService.MessageReceived += MessageReceived;
            _messagingService.SomeoneEntered += SomeoneEntered;
            _messagingService.SomeoneLeft += SomeoneLeft;

            #endregion

            PageDisappearing += App_PageDisappearing;
            PageAppearing += App_PageAppearing;
        }

        #endregion

        #region private methods
        private void Trace_ConnectionStatus(object sender, ConnectionEventArgs e)
        {
            //_messageRepository.AddLocalMessage(new Message { Body = $"{DateTime.UtcNow}\n{e.Message}", Sender = Constants.System });

            Trace(e.Message);
        }

        private void MessagingService_Disconnected(object sender, ConnectionEventArgs e)
        {
            Trace_ConnectionStatus(sender, e);
            _restService.ReportDeviceStatus(false);
        }
        private void App_PageAppearing(object sender, Page page)
        {
            if (page is ChatPage)
                _restService.ReportDeviceStatus(true);

        }
        private void App_PageDisappearing(object sender, Page page)
        {
            if (page is ChatPage)
                _restService.ReportDeviceStatus(false);

        }
        #endregion

        #region messaging handlers
        public static void Trace(string message = "", bool analyticsAlso = false, [CallerMemberName] string callerName = "")
        {
            message = $"\n[{DateTime.UtcNow.ToString("yy-MM-dd HH:mm:ss")}]{callerName}: {message}";

            Debug.WriteLine(message);

            if (analyticsAlso)
                Analytics.TrackEvent(message);
        }
        private void SomeoneLeft(object sender, SignalREventArgs e)
        {
            if (e.Sender != UserName)
                _messageRepository.AddLocalMessage(new Message { Body = e.Message, Sender = Constants.System });

            Trace(e.Message);
        }
        private void SomeoneEntered(object sender, SignalREventArgs e)
        {
            if (e.Sender != UserName)
                _messageRepository.AddLocalMessage(new Message { Body = e.Message, Sender = Constants.System });

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

                    if (CurrentRoomId != null && !IsChatPage)
                    {
                        await Shell.Current.Navigation.PopToRootAsync(false);
                        await Shell.Current.GoToAsync($"//mappage/chatpage?room={CurrentRoomId}", false);
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

                Debug.WriteLine($"device id: {DeviceId}");
            }
            #endregion

            _restService.ReportDeviceStatus(false);

            #region Connect and load messages
            if (IsLoggedIn)
            {
                await _messageRepository.LoadMessagesAsync();

                _messagingService.Init(Constants.HubUrl, Token);
                await _messagingService.ConnectAsync();
            }
            #endregion
            base.OnStart();
        }
        protected override void OnSleep()
        {
            Debug.WriteLine($"{DateTime.Now.Millisecond}:OnSleep");
            _restService.ReportDeviceStatus(false);
            base.OnSleep();
        }
        protected override void OnResume()
        {
            //try
            //{
            Debug.WriteLine($"{DateTime.Now.Millisecond}:OnResume");

            _restService.ReportDeviceStatus(IsChatPage);

            if (IsLoggedIn)
            {
                _messageRepository.LoadMessagesAsync();

                if (!_messagingService.IsReconnecting)
                    _messagingService.ConnectAsync();

            }
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine(ex.Message);
            //}
        }
        #endregion
    }
}
