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

        public AuthController(RingerDbContext dbContext, ILogger<AuthController> logger, IUserService userService, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _logger = logger;
            _userService = userService;
            _configuration = configuration;
        }

        [HttpGet("user/{id}")]
        public async Task<ActionResult<User>> GetUserByIdAsync(int id){
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id).ConfigureAwait(false);

            if (user == null)
                return NotFound();

            return user;
        }

        [HttpGet("messagelist")]
        public IEnumerable<Message> MessageList()
        {
            return _dbContext.Messages.ToList();
        }

        [HttpGet("messages")]
        public async Task<IActionResult> Messages()
        {
            var messages = await _dbContext.Messages.ToListAsync();
            return Ok(messages);
        }

        [HttpGet("json")]
        public async Task<IActionResult> Json()
        {
            var messages = await _dbContext.Messages.ToListAsync();
            var response = JsonSerializer.Serialize<List<Message>>(messages);
            return Ok(messages);
        }

        [HttpGet("people")]
        public List<Person> People() 
        {
            return new List<Person>
            {
                new Person(44, "Mobum"),
                new Person(46, "Mose"),
                new Person(39, "Eunmi"),
                new Person(8, "Loc Shin"),
            };
        }
        public class Person
        {
            public Person(int age, string name)
            {
                Age = age;
                Name = name;
            }
            public int Age { get; set; }
            public string Name { get; set; }

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

        [HttpPost("register")]
        public async Task<ActionResult> RegisterAsync([FromBody]RegisterInfo registerInfo)
        {
            // TODO: implement AutoMapper registerInfo -> User
            var user = new User
            {
                Email = registerInfo.Email,
                UserType = UserType.Staff,
                CreatedAt = DateTime.Now
            };

            try
            {
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
            var secretKey = _configuration["SecurityKey"];

            if (user == null)
                return BadRequest();

            loginInfo.DeviceId ??= "deviceId";
            user.Name ??= "name here";

            var token = user.JwtToken(loginInfo, secretKey);
            var response = new { Token = token, RoomId = "room id here" };

            return Ok(response);
        }


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

            var token = user.JwtToken(loginInfo, secretKey);
            var response = new LoginResponse { token = token, roomId = roomId, userId = user.Id };

            return Ok(response);
        }

    }
}
