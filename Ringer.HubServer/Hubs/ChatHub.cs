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
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;

namespace Ringer.HubServer.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly RingerDbContext _dbContext;
        private readonly ILogger<ChatHub> _logger;
        private readonly IWebHostEnvironment _env;

        private int UserId => Convert.ToInt32(Context.UserIdentifier);
        private string DeviceId => Context.User?.Claims?.FirstOrDefault(c => c.Type == "DeviceId")?.Value;
        private string DeviceType => Context.User?.Claims?.FirstOrDefault(c => c.Type == "DeviceType")?.Value;
        private string UserName => Context.User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

        public ChatHub(RingerDbContext dbContext, ILogger<ChatHub> logger, IWebHostEnvironment env)
        {
            _dbContext = dbContext;
            _logger = logger;
            _env = env;
        }

        #region Enter or Leave a Room
        public async Task AddToGroup(string roomId, string userName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            var room = await _dbContext.Rooms
                .Include(r => r.Enrollments)
                    .ThenInclude(e => e.User)
                .FirstOrDefaultAsync(r => r.Id == roomId);

            // check if the user is already in this room
            var en = room.Enrollments.FirstOrDefault(e => e.UserId == UserId);
            if (en is null)
            {
                room.Enrollments.Add(new Enrollment{ UserId = UserId, RoomId = roomId });
                
                _logger.LogWarning($"User({userName}) entered to room({roomId}) with device({DeviceId}({DeviceType}))");
                
                await _dbContext.SaveChangesAsync();
                // notify entering
                await Clients.Group(roomId).SendAsync("Entered", userName);
            }

        }
        public async Task RemoveFromGroup(string roomId, string userName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);

            var room = await _dbContext.Rooms
                .Include(r => r.Enrollments)
                .FirstOrDefaultAsync(r => r.Id == roomId);

            var en = room.Enrollments.FirstOrDefault(e => e.UserId == UserId);

            if (en is null)
                return;

            _logger.LogWarning($"User {userName} removed from room. [room id: {roomId}]");
            await Clients.Group(roomId).SendAsync("Left", userName);

            room.Enrollments.Remove(en);
            await _dbContext.SaveChangesAsync();
        }
        #endregion

        #region Send Message to Room
        public async Task SendMessageToRoomAsyc(string body, string roomId)
        {
            User user = await _dbContext.Users.FindAsync(UserId);

            Message message = new Message
            {
                Body = body,
                CreatedAt = DateTime.UtcNow,
                RoomId = roomId,
                SenderId = UserId
            };

            var sw = new Stopwatch();
            sw.Start();

            // 디비에 메시지 저장
            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();

            _logger.LogWarning($"Save To DB: {sw.ElapsedMilliseconds}");

            sw.Restart();

            // 접속중인 디바이스는 일단 다 보낸다.
            await Clients.Group(roomId).SendAsync("ReceiveMessage", user.Name, body, message.Id, UserId, message.CreatedAt, roomId);

            _logger.LogWarning($"Send to Connected Devices: {sw.ElapsedMilliseconds} millisecond");
            _logger.LogWarning($"Message id: {message?.Id ?? -1}");

            // production에서만 푸시
            // TODO: DeviceType에 simulator/emulator/vertual을 추가해서 개발 도중에도 푸시 받을 수 있도록 한다.
            if (_env.IsDevelopment())
            {
                sw.Stop();
                return;
            }

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
                if (enroll.UserId != UserId)
                    foreach (Device device in enroll.User.Devices)
                    {
                        if (!device.IsOn &&
                            (device.DeviceType == Core.Data.DeviceType.iOS || device.DeviceType == Core.Data.DeviceType.Android))
                            pushDic.Add(device.Id, device.DeviceType == Core.Data.DeviceType.iOS ? "iOS" : "Android");
                    }
            }

            //if (pushDic.Count > 0 && false)
            if (pushDic.Count > 0)
            {
                var customDataDic = new Dictionary<string, string>();
                customDataDic.Add("sound", "default");
                customDataDic.Add("room", roomId);
                customDataDic.Add("body", body);
                customDataDic.Add("sender", user.Name);

                var pushService = new PushService(pushDic);

                pushService.Push(user.Name, body, customDataDic);

                foreach (var push in pushDic)
                    _logger.LogWarning($"Push message to [{push.Key}]({push.Value}) from {user.Name}");
            }

            sw.Stop();
            _logger.LogWarning($"Push to unconnected Devices: {sw.ElapsedMilliseconds}");
        }
        #endregion

        #region Connection Control
        public override async Task OnConnectedAsync()
        {
            try
            {
                _logger.LogWarning($"user {UserName}({Context.ConnectionId}) with device [{DeviceId}]({DeviceType}) Connected.");

                var device = await _dbContext.Devices
                    .Include(d => d.Owner)
                        .ThenInclude(u => u.Enrollments)
                            .ThenInclude(e => e.Room)
                    .FirstOrDefaultAsync(d => d.Id == DeviceId);

                if (device != null)
                {
                    device.IsOn = true;
                    device.ConnectionId = Context.ConnectionId;

                    await _dbContext.SaveChangesAsync();

                    foreach (var en in device.Owner.Enrollments)
                    {
                        await AddToGroup(en.RoomId, UserName);
                        _logger.LogWarning($"user {UserName}({Context.ConnectionId}) with device [{DeviceId}]({DeviceType}) added to romm {en.Room.Name}[{en.Room.Id}].");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
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
                var device = await _dbContext.Devices.FirstOrDefaultAsync(d => d.Id == DeviceId);

                if (device != null)
                {
                    device.ConnectionId = null;
                    device.IsOn = false;
                    await _dbContext.SaveChangesAsync();
                    _logger.LogWarning($"[{DeviceId}]({DeviceType})'s IsOn: {device.IsOn}");
                    _logger.LogWarning($"user {UserName}({Context.ConnectionId}) with device [{DeviceId}]({DeviceType}) Disconnected.");
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
        #endregion
    }
}
