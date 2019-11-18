using Microsoft.AspNetCore.SignalR;
using System;
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
        /// Hub Calls this method when new connection is established
        /// </summary>
        /// <returns></returns>
        public override Task OnConnectedAsync()
        {
            string connectionId = Context.ConnectionId;
            System.Security.Claims.ClaimsPrincipal user = Context.User;
            System.Threading.CancellationToken cancellationToken = Context.ConnectionAborted;

            return base.OnConnectedAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override Task OnDisconnectedAsync(Exception exception)
        {
            string connectionId = Context.ConnectionId;
            System.Security.Claims.ClaimsPrincipal user = Context.User;
            System.Threading.CancellationToken cancellationToken = Context.ConnectionAborted;

            return base.OnDisconnectedAsync(exception);
        }
    }
}
