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
using Ringer.Types;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ringer
{
    public partial class App : Application
    {
        #region private members
        private IMessageRepository _messageRepository;
        private ILocalDbService _localDbService;
        private IMessagingService _messagingService;
        #endregion

        #region public static propertie
        public static bool IsChatPage => Shell.Current.CurrentState.Location.ToString().EndsWith("chatpage", StringComparison.CurrentCultureIgnoreCase);
        public static bool IsLoggedIn => Token != null && DeviceId != null && UserName != null && RoomId != null;
        public static bool DeviceIsOn
        {
            get => Preferences.Get(nameof(DeviceIsOn), false);
            set => Preferences.Set(nameof(DeviceIsOn), value);
        }
        public static int LastServerMessageId
        {
            get => Preferences.Get(nameof(LastServerMessageId), 0);
            set => Preferences.Set(nameof(LastServerMessageId), value);
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
        public static int UserId
        {
            get => Preferences.Get(nameof(UserId), -1);
            set => Preferences.Set(nameof(UserId), value);
        }
        public static string RoomId
        {
            get => Preferences.Get(nameof(RoomId), null);
            set => Preferences.Set(nameof(RoomId), value);
        }
        #endregion

        #region Constructor
        public App()
        {
            //Xamarin.Forms.Device.SetFlags(new string[] { "MediaElement_Experimental" });

            InitializeComponent();

            MainPage = new AppShell();

            #region Register messagingService
            DependencyService.Register<IMessagingService, MessagingService>();
            DependencyService.Register<ILocalDbService, LocalDbService>();
            DependencyService.Register<IMessageRepository, MessageRepository>();
            DependencyService.Register<IRESTService, RESTService>();

            _messagingService = DependencyService.Resolve<IMessagingService>();
            _messageRepository = DependencyService.Resolve<IMessageRepository>();
            _localDbService = DependencyService.Resolve<ILocalDbService>();

            _messagingService.Connecting += Trace_ConnectionStatus;
            _messagingService.Connected += Trace_ConnectionStatus;
            _messagingService.ConnectionFailed += ConnectionFailed; ;

            _messagingService.Disconnecting += Trace_ConnectionStatus;
            //_messagingService.Disconnected += Trace_ConnectionStatus;
            _messagingService.Disconnected += ConnectionFailed;
            _messagingService.DisconnectionFailed += Trace_ConnectionStatus;

            //_messagingService.Closed += Trace_ConnectionStatus;
            _messagingService.Closed += ConnectionFailed;
            _messagingService.Reconnecting += Trace_ConnectionStatus;
            _messagingService.Reconnected += Trace_ConnectionStatus;

            _messagingService.MessageReceived += MessageReceived;
            _messagingService.SomeoneEntered += SomeoneEntered;
            _messagingService.SomeoneLeft += SomeoneLeft;

            #endregion

            //PageDisappearing += App_PageDisappearing;
            //PageAppearing += App_PageAppearing;
        }

        private void ConnectionFailed(object sender, ConnectionEventArgs e)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
            {
                Shell.Current.DisplayAlert("이럴수가", e.Message, "닫기");
            });
        }
        #endregion

        #region public static methods
        public static void Trace(string message = "", bool analyticsAlso = false, [CallerMemberName] string callerName = "")
        {
            message = $"\n[{DateTime.UtcNow.ToString("yy-MM-dd HH:mm:ss")}]{callerName}: {message}";

            Debug.WriteLine(message);

            if (analyticsAlso)
                Analytics.TrackEvent(message);
        }
        #endregion

        #region messaging handlers
        private void Trace_ConnectionStatus(object sender, ConnectionEventArgs e)
        {
            Trace(e.Message);
        }
        private void SomeoneLeft(object sender, SignalREventArgs e)
        {
            if (e.Sender != UserName)
                _messageRepository.AddLocalMessage(new MessageModel { Body = $"{e.Sender}님이 나갔습니다.", Sender = Constants.System, MessageTypes = MessageTypes.EntranceNotice });

            Trace(e.Message);
        }
        private void SomeoneEntered(object sender, SignalREventArgs e)
        {
            if (e.Sender != UserName)
                _messageRepository.AddLocalMessage(new MessageModel { Body = $"{e.Sender}님이 들어왔습니다.", Sender = Constants.System, MessageTypes = MessageTypes.EntranceNotice });

            Trace(e.Message);
        }
        private async void MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            MessageTypes messageTypes = Utilities.SetMessageTypes(e.Body, e.SenderId, UserId);

            var message = new MessageModel
            {
                ServerId = e.MessageId,
                RoomId = RoomId,
                Body = e.Body,
                Sender = e.SenderName,
                SenderId = e.SenderId,
                CreatedAt = e.CreatedAt,
                ReceivedAt = DateTime.UtcNow,
                MessageTypes = messageTypes
            };

            await _messageRepository.AddMessageAsync(message);

            Debug.WriteLine(Preferences.Get(RoomId, 0));

            Trace(message.MessageTypes.ToString());
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
                                    RoomId = e.CustomData[key];
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

                    if (RoomId != null && !IsChatPage)
                    {
                        await Shell.Current.Navigation.PopToRootAsync(false);
                        await Shell.Current.GoToAsync($"//mappage/chatpage?room={RoomId}", false);
                    }
                };
            }

            AppCenter.Start(Constants.AppCenterAndroid + Constants.AppCenteriOS, typeof(Analytics), typeof(Crashes), typeof(Push));

            Analytics.TrackEvent("Ringer started");

            if (await Push.IsEnabledAsync())
            {
                Guid? id = await AppCenter.GetInstallIdAsync();

                Debug.WriteLine("-------------------------");
                Debug.WriteLine($"device id: {id}");
                Debug.WriteLine("-------------------------");

                if (id != null)
                    DeviceId = id?.ToString();
            }
            #endregion

            //await _restService.ReportDeviceStatusDebouncedAsync(false);
            //_restService.ReportDeviceStatus(false);

            #region Connect and load messages
            if (IsLoggedIn)
            {
                await _messageRepository.LoadMessagesAsync().ConfigureAwait(false);

                _messagingService.Init(Constants.HubUrl, Token);
                await _messagingService.ConnectAsync().ConfigureAwait(false);//OnStart
            }
            #endregion
            base.OnStart();
        }
        protected override void OnSleep()
        {
            Debug.WriteLine($"{DateTime.Now.Millisecond}:OnSleep");
            //await _restService.ReportDeviceStatusDebouncedAsync(false);
            //await _restService.ReportDeviceStatusDebouncedAsync(false, 1000);
            //_restService.ReportDeviceStatus(false);
            base.OnSleep();
        }
        protected override async void OnResume()
        {
            //try
            //{
            Debug.WriteLine($"{DateTime.Now.Millisecond}:OnResume");

            //_restService.ReportDeviceStatus(IsChatPage);
            //await _restService.ReportDeviceStatusDebouncedAsync(IsChatPage);

            if (IsLoggedIn)
            {
                await _messageRepository.LoadMessagesAsync().ConfigureAwait(false);

                if (!_messagingService.IsReconnecting)
                    await _messagingService.ConnectAsync().ConfigureAwait(false);//OnResume

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
