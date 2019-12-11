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
using System.Linq;

namespace Ringer.Backend.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly RingerDbContext _dbContext;
        private readonly ILogger<ChatHub> _logger;

        int _userId => int.Parse(Context.UserIdentifier);
        string _deviceId => Context.User?.Claims?.FirstOrDefault(c => c.Type == "DeviceId")?.Value;
        string _deviceType => Context.User?.Claims?.FirstOrDefault(c => c.Type == "DeviceType")?.Value;

        public ChatHub(RingerDbContext dbContext, ILogger<ChatHub> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task AddToGroup(string group, string user)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, group);

            _logger.LogWarning($"User {user} entered to room. [room id: {group}]");

            // TODO: user가 방에 원래 있었다면 들어왔다는 메시지를 보내지 않는다.

            await Clients.Group(group).SendAsync("Entered", user);
        }
        public async Task RemoveFromGroup(string group, string user)
        {
            // TODO: user가 원래 방에 있었는지 확인

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);

            _logger.LogWarning($"User {user} removed from room. [room id: {group}]");

            await Clients.Group(group).SendAsync("Left", user);
        }
        public async Task SendMessageToRoomAsyc(string body, string roomId)
        {

            User user = await _dbContext.Users.FindAsync(_userId);

            Message message = new Message
            {
                Body = body,
                CreatedAt = DateTime.UtcNow,
                RoomId = roomId,
                SenderId = _userId
            };

            var sw = new Stopwatch();
            sw.Start();

            // 디비에 메시지 저장
            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();

            _logger.LogWarning($"Save To DB: {sw.ElapsedMilliseconds}");

            sw.Restart();

            // 접속중인 디바이스는 일단 다 보낸다.
            //await Clients.Group(roomId).SendAsync("ReceiveMessage", user.Name, body, _userId, message.CreatedAt);
            await Clients.Group(roomId).SendAsync("ReceiveMessage", user.Name, body, message.Id, _userId, message.CreatedAt);

            _logger.LogWarning($"Send to Connected Devices: {sw.ElapsedMilliseconds}");

            sw.Restart();

            // 룸에 속한 유저의 디바이스들 중 !IsOn인 디바이스는 푸시
            var room = await _dbContext.Rooms
                .Include(room => room.Enrollments)
                    .ThenInclude(enrollment => enrollment.User)
                        .ThenInclude(user => user.Devices)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == roomId);

            _logger.LogWarning($"Select un-connected devices: {sw.ElapsedMilliseconds}");

            sw.Restart();

            var pushDic = new Dictionary<string, string>();

            foreach (Enrollment enroll in room.Enrollments)
            {
                if (enroll.UserId != _userId)
                    foreach (Device device in enroll.User.Devices)
                    {
                        if (!device.IsOn &&
                            (device.DeviceType == DeviceType.iOS || device.DeviceType == DeviceType.Android))
                            pushDic.Add(device.Id, device.DeviceType == DeviceType.iOS ? "iOS" : "Android");
                    }
            }

            if (pushDic.Count > 0)
            {
                var customDataDic = new Dictionary<string, string>();
                customDataDic.Add("sound", "default");
                customDataDic.Add("room", roomId);
                customDataDic.Add("body", body);
                customDataDic.Add("sender", user.Name);

                var pushService = new PushService(pushDic);

                await pushService.Push(user.Name, body, customDataDic);

                foreach (var push in pushDic)
                    _logger.LogWarning($"Push message to [{push.Key}]({push.Value}) from {user.Name}");
            }

            sw.Stop();
            _logger.LogWarning($"Push to unconnected Devices: {sw.ElapsedMilliseconds}");
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

                _logger.LogWarning($"user {user.Name}({Context.ConnectionId}) with device [{_deviceId}]({_deviceType}) Connected.");

                foreach (Enrollment enrollment in user.Enrollments)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, enrollment.Room.Id);
                    _logger.LogWarning($"user {user.Name}({Context.ConnectionId}) with device [{_deviceId}]({_deviceType}) added to romm {enrollment.Room.Name}[{enrollment.Room.Id}].");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                await Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessage", "error", ex.Message);
            }
            finally
            {
                await base.OnConnectedAsync();
            }

        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                if (_deviceId != null)
                {
                    var device = await _dbContext.Devices.FirstOrDefaultAsync(d => d.Id == _deviceId);
                    device.IsOn = false;
                    await _dbContext.SaveChangesAsync();

                    _logger.LogWarning($"[{_deviceId}]({_deviceType})'s IsOn: {device.IsOn}");
                }

                // 접속한 Device의 Ower(User)가 속한 모든 방에서 Device를 제거
                User user = await _dbContext.Users
                    .Include(u => u.Enrollments)
                        .ThenInclude(enrollment => enrollment.Room)
                    .FirstOrDefaultAsync(u => u.Id == _userId);

                _logger.LogWarning($"user {user.Name}({Context.ConnectionId}) with device [{_deviceId}]({_deviceType}) Disconnected.");

                foreach (Enrollment enrollment in user.Enrollments)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, enrollment.Room.Id);

                    _logger.LogWarning($"user {user.Name}[{Context.ConnectionId}] with device [{_deviceId}]({_deviceType}) removed from romm {enrollment.Room.Name}[{enrollment.Room.Id}].");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            finally
            {
                await base.OnDisconnectedAsync(exception);
            }
        }

    }
}
