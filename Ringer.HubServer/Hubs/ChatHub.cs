using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ringer.Core.Models;
using Ringer.HubServer.Data;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Ringer.Backend.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly RingerDbContext _dbContext;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(RingerDbContext dbContext, ILogger<ChatHub> logger)
        {
            _dbContext = dbContext;
            this._logger = logger;
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
            try
            {
                // 접속한 Device의 Owner(User)가 속한 모든 방에 Device를 추가
                int userId = int.Parse(Context.UserIdentifier);

                User user = await _dbContext.Users
                    .Include(user => user.Enrollments)
                        .ThenInclude(enrollment => enrollment.Room)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                _logger.LogInformation($"user {user.Name}({Context.ConnectionId}) Connected.");

                foreach (Enrollment enrollment in user.Enrollments)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, enrollment.Room.Name);

                    _logger.LogInformation($"user {user.Name}({Context.ConnectionId}) added to romm {enrollment.Room.Name}.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                await Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessage", "error", ex.Message);
            }

            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);

            try
            {
                // 접속한 Device의 Ower(User)가 속한 모든 방에서 Device를 제거
                User user = await _dbContext.Users
                    .Include(u => u.Enrollments)
                        .ThenInclude(enrollment => enrollment.Room)
                    .FirstOrDefaultAsync(u => u.Id == int.Parse(Context.UserIdentifier));

                _logger.LogInformation($"user {user.Name}({Context.ConnectionId}) Disconnected.");

                foreach (Enrollment enrollment in user.Enrollments)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, enrollment.Room.Name);

                    _logger.LogInformation($"user {user.Name}({Context.ConnectionId}) removed from romm {enrollment.Room.Name}.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                await Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessage", "error", ex.Message);
            }
        }
        public async Task SendMessageToRoomAsyc(string content, string sender, int roomId)
        {
            // 접속중인 디바이스는 일단 다 보낸다.
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveMessage", sender, content);

            // 디비에 메시지 저장
            var message = new Message(content, sender);
            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();

            // 룸에 속한 유저의 디바이스들 중 !IsOn인 디바이스는 푸시
            var room = await _dbContext.Rooms
                .Include(room => room.Enrollments)
                    .ThenInclude(enrollment => enrollment.User)
                        .ThenInclude(user => user.Devices.Where(device => !device.IsOn))
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == roomId);

            foreach (Enrollment enroll in room.Enrollments)
                foreach (Device device in enroll.User.Devices)
                {
                    // device.pendings에 message 추가

                    // device에 push

                    Console.WriteLine($"{enroll.User.Name}({device.DeviceType}): {device.Id}");

                }
        }
    }
}
