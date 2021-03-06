﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ringer.Core.Data;
using Ringer.HubServer.Data;
using Ringer.HubServer.Hubs;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Ringer.HubServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MessageController : Controller
    {
        private readonly RingerDbContext _dbContext;
        private readonly ILogger<MessageController> _logger;
        private readonly IHubContext<ChatHub> _hubContext;

        public MessageController(RingerDbContext dbContext, ILogger<MessageController> logger, IHubContext<ChatHub> hubContext)
        {
            _dbContext = dbContext;
            _logger = logger;
            _hubContext = hubContext;
        }

        [Authorize]
        [HttpGet("segmented-messages")]
        public async Task<ActionResult> GetSegmentedMessages(string roomId, string skip, string take)
        {
            var messages = await _dbContext.Messages
                .Where(m => m.RoomId == roomId)
                .Include(m => m.Sender)
                .OrderByDescending(m => m.Id)
                .Skip(int.Parse(skip))
                .Take(int.Parse(take))
                .Select(m => new PendingMessage
                {
                    Id = m.Id,
                    Body = m.Body,
                    CreatedAt = m.CreatedAt,
                    SenderId = m.SenderId,
                    SenderName = m.Sender.Name
                })
                .ToListAsync();

            return Ok(JsonSerializer.Serialize(messages));
        }

        [Authorize]
        [HttpGet("pending")]
        public async Task<ActionResult<string>> GetPendings(string roomId, int lastId = 0)
        {
            _logger.LogWarning($"room: {roomId}, lastId: {lastId}");

            var messages = await _dbContext.Messages
                .Where(m => m.RoomId == roomId && m.Id > lastId)
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

            var response = JsonSerializer.Serialize(messages);

            return Ok(response);
        }

        static int messageId = 10000;

        [HttpPost("report")]
        public async Task<IActionResult> Report(string userId)
        {
            var userProxy = _hubContext.Clients.User(userId);

            await userProxy.SendAsync("ReceiveMessage", "hub", "call from report api", messageId++, 7, DateTime.UtcNow, null);

            return Ok();
        }
    }
}
