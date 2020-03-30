using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ringer.Core.Data;
using Ringer.Core.Models;
using Ringer.HubServer.Data;
using Ringer.HubServer.Hubs;

namespace Ringer.HubServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly RingerDbContext dbContext;
        private readonly ILogger<RoomsController> logger;
        private readonly IHubContext<ChatHub> hubContext;

        public RoomsController(RingerDbContext dbContext, ILogger<RoomsController> logger, IHubContext<ChatHub> hubContext)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            this.hubContext = hubContext;
        }

        [Authorize]
        [HttpGet("list")]
        public async Task<ActionResult<string>> GetListAsync()
        {
            List<Room> rooms = await dbContext.Rooms.ToListAsync();
            var response = JsonSerializer.Serialize(rooms);

            return Ok(response);
        }

        [Authorize]
        [HttpGet("room-with-informations")]
        public async Task<ActionResult> GetListWithInfoAsync()
        {
            List<Room> rooms = await dbContext.Rooms.ToListAsync();

            var roomInfos = new List<RoomInformation>();

            foreach (Room room in rooms)
            {
                var lastMessage = await dbContext.Messages
                    .Where(m => m.RoomId == room.Id)
                    .OrderByDescending(m => m.Id)
                    .FirstOrDefaultAsync();
                roomInfos.Add(new RoomInformation { Room = room, LastMessage = lastMessage });
            }

            return Ok(roomInfos);
        }

        [Authorize]
        [HttpGet("name")]
        public async Task<ActionResult<string>> GetRoomNameById(string roomId)
        {
            var room = await dbContext.Rooms.FirstOrDefaultAsync(r => r.Id == roomId);

            return Ok(room?.Name);
        }



        //[Authorize]
        //[HttpGet]
        //public async Task<ActionResult> GetAll()
        //{
        //    var id = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        //    var name = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        //    var deviceId = User?.Claims?.FirstOrDefault(c => c.Type == "DeviceId")?.Value;

        //    var user = await dbContext.Users
        //        .Include(u => u.Enrollments)
        //        .FirstOrDefaultAsync(u => u.Id == int.Parse(id));

        //    var roomList = new List<string>();
        //    foreach (var enrollment in user.Enrollments)
        //        roomList.Add(enrollment.RoomId);

        //    var response = new
        //    {
        //        Id = id,
        //        Name = name,
        //        DeviceId = deviceId,
        //        Rooms = roomList
        //    };

        //    return Ok(response);
        //}


        //[Authorize]
        //[HttpGet("content")]
        //public ContentResult GetContent()
        //{
        //    return Content("An API listing authors of docs.asp.net.");
        //}

        //[HttpGet("version")]
        //public string Version()
        //{
        //    return "Version 1.0.0";
        //}

        // GET api/authors/RickAndMSFT
        //[HttpGet("{body}")]
        //[Authorize]
        //public async Task<Author> Get(string body, string roomId = "a0ed073a-fec6-4c5e-9a20-6c1ff55d7f44")
        //{
        //    /*
        //        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Context.UserIdentifier
        //        new Claim(ClaimTypes.Name, user.Name),
        //        new Claim("DeviceId", loginInfo.DeviceId),
        //        new Claim("DeviceType", loginInfo.DeviceType.ToString())
        //    */

        //    var id = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        //    var name = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        //    var deviceId = User?.Claims?.FirstOrDefault(c => c.Type == "DeviceId")?.Value;

        //    var userId = int.Parse(id);

        //    var user = await dbContext.Users.FindAsync(userId);
        //    Message message = new Message
        //    {
        //        Body = body,
        //        CreatedAt = DateTime.UtcNow,
        //        RoomId = roomId,
        //        SenderId = userId
        //    };

        //    dbContext.Messages.Add(message);
        //    await dbContext.SaveChangesAsync();

        //    await hubContext.Clients.Group(roomId).SendAsync("ReceiveMessage", name, body, message.Id, user.Id, message.CreatedAt, message.Id);
        //    return new Author { Id = 1, Name = "mike" };
        //}
    }

    public class Author
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Tester
    {
        public string HelloWorld { get; set; }
        public string WelcomeToRinger { get; set; }

    }
}