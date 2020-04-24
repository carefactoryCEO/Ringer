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
using Ringer.HubServer.Services;
using Microsoft.Extensions.Configuration;

namespace Ringer.HubServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RingerDbContext _dbContext;
        private readonly ILogger<AuthController> _logger;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender emailSender;

        public AuthController(RingerDbContext dbContext, ILogger<AuthController> logger, IUserService userService, IConfiguration configuration, IEmailSender emailSender)
        {
            _dbContext = dbContext;
            _logger = logger;
            _userService = userService;
            _configuration = configuration;
            this.emailSender = emailSender;
        }

        [HttpGet("send")]
        public async Task<ActionResult> SendTestMail()
        {
            await emailSender.SendMail("jhylmb@gmail.com", "안녕하세요", "<h1>링거입니다.</h1>");

            return Ok();
        }

        [HttpGet("user/{id}")]
        public async Task<ActionResult<User>> GetUserByIdAsync(int id)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id).ConfigureAwait(false);

            if (user == null)
                return NotFound();

            return user;
        }

        #region staff
        [HttpPost("register")]
        public async Task<ActionResult> RegisterAsync([FromBody]RegisterInfo registerInfo)
        {

            // TODO: implement AutoMapper registerInfo -> User
            var user = new User
            {
                Name = registerInfo.Name,
                Email = registerInfo.Email,
                UserType = UserType.Staff,
                CreatedAt = DateTime.Now,
            };

            try
            {
                var bio = registerInfo.BirthDateGenderString;

                if (int.TryParse(bio.Substring(6, 1), out int sex) &&
                    int.TryParse(bio.Substring(0, 2), out int year) &&
                    int.TryParse(bio.Substring(2, 2), out int month) &&
                    int.TryParse(bio.Substring(4, 2), out int day))
                {
                    year += sex > 2 ? 2000 : 1900;
                    user.BirthDate = new DateTime(year, month, day);
                    user.Gender = sex % 2 == 0 ? GenderType.Female : GenderType.Male;
                }
                await _userService.CreateAsync(user, registerInfo.Password);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("staff-login")]
        public async Task<ActionResult> StaffLoginAsync([FromBody]LoginInfo loginInfo)
        {
            var user = await _userService.LogInAsync(loginInfo.Email, loginInfo.Password);

            if (user == null)
                return BadRequest();

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
                    });
                }

                await _dbContext.SaveChangesAsync();
            }

            var secretKey = _configuration["SecurityKey"];

            var token = user.JwtToken(loginInfo, secretKey);
            var response = new LoginResponse
            {
                token = token,
                roomId = null,
                userId = user.Id,
                success = true,
                userName = user.Name
            };

            return Ok(response);
        }
        #endregion

        #region Terms
        [HttpGet("terms/{id}")]
        public async Task<Terms> GetTermsById(int id)
        {
            return await _dbContext.Terms.FirstOrDefaultAsync(t => t.Id == id);
        }

        [HttpPost("terms")]
        public async Task SetAgreements([FromBody]List<Agreement> agreements)
        {
            _dbContext.AddRange(agreements);
            await _dbContext.SaveChangesAsync();
        }

        [HttpGet("terms")]
        public async Task<IEnumerable<Terms>> GetTermsListAsync()
        {
            return await _dbContext.Terms.Where(t => t.IsCurrent).ToListAsync();
        }
        #endregion

        #region consumer
        [HttpPost("register-consumer")]
        public async Task<ActionResult> RegisterConsumerAsync([FromBody]RegisterConsumerRequest req)
        {
            try
            {
                var secretKey = _configuration["SecurityKey"];
                var device = req.Device;
                device.IsActive = true;
                string roomId = Guid.NewGuid().ToString();
                var tokenInfo = new LoginInfo { DeviceId = device.Id, DeviceType = device.DeviceType };
                var user = await _dbContext.Users
                    .Where(u => u.Name == req.User.Name)
                    .Where(u => u.BirthDate.Date == req.User.BirthDate.Date)
                    .Where(u => u.Gender == req.User.Gender)
                    .Where(u => u.Email == req.User.Email)
                    .Include(u => u.Devices)
                    .Include(u => u.Enrollments)
                    .FirstOrDefaultAsync();

                if (user != null) // user already exists...
                {
                    // 방금 입력한 비밀번호로 로그인 되면
                    if (_userService.VerifyPasswordHash(req.User.Password, user.PasswordHash, user.PasswordSalt))
                    {
                        // 이전 기기들 비활성화
                        foreach (var d in user.Devices)
                            d.IsActive = false;

                        // 로그인한 기기 활성화
                        user.Devices.Add(device);

                        if (user.Enrollments.Any())
                        {
                            roomId = user.Enrollments.First().RoomId;
                        }
                        else
                        {
                            var room = new Room { Name = user.Name, Id = roomId };
                            user.Enrollments.Add(new Enrollment { Room = room });
                        }

                        await _dbContext.SaveChangesAsync();

                        return Ok(new RegisterConsumerResponse
                        {
                            RoomId = roomId,
                            UserId = user.Id,
                            UserName = user.Name,
                            Success = true,
                            Token = user.JwtToken(tokenInfo, secretKey)
                        });

                        // TODO: App.Startup()에서 Device의 활성 상태 검증
                        // 비활성 기기면 재로그인
                    }
                    else // Name, BirthDate, Sex, Email은 일치하지만 비번이 틀림
                    {
                        // TODO: 비밀번호 재입력 및 재설정 안내
                        // 신모법님은 링거에 가입되어 있지만 비밀번호가 틀렸습니다. 정확한 비밀번호를 입력하세요.
                        // 여기를 누르면 계정이메일(jhylmb@gmail.com)으로 비밀번호 재설정 링크를 발송합니다.
                        // jhylmb@gmail.com으로 비밀번호 재설정 링크를 발송했습니다.
                        return Unauthorized(new RegisterConsumerResponse
                        {
                            Success = false,
                            RequireLogin = true
                        }); ;
                    }
                }
                else
                {
                    var createdUser = await _userService.CreateAsync(req.User, req.User.Password);

                    user = await _dbContext.Users
                        .Include(u => u.Devices)
                        .Include(u => u.Enrollments)
                        .FirstOrDefaultAsync(u => u.Id == createdUser.Id);

                    // add device
                    user.Devices.Add(device);

                    // add enrollment / room
                    var room = new Room { Id = roomId, Name = user.Name };
                    user.Enrollments.Add(new Enrollment { Room = room });

                    await _dbContext.SaveChangesAsync();

                    return Ok(new RegisterConsumerResponse
                    {
                        RoomId = room.Id,
                        UserId = user.Id,
                        UserName = user.Name,
                        Success = true,
                        Token = user.JwtToken(tokenInfo, secretKey)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Consumer Registration Throws", req);
                return StatusCode(500);
            }
        }

        [HttpPost("login-consumer")]
        public async Task<ActionResult> LoginConsumerAsync()
        {
            await Task.Delay(0);
            return Ok();
        }
        #endregion

        #region console
        [HttpPost("login")]
        public async Task<ActionResult<string>> UserLoginAsync([FromBody]LoginInfo loginInfo)
        {
            var secretKey = _configuration["SecurityKey"];

            // TODO: Consumer인 경우 Ticket의 Travel 정보와 LoginInfo의 Location 정보를 대조
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
                return NotFound(new LoginResponse { success = false });

            _logger.LogWarning($"user {user.Name} logged in.");

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

            var token = user.JwtToken(loginInfo, secretKey);
            var response = new LoginResponse
            {
                token = token,
                roomId = roomId,
                userId = user.Id,
                userName = user.Name,
                success = true
            };

            return Ok(response);
        }
        #endregion
    }
}
