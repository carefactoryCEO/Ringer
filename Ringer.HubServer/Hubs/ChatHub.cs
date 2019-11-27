using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Ringer.HubServer.Data;
using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ringer.Backend.Hubs
{
    //[Authorize]
    public class ChatHub : Hub
    {
        private string Name => Context.User?.FindFirst(ClaimTypes.Name)?.Value;

        private readonly RingerDbContext _dbContext;

        public ChatHub(RingerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

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

            logMessage = $"{Name}({Context.UserIdentifier}) sent: {message}";

            //_logger.LogWarning(logMessage);

            Debug.WriteLine(logMessage);
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            logMessage = $"{Name}({Context.UserIdentifier}) disconnected";

            return base.OnDisconnectedAsync(exception);
        }

        string logMessage;

        public override Task OnConnectedAsync()
        {

            logMessage = $"{Name}({Context.UserIdentifier}) Connected";
            Debug.WriteLine(logMessage);

            return base.OnConnectedAsync();
        }
    }
}
