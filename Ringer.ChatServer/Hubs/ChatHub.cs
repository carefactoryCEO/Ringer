using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ringer.Backend.Hubs
{
    public class ChatHub : Hub
    {
        public async Task AddToGroup(string group, string user)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, group);

            await Clients.Group(group).SendAsync("Entered", user);
        }

        public async Task RemoveFromGroup(string group, string user)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);

            await Clients.Group(group).SendAsync("Left", user);
        }

        public async Task SendMessageGroup(string group, string user, string message)
        {
            await Clients.Group(group).SendAsync("ReceiveMessage", user, message);
        }

        public async Task SendMessageToUser(string userid, string message)
        {
            await Clients.User(userid).SendAsync("ReceivePrivateMessage", message);
        }
        /// <summary>
        /// Hub Calls this method when a new connection is established
        /// </summary>
        /// <returns></returns>
        //public override Task OnConnectedAsync()
        //{
        //    return base.OnConnectedAsync();
        //}

        /// <summary>
        /// Hub calls this method when a device is disconnected
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        //public override Task OnDisconnectedAsync(Exception exception)
        //{
        //    return base.OnDisconnectedAsync(exception);
        //}
    }
}
