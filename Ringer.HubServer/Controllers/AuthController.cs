using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Ringer.Core.Models;
using Ringer.Core.Data;
using Ringer.HubServer.Data;
using Ringer.HubServer.Extensions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;

namespace Ringer.HubServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RingerDbContext _dbContext;

        public AuthController(RingerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> UserLoginAsync(LoginInfo loginInfo)
        {
            User user = await _dbContext.Users
                .Include(u => u.Devices)
                .FirstOrDefaultAsync(u =>
                    u.Name == loginInfo.Name &&
                    u.BirthDate.Date == loginInfo.BirthDate.Date &&
                    u.Gender == loginInfo.Gender
                );

            // TODO: DB에서 일치하는 user를 찾지 못했을 때
            // 새 user로 등록? 에러 throw?

            // DB에 정보 없음.
            if (user == null)
                return NotFound("notFound");

            // User가 이미 Device를 등록했는지 체크 -> 새 device 등록
            if (!user.Devices.Any(d => d.Id == loginInfo.DeviceId))
            {
                Device device = new Device
                {
                    Id = loginInfo.DeviceId,
                    DeviceType = loginInfo.DeviceType,

                    Owner = user
                };

                _dbContext.Devices.Add(device);
                await _dbContext.SaveChangesAsync();
            }


            return Ok(user.JwtToken(loginInfo));
        }
    }
}
