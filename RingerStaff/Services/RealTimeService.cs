﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Ringer.Core.EventArgs;
using RingerStaff.Models;

namespace RingerStaff.Services
{
    public static class RealTimeService
    {
        private static HubConnection _hubConnection;

        public static bool IsConnected =>
            _hubConnection != null &&
            _hubConnection.ConnectionId != null &&
            _hubConnection.State == HubConnectionState.Connected;

        public static void Init(string url, string token)
        {
            if (IsConnected)
                return;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(url, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(token);
                })
                .WithAutomaticReconnect(new[]
                {
                    TimeSpan.FromSeconds(0),
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(1)
                })
                .Build();

            _hubConnection.Closed += OnClosed;
            _hubConnection.Reconnecting += OnReconnecting;
            _hubConnection.Reconnecting += OnReconnected;

            _hubConnection.On<string, string, int, int, DateTime>("ReceiveMessage", OnReceiveMessage);
            _hubConnection.On<string>("Entered", OnEntered);
            _hubConnection.On<string>("Left", OnLeft);
        }

        public static string Url;
        public static string Token;

        public static async Task SendMessageAsync(MessageModel message, string roomId)
        {
            if (!IsConnected)
                await ConnectAsync();

            var id = await _hubConnection.InvokeAsync<int>("SendMessageToRoomAsyc", message.Body, roomId);

            Debug.WriteLine(id);

        }

        public static async Task EnterRoomAsync(string roomId, string name)
        {
            if (!IsConnected)
                await ConnectAsync();

            await _hubConnection.InvokeAsync("AddToGroup", roomId, name);
        }

        public static async Task ConnectAsync(string url, string token)
        {
            if (IsConnected)
                return;

            Url = url;
            Token = token;

            Init(url, token);
            await ConnectAsync();
        }

        public static async Task ConnectAsync()
        {
            if (IsConnected)
                return;

            try
            {
                Connecting?.Invoke(_hubConnection, new ConnectionEventArgs($"Try to Connect to SignalR Hub"));

                await _hubConnection.StartAsync().ConfigureAwait(false);

                if (!IsConnected)
                    throw new InvalidOperationException($"Connecting to SignalR Hub failed.");

                Connected?.Invoke(_hubConnection, new ConnectionEventArgs($"Successfully Connected to SignarR Hub. ConnectionId:{_hubConnection.ConnectionId}"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                ConnectionFailed?.Invoke(_hubConnection, new ConnectionEventArgs(ex.Message));
            }
        }

        private static void OnLeft(string obj)
        {
            throw new NotImplementedException();
        }

        private static void OnEntered(string obj)
        {
            throw new NotImplementedException();
        }

        private static void OnReceiveMessage(string senderName, string body, int messageId, int senderId, DateTime createdAt)
        {
            MessageReceived?.Invoke(_hubConnection, new MessageReceivedEventArgs(body, senderName, messageId, senderId, createdAt));
        }

        private static Task OnReconnected(Exception arg)
        {
            throw new NotImplementedException();
        }

        private static Task OnReconnecting(Exception arg)
        {
            throw new NotImplementedException();
        }

        private static Task OnClosed(Exception arg)
        {
            throw new NotImplementedException();
        }

        public static event EventHandler<ConnectionEventArgs> Connecting;
        public static event EventHandler<ConnectionEventArgs> Connected;
        public static event EventHandler<ConnectionEventArgs> ConnectionFailed;
        public static event EventHandler<ConnectionEventArgs> Disconnecting;
        public static event EventHandler<ConnectionEventArgs> Disconnected;
        public static event EventHandler<ConnectionEventArgs> DisconnectionFailed;
        public static event EventHandler<ConnectionEventArgs> Closed;
        public static event EventHandler<ConnectionEventArgs> Reconnecting;
        public static event EventHandler<ConnectionEventArgs> Reconnected;

        public static event EventHandler<SignalREventArgs> SomeoneEntered;
        public static event EventHandler<SignalREventArgs> SomeoneLeft;

        public static event EventHandler<MessageReceivedEventArgs> MessageReceived;
    }
}