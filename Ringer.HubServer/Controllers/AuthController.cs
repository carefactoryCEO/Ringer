using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Ringer.Core.Models;
using Ringer.HubServer.Data;
using Ringer.HubServer.Extensions;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

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

            // TODO: DB에서 일치하는 user를 찾지 못했을 때 새 user로 등록

            return Ok(user.JwtToken());
        }
    }


    /*
     * var loginInfo = JsonSerializer.Serialize(new
            {
                Email = "test@carefactory.co.kr",
                Password = "some-password",
                DeviceType = "console",
                LoginType = "Password",
                Name = name
            });
     */
}
