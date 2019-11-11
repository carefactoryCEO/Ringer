using Xamarin.Forms;
using Ringer.Core;
using Ringer.Models;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System;
using System.Threading.Tasks;

namespace Ringer
{
    public partial class App : Application
    {
        #region public static members
        public static readonly string User = "김영미";
        public static readonly string Group = "Xamarin";
        public static readonly string ChatURL = "ringerchat.azurewebsites.net";
        #endregion

        #region Public Properties
        public SignalRService SignalR { get; }
        public ObservableCollection<Message> Messages { get; }
        public bool IsSignalRConnected { get; set; } = false;
        public bool IsProcessingSignalR { get; set; } = false;
        #endregion

        #region Constructor
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();

            Messages = new ObservableCollection<Message>
            {
                new Message
                {
                    Text = "dummy",
                    User = "dummy"
                }
            };

            #region Prepare SignalR
            SignalR = new SignalRService();
            SignalR.Init(urlRoot: ChatURL, useHttps: true);

            // Connection events
            SignalR.Closed += (s, e) => AddMessage(e.Message, e.User);
            SignalR.Reconnecting += (s, e) => AddMessage(e.Message, e.User);
            SignalR.Reconnected += (s, e) => AddMessage(e.Message, e.User);
            
            // Message events
            SignalR.OnEntered += (s, e) => AddMessage(e.Message, e.User);
            SignalR.OnLeft += (s, e) => AddMessage(e.Message, e.User);
            SignalR.OnReceivedMessage += (s, e) => AddMessage(e.Message, e.User);
            #endregion
        }
        #endregion

        #region Life Cycle Methods
        protected override async void OnStart()
        {
            base.OnStart();

            Debug.WriteLine("App.OnStart");

            await ConnectSignalRAsync();
        }
        protected override async void OnSleep()
        {
            Debug.WriteLine("OnSleep");

            await DisconnectSignalRAsync();
            
            base.OnSleep();
        }
        protected override async void OnResume()
        {
            Debug.WriteLine("OnResume");

            await ConnectSignalRAsync();

            base.OnResume();
        }
        #endregion

        #region Private Methods        
        private async Task ConnectSignalRAsync()
        {
            if (IsSignalRConnected || DesignMode.IsDesignModeEnabled)
                return;

            try
            {
                IsProcessingSignalR = true;

                await SignalR.ConnectAsync();
                AddMessage($"App:Connected! {DateTime.Now}\n{SignalR.HubConnection.ConnectionId}", string.Empty);
                
                await SignalR.JoinChannelAsync(Group, User);


                IsSignalRConnected = true;
            }
            catch (Exception ex)
            {
                AddMessage($"App:Connection error: {ex.Message}", User);
            }
            finally
            {
                IsProcessingSignalR = false;
            }
        }        
        private async Task DisconnectSignalRAsync()
        {
            if (!IsSignalRConnected || DesignMode.IsDesignModeEnabled)
                return;

            try
            {
                IsProcessingSignalR = true;

                await SignalR.LeaveChannelAsync(Group, User);

                await SignalR.DisconnectAsync();
                AddMessage(message: $"App:Disconnected...{DateTime.Now}", user: string.Empty);
            

                IsSignalRConnected = false;
            }
            catch (Exception ex)
            {
                AddMessage($"App:Discsonnection error: {ex.Message}", User);
            }
            finally
            {
                IsProcessingSignalR = false;
            }

        }
        public void AddMessage(string message, string user)
        {
            Messages.Insert(0, new Message
            {
                Text = message,
                User = user
            });
        }
        #endregion
    }
}
