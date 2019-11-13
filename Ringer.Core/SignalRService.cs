using Microsoft.AspNetCore.SignalR.Client;
using Ringer.Core.EventArgs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Ringer.Core
{
    public class SignalRService
    {
        #region private members
        string url = null;
        #endregion

        #region public properties
        public bool IsConnected => HubConnection.State == HubConnectionState.Connected;
        public Dictionary<string, string> ActiveChannels { get; } = new Dictionary<string, string>();
        public HubConnection HubConnection { get; private set; }
        #endregion

        #region Initializer
        public void Init(string urlRoot, bool useHttps)
        {
            if (url == null)
            {
                var port = (urlRoot == "localhost" || urlRoot == "10.0.2.2") ? (useHttps ? ":5001" : ":5000") :string.Empty;

                url = $"http{(useHttps ? "s" : string.Empty)}://{urlRoot}{port}/hubs/chat";
            }
            
            HubConnection = new HubConnectionBuilder().WithUrl(url).Build();

            // Handle Hub connection events
            HubConnection.Closed        += HubConnection_Closed;
            HubConnection.Reconnecting  += HubConnection_Reconnecting;
            HubConnection.Reconnected   += HubConnection_Reconnected;
            
            // Handle Hub messages
            HubConnection.On<string, string>("ReceiveMessage", HandleRecievedMessage);
            HubConnection.On<string>("Entered", HandleEntered);
            HubConnection.On<string>("Left", HandleLeft);
        }
        #endregion

        #region Private Mothods
        private async Task HubConnection_Closed(Exception err)
        {
            Closed?.Invoke(
                    this,
                    new ChatEventArgs($"Service.Closed...{DateTime.Now} {err.Message}", string.Empty));

            ActiveChannels.Clear();

            do
            {
                await Task.Delay(333);

                try
                {
                    Init(null, true);
                    await ConnectAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

            } while (!IsConnected);

        }
        private Task HubConnection_Reconnecting(Exception err)
        {
            Reconnecting?.Invoke(
                   this,
                   new ChatEventArgs($"Service.Reconnecting...{DateTime.Now} {err.Message}", string.Empty));

            return Task.CompletedTask;
        }
        private Task HubConnection_Reconnected(string message)
        {
            Reconnected?.Invoke(
                    this,
                    new ChatEventArgs($"Service.Reconnected...{DateTime.Now} {message}", string.Empty));

            return Task.CompletedTask;
        }

        private void HandleRecievedMessage(string user, string message)
        {
            OnReceivedMessage?.Invoke(this, new ChatEventArgs(message, user));
        }
        private void HandleEntered(string user)
        {
            OnEntered?.Invoke(this, new ChatEventArgs($"{user} entered.", user));
        }
        private void HandleLeft(string user)
        {
            OnLeft?.Invoke(this, new ChatEventArgs($"{user} left.", user));
        }
        #endregion

        #region Public Methods
        public async Task ConnectAsync()
        {
            if (IsConnected)
                return;

            try
            {
                await HubConnection.StartAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Connection Failed: " + ex.Message);
            }

            
        }
        public async Task DisconnectAsync()
        {
            if (!IsConnected)
                return;
            try
            {
                await HubConnection.StopAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            ActiveChannels.Clear();
        }
        public async Task LeaveChannelAsync(string group, string userName)
        {
            if (!IsConnected || !ActiveChannels.ContainsKey(group))
                return;

            await HubConnection.SendAsync("RemoveFromGroup", group, userName);

            ActiveChannels.Remove(group);
        }
        public async Task JoinChannelAsync(string group, string userName)
        {
            if (!IsConnected || ActiveChannels.ContainsKey(group))
                return;

            await HubConnection.SendAsync("AddToGroup", group, userName);
            ActiveChannels.Add(group, userName);

        }
        public async Task SendMessageAsync(string group, string userName, string message)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Not connected");

            await HubConnection.InvokeAsync("SendMessageGroup", group, userName, message);
        }
        #endregion

        #region Events
        public event EventHandler<ChatEventArgs> Closed;
        public event EventHandler<ChatEventArgs> Reconnecting;
        public event EventHandler<ChatEventArgs> Reconnected;

        public event EventHandler<ChatEventArgs> OnEntered;
        public event EventHandler<ChatEventArgs> OnLeft;
        public event EventHandler<ChatEventArgs> OnReceivedMessage;
        #endregion

        #region destructor
        ~SignalRService()
        {
            Console.WriteLine("Destructor was called");
            Closed?.Invoke(this, new ChatEventArgs($"ChatService.Destructor:Service Destructed..{DateTime.Now}", string.Empty));
        }
        #endregion
    }
}