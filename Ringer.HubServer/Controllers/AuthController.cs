using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Ringer.Core.Models;
using Ringer.Core.Data;
using Ringer.HubServer.Data;
using Ringer.HubServer.Extensions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Collections.Generic;

namespace Ringer.HubServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RingerDbContext _dbContext;
        private readonly ILogger<AuthController> _logger;

        public AuthController(RingerDbContext dbContext, ILogger<AuthController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpPost("report")]
        public async Task<ActionResult> ReportStatusAsync(DeviceReport report)
        {
            var device = await _dbContext.Devices.FindAsync(report.DeviceId);

            if (device == null)
                return NotFound();

            if (device.DeviceType == DeviceType.iOS || device.DeviceType == DeviceType.Android)
            {
                device.IsOn = report.Status;
                await _dbContext.SaveChangesAsync();

                _logger.LogWarning($"Device [{device.Id}]({device.DeviceType}) is On:{device.IsOn}");
            }

            return Ok();
        }

        [HttpGet("list")]
        public async Task<ActionResult<string>> GetListAsync()
        {
            var rooms = await _dbContext.Rooms.ToListAsync();
            var response = JsonSerializer.Serialize<List<Room>>(rooms);

            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> UserLoginAsync(LoginInfo loginInfo)
        {
            User user = await _dbContext.Users
                .Include(u => u.Devices)
                .Include(u => u.Enrollments)
                .FirstOrDefaultAsync(u =>
                    u.Name == loginInfo.Name &&
                    u.BirthDate.Date == loginInfo.BirthDate.Date &&
                    u.Gender == loginInfo.Gender
                );

            // DB에 정보 없음.
            if (user == null)
                return NotFound("notFound");

            _logger.LogInformation($"user {user.Name} logged in.");

            string roomId = Guid.NewGuid().ToString();

            if (user.UserType == UserType.Consumer)
            {
                // 컨슈머가 방이 없다면 
                if (!user.Enrollments.Any())
                {
                    // 만들어 주고 입장시킨다.
                    user.Enrollments.Add(new Enrollment { Room = new Room { Id = roomId, Name = user.Name } });
                }
                else
                {
                    // 방이 있으니까 방 id를 알려준다.
                    roomId = user.Enrollments.First().RoomId;
                }
            }

            // User가 이미 Device를 등록했는지 체크 -> 새 device 등록
            if (!user.Devices.Any(d => d.Id == loginInfo.DeviceId))
            {
                // 디바이스가 디비에 존재하면 다른 유저가 이 디바이스를 사용했었던 것임.
                var device = await _dbContext.Devices.FirstOrDefaultAsync(d => d.Id == loginInfo.DeviceId);

                if (device != null)
                {
                    // 디바이스의 오너를 현재 유저로 변경한다.
                    device.Owner = user;
                }
                else
                {
                    user.Devices.Add(new Device
                    {
                        Id = loginInfo.DeviceId,
                        DeviceType = loginInfo.DeviceType,
                        IsOn = true // 로그인 자체를 chatpage에서 하니까..
                    });
                }
            }

            await _dbContext.SaveChangesAsync();

            var token = user.JwtToken(loginInfo);
            var response = new { Token = token, RoomId = roomId };

            return Ok(response);
        }
    }
}
