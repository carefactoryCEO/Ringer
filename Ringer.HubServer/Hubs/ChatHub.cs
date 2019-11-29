using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Ringer.Core.Models;
using Ringer.HubServer.Data;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ringer.Backend.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly RingerDbContext _dbContext;

        public ChatHub(RingerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region present methods
        public async Task AddToGroup(string group, string user)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, group);

            // TODO: user가 방에 원래 있었다면 들어왔다는 메시지를 보내지 않는다.

            await Clients.Group(group).SendAsync("Entered", user);
        }

        public async Task RemoveFromGroup(string group, string user)
        {
            // TODO: user가 원래 방에 있었는지 확인

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);

            await Clients.Group(group).SendAsync("Left", user);
        }

        public async Task SendMessageGroup(string group, string sender, string content)
        {
            await Clients.Group(group).SendAsync("ReceiveMessage", sender, content);

            var message = new Message
            {
                Content = content,
                Sender = sender
            };

            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();

            Console.WriteLine($"------------- content:{message.Content} -- sender:{message.Sender} --------------");
        }
        #endregion

        public override async Task OnConnectedAsync()
        {
            // Get user id
            string userId = Context.UserIdentifier;

            // null check
            if (userId != null)
            {
                Console.WriteLine($"------------- userId: {userId} --------------");
            }

            // Get Device id
            string deviceId = Context.User?.Claims?.FirstOrDefault(c => c.Type == "DeviceId")?.Value;

            // null check
            if (deviceId != null)
            {
                Console.WriteLine($"------------- deviceId: {deviceId} --------------");

                var device = await _dbContext.Devices.Include(d => d.Owner).FirstOrDefaultAsync(d => d.Id == deviceId);
                device.IsOn = true;
                await _dbContext.SaveChangesAsync();

                Console.WriteLine($"------------- Owner: {device.Owner.Name} --------------");

                Console.WriteLine($"------------- change saved --------------");
            }

            // Query DB using user id
            User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(userId));



            // TODO: Mark User as connected
            //user.IsConnected = true;

            // Save Changes
            //await _dbContext.SaveChangesAsync(); 

            string claimName = Context.User?.FindFirstValue(ClaimTypes.Name);
            string claimId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string claimBabo = Context?.User?.Claims?.FirstOrDefault(c => c.Type == "testClaim")?.Value;

            // TODO: base method를 실행할 필요가 있을까? 판단해야.
            // 이미 커넥션 자체는 이루어졌고 그 후처리를 할 뿐이므로 필요 없나?
            // 아니면 base에서도 커넥션 이후에 후처리할 일이 있을 수도 있겠다.
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);

            var deviceId = Context.User?.Claims?.FirstOrDefault(c => c.Type == "DeviceId")?.Value;

            // null check
            if (deviceId != null)
            {
                var device = await _dbContext.Devices.FirstOrDefaultAsync(d => d.Id == deviceId);
                device.IsOn = false;
                await _dbContext.SaveChangesAsync();
            }
        }
        public async Task SendMessageToRoomAsyc(string content, string sender, int roomId)
        {
            // send all connected devices 접속중인 기계는 다 보낸다.
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveMessage", sender, content);

            // save message to db
            var message = new Message(content, sender);
            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();

            // 룸에 속한 user들을 찾는다.

            // 각 유저의 디바이스들 중에서 IsOn == false인 디바이스에 푸시
        }
    }
}
