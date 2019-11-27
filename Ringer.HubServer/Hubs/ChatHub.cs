using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Ringer.HubServer.Data;
using System;
using System.Diagnostics;
using System.Linq;
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

        public override async Task OnConnectedAsync()
        {
            // Get user id
            var id = int.Parse(Context.UserIdentifier);
            
            // Query DB using user id
            var user = _dbContext.Users.FirstOrDefault(u => u.ID == id);

            // TODO: Mark User as connected
            //user.IsConnected = true;

            // Save Changes
            //await _dbContext.SaveChangesAsync(); 
            
            string claimName = Context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;

            logMessage = $"[ChatHub]{Context.User}({Context.UserIdentifier}) Connected";

            Debug.WriteLine(logMessage);

            // TODO: base method를 실행할 필요가 있을까? 판단해야.
            // 이미 커넥션 자체는 이루어졌고 그 후처리를 할 뿐이므로 필요 없나?
            // 아니면 base에서도 커넥션 이후에 후처리할 일이 있을 수도 있겠다.
            await base.OnConnectedAsync();
        }
    }
}
