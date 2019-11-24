using Microsoft.AspNetCore.SignalR.Client;
using Ringer.Core.EventArgs;
using Ringer.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Ringer.Core
{
    public class SignalRService
    {
        #region private members
        private string user;
        private string group;
        #endregion

        #region Constructor and Initializer
        public SignalRService()
        {
            ActiveChannels = new Dictionary<string, string>();

            Messages = new ObservableCollection<Message>()
            {
                new Message
                {
                    Text = "dummy",
                    User = "dummy"
                }
            };
        }
        /// <summary>
        /// SignalR Hub 서버와 통신하기 위한 기반을 닦음
        /// 1.HubConnection을 만든다 (연결하는 건 아님)
        /// 2.Hubconnection events을 관리한다.
        /// 3.Hub에서 호출할 Methods를 지정한다.
        /// </summary>
        /// <param name="urlRoot"></param>
        /// <param name="useHttps"></param>
        public void Init(string urlRoot, string user, string group, bool useHttps = false)
        {
            this.user = user;
            this.group = group;



            var port = (urlRoot == "localhost" || urlRoot == "10.0.2.2") ? (useHttps ? ":5001" : ":5000") : string.Empty;
            var url = $"http{(useHttps ? "s" : string.Empty)}://{urlRoot}{port}/hubs/chat";


            //var url = $"https://{urlRoot}/hubs/chat";
            Console.WriteLine("---------------------");
            Console.WriteLine(url);
            Console.WriteLine("---------------------");

            // Build HubConnection
            HubConnection = new HubConnectionBuilder()
                .WithUrl(url)
                .WithAutomaticReconnect() // wait 0,2,10,30 seconds and try to reconnect
                .Build();

            // Handle Hub connection events
            HubConnection.Closed += HubConnection_Closed;
            HubConnection.Reconnecting += HubConnection_Reconnecting;
            HubConnection.Reconnected += HubConnection_Reconnected;

            // Handle Hub messages
            HubConnection.On<string, string>("ReceiveMessage", HandleRecievedMessage);
            HubConnection.On<string>("Entered", HandleEntered);
            HubConnection.On<string>("Left", HandleLeft);
        }
        #endregion

        #region public properties

        public bool IsConnected => HubConnection.State == HubConnectionState.Connected;
        public bool IsConnecting => HubConnection.State == HubConnectionState.Connecting;
        public bool IsDisconnected => HubConnection.State == HubConnectionState.Disconnected;
        public bool IsReconnecting => HubConnection.State == HubConnectionState.Reconnecting;

        public Dictionary<string, string> ActiveChannels { get; }
        public HubConnection HubConnection { get; private set; }
        public ObservableCollection<Message> Messages { get; set; }
        #endregion

        #region Private Mothods
        private Task HubConnection_Closed(Exception err)
        {
            Closed?.Invoke(
                    this,
                    new SignalREventArgs($"HubConnection.Closed event fired\n{err.Message}\n{DateTime.Now}", string.Empty));

            ActiveChannels.Clear();

            return Task.CompletedTask;
        }
        private Task HubConnection_Reconnecting(Exception err)
        {
            Reconnecting?.Invoke(
                   this,
                   new SignalREventArgs($"HubConnection.Reconnecting event fired\n{err.Message}\n{DateTime.Now}", string.Empty));

            ActiveChannels.Clear();

            return Task.CompletedTask;
        }
        private async Task HubConnection_Reconnected(string message)
        {
            Reconnected?.Invoke(
                    this,
                    new SignalREventArgs($"HubConnection.Reconnected event fired\n{message}\n{DateTime.Now}", string.Empty));

            await JoinChannelAsync(group, user);
        }

        private void HandleRecievedMessage(string user, string message)
        {
            OnReceivedMessage?.Invoke(this, new SignalREventArgs(message, user));
        }
        private void HandleEntered(string user)
        {
            OnEntered?.Invoke(this, new SignalREventArgs($"{user} entered.", user));
        }
        private void HandleLeft(string user)
        {
            OnLeft?.Invoke(this, new SignalREventArgs($"{user} left.", user));
        }

        public void AddLocalMessage(string message, string user)
        {
            Messages.Insert(0, new Message
            {
                Text = message,
                User = user
            });
        }
        #endregion        

        #region Public Methods
        public async Task ConnectAsync()
        {
            if (IsConnected)
                return;
            try
            {
                Debug.WriteLine($"Try to Connect\n{DateTime.Now}");
                AddLocalMessage($"Try to Connect\n{DateTime.Now}", string.Empty);

                await HubConnection.StartAsync();

                if (!IsConnected)
                    throw new InvalidOperationException("SignalRService.ConnectAsync() faild");

                Debug.WriteLine($"Connection Successed\n{DateTime.Now}\n{HubConnection.ConnectionId}");
                AddLocalMessage($"Connection Successed\n{DateTime.Now}\n{HubConnection.ConnectionId}", string.Empty);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Connection Failed: " + ex.Message);
                AddLocalMessage("Connection Failed: " + ex.Message, string.Empty);
            }
        }
        public async Task DisconnectAsync()
        {
            if (!IsConnected)
                return;
            try
            {
                Debug.WriteLine($"Try to Disconnect\n{DateTime.Now}\n{HubConnection.ConnectionId}");
                AddLocalMessage($"Try to Disconnect\n{DateTime.Now}\n{HubConnection.ConnectionId}", string.Empty);

                await HubConnection.StopAsync();

                if (IsConnected)
                    throw new InvalidOperationException("SignalRService.DisconnectAsync() faild");

                Debug.WriteLine($"Disconnection completed\n{DateTime.Now}");
                AddLocalMessage($"Disconnection completed\n{DateTime.Now}", string.Empty);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Disconnection Failed: " + ex.Message);
                AddLocalMessage("Disconnection Failed: " + ex.Message, string.Empty);
            }

            ActiveChannels.Clear();
        }
        public async Task JoinChannelAsync(string group, string user)
        {
            if (ActiveChannels.ContainsKey(group))
                return;

            await HubConnection.SendAsync("AddToGroup", group, user);
            ActiveChannels.Add(group, user);

        }
        public async Task LeaveChannelAsync(string group, string user)
        {
            if (!IsConnected || !ActiveChannels.ContainsKey(group))
                return;

            await HubConnection.SendAsync("RemoveFromGroup", group, user);

            ActiveChannels.Remove(group);
        }
        public async Task SendMessageToGroupAsync(string group, string sender, string message)
        {

            if (!IsConnected)
            {
                await ConnectAsync();
                await JoinChannelAsync(group, user);
            }

            var status = HubConnection.State;

            await HubConnection.InvokeAsync("SendMessageGroup", group, sender, message);

        }
        #endregion

        #region Public Events
        public event EventHandler<SignalREventArgs> Closed;
        public event EventHandler<SignalREventArgs> Reconnecting;
        public event EventHandler<SignalREventArgs> Reconnected;

        public event EventHandler<SignalREventArgs> OnEntered;
        public event EventHandler<SignalREventArgs> OnLeft;
        public event EventHandler<SignalREventArgs> OnReceivedMessage;
        #endregion

        #region destructor
        ~SignalRService()
        {
            Debug.WriteLine("Destructor was called");
        }
        #endregion
    }
}