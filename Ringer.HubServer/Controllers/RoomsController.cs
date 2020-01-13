using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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

        [HttpGet("hello")]
        public async Task<ActionResult> GetAll()
        {
            await Task.Delay(1);

            var response = new Tester
            {
                HelloWorld = "world",
                WelcomeToRinger = "nice to meet you"
            };

            return Ok(response);
        }

        [HttpGet("content")]
        public ContentResult GetContent()
        {
            return Content("An API listing authors of docs.asp.net.");
        }

        [HttpGet("version")]
        public string Version()
        {
            return "Version 1.0.0";
        }

        // GET api/authors/RickAndMSFT
        [HttpGet("{alias}")]
        [Authorize]
        public async Task<Author> Get()
        {
            var user = User;

            await hubContext.Clients.All.SendAsync("ReceiveMessage", "알리아스", "외부에서 호출했음", 10, 11, DateTime.Now);
            return new Author{ Id = 1, Name = "mike"};
        }
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