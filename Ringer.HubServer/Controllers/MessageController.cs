using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ringer.Core.Data;
using Ringer.Core.Models;
using Ringer.HubServer.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Ringer.HubServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MessageController : Controller
    {
        private readonly RingerDbContext _dbContext;
        private readonly ILogger<MessageController> _logger;


        public MessageController(RingerDbContext dbContext, ILogger<MessageController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        // GET: api/values
        [HttpGet("pending")]
        public async Task<ActionResult<string>> Get(string roomId, int lastNumber = 0)
        {
            var messages = await _dbContext.Messages
                .Where(m => m.RoomId == roomId && m.Id > lastNumber)
                .Include(m => m.Sender)
                .Select(m => new PendingMessage
                {
                    Id = m.Id,
                    Body = m.Body,
                    CreatedAt = m.CreatedAt,
                    SenderName = m.Sender.Name,
                    SenderId = m.SenderId,
                })
                .ToListAsync();

            var response = JsonSerializer.Serialize<List<PendingMessage>>(messages);

            return Ok(response);
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
