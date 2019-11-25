using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ringer.Backend.Hubs
{
    [Authorize]
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

        public async Task SendMessageGroup(string group, string sender, string message)
        {
            await Clients.Group(group).SendAsync("ReceiveMessage", sender, message);

            var email = Context.User?.FindFirst(ClaimTypes.Email)?.Value;

            Debug.WriteLine($"{email}({Context.UserIdentifier}) sent: {message}");
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {

            var email = Context.User?.FindFirst(ClaimTypes.Email)?.Value;

            Debug.WriteLine(exception?.Message);
            Debug.WriteLine($"{email}({Context.UserIdentifier}) disconnected");

            return base.OnDisconnectedAsync(exception);
        }

        public override Task OnConnectedAsync()
        {
            var email = Context.User?.FindFirst(ClaimTypes.Email)?.Value;
            Debug.WriteLine($"{email}({Context.UserIdentifier}) Connected");

            return base.OnConnectedAsync();
        }
    }
}
