using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Ringer.Core.Models;
using Ringer.HubServer.Data;
using Ringer.HubServer.Extensions;

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
        public ActionResult<string> Login(LoginInfo loginInfo)
        {
            User user = _dbContext.Users.FirstOrDefault(u =>
                u.Name == loginInfo.Name &&
                u.BirthDate.Date == loginInfo.BirthDate.Date &&
                u.Gender == loginInfo.Gender
            );

            // DB에 정보 없음.
            if (user == null)
                return NotFound("notFound");

            // TODO: DB에서 일치하는 user를 찾지 못했을 때
            // 새 user로 등록? 에러 throw?

            return Ok(user.JwtToken());
        }
    }
}
