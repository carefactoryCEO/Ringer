using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ringer.Core.Models;
using Ringer.Core.Data;
using Ringer.HubServer.Data;
using Ringer.HubServer.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Ringer.Backend.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly RingerDbContext _dbContext;
        private readonly ILogger<ChatHub> _logger;

        int _userId => int.Parse(Context.UserIdentifier);

        public ChatHub(RingerDbContext dbContext, ILogger<ChatHub> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        #region present methods
        public async Task AddToGroup(string group, string user)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, group);

            _logger.LogInformation($"{user} entered to {group}");

            // TODO: user가 방에 원래 있었다면 들어왔다는 메시지를 보내지 않는다.

            await Clients.Group(group).SendAsync("Entered", user);
        }

        public async Task RemoveFromGroup(string group, string user)
        {
            // TODO: user가 원래 방에 있었는지 확인

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);

            await Clients.Group(group).SendAsync("Left", user);
        }
        #endregion

        public async Task SendMessageToRoomAsyc(string body, string roomId)
        {
            _logger.LogInformation($"message send to {roomId}");
            User user = await _dbContext.Users.FindAsync(_userId);

            // 접속중인 디바이스는 일단 다 보낸다.
            await Clients.Group(roomId).SendAsync("ReceiveMessage", user.Name, body);

            // 디비에 메시지 저장
            _dbContext.Messages.Add(new Message
            {
                Body = body,
                CreatedAt = DateTime.UtcNow,
                RoomId = roomId,
                SenderId = _userId
            });
            await _dbContext.SaveChangesAsync();

            // 룸에 속한 유저의 디바이스들 중 !IsOn인 디바이스는 푸시
            var room = await _dbContext.Rooms
                .Include(room => room.Enrollments)
                    .ThenInclude(enrollment => enrollment.User)
                        .ThenInclude(user => user.Devices)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == roomId);

            var pushDic = new Dictionary<Guid, string>();

            foreach (Enrollment enroll in room.Enrollments)
                foreach (Device device in enroll.User.Devices)
                {
                    if (!device.IsOn && (device.DeviceType == DeviceType.iOS || device.DeviceType == DeviceType.Android))
                        pushDic.Add(Guid.Parse(device.Id), device.DeviceType == DeviceType.iOS ? "iOS" : "Android");
                }


            if (pushDic.Count > 0)
            {
                var contentDic = new Dictionary<string, string>();
                contentDic.Add("sound", "default");
                contentDic.Add("room", roomId);
                contentDic.Add("body", body);
                contentDic.Add("sender", user.Name);

                var pushService = new PushService(pushDic);
                await pushService.Push(user.Name, body, contentDic);
            }

            foreach (var dic in pushDic)
                _logger.LogWarning($"Push to device id {dic.Key}({dic.Value})");
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                // 접속한 Device의 Owner(User)가 속한 모든 방에 Device를 추가
                User user = await _dbContext.Users
                    .Include(user => user.Enrollments)
                        .ThenInclude(enrollment => enrollment.Room)
                    .FirstOrDefaultAsync(u => u.Id == _userId);

                _logger.LogInformation($"user {user.Name}({Context.ConnectionId}) Connected.");

                foreach (Enrollment enrollment in user.Enrollments)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, enrollment.Room.Id);

                    _logger.LogInformation($"user {user.Name}({Context.ConnectionId}) added to romm {enrollment.Room.Id}.");
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
                    .FirstOrDefaultAsync(u => u.Id == _userId);

                _logger.LogInformation($"user {user.Name}({Context.ConnectionId}) Disconnected.");

                foreach (Enrollment enrollment in user.Enrollments)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, enrollment.Room.Id);

                    _logger.LogInformation($"user {user.Name}({Context.ConnectionId}) removed from romm {enrollment.Room.Id}.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                await Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessage", "error", ex.Message);
            }
        }

    }
}
