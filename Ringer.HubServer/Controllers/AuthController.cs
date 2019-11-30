using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Ringer.Core.Models;
using Ringer.Core.Data;
using Ringer.HubServer.Data;
using Ringer.HubServer.Extensions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

            _logger.LogInformation($"user {user.Name} logged in.");

            // DB에 정보 없음.
            if (user == null)
                return NotFound("notFound");

            // 컨슈머가 방이 없다면 만들어 주고 입장시킨다.
            if (!user.Enrollments.Any() && user.UserType == UserType.Consumer)
                user.Enrollments.Add(new Enrollment { Room = new Room { Name = user.Name } });

            // User가 이미 Device를 등록했는지 체크 -> 새 device 등록
            if (!user.Devices.Any(d => d.Id == loginInfo.DeviceId))
                user.Devices.Add(new Device
                {
                    Id = loginInfo.DeviceId,
                    DeviceType = loginInfo.DeviceType,
                    IsOn = true
                });

            await _dbContext.SaveChangesAsync();

            var token = user.JwtToken(loginInfo);

            return Ok(user.JwtToken(loginInfo));
        }
    }
}
