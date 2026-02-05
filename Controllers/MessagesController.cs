using FixItUp.Data;
using FixItUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FixItUp.Controllers
{
    [ApiController]
    [Route("api/messages")]
    public class MessagesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public MessagesController(AppDbContext db)
        {
            _db = db;
        }

        // Get conversation between two users for a specific task
        [HttpGet("conversation")]
        public IActionResult GetConversation(int taskId, int userId1, int userId2)
        {
            var messages = _db.Messages
                .Where(m => m.TaskId == taskId &&
                           ((m.SenderId == userId1 && m.ReceiverId == userId2) ||
                            (m.SenderId == userId2 && m.ReceiverId == userId1)))
                .OrderBy(m => m.CreatedAt)
                .ToList();

            return Ok(messages);
        }

        // Get all conversations for a user
        [HttpGet("user/{userId}")]
        public IActionResult GetUserConversations(int userId)
        {
            var conversations = _db.Messages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .GroupBy(m => m.TaskId)
                .Select(g => new
                {
                    TaskId = g.Key,
                    LastMessage = g.OrderByDescending(m => m.CreatedAt).FirstOrDefault(),
                    UnreadCount = g.Count(m => m.ReceiverId == userId && !m.IsRead)
                })
                .ToList();

            // Enrich with task and other user info
            var enrichedConversations = conversations.Select(c =>
            {
                var task = _db.Tasks.Find(c.TaskId);
                var lastMessage = c.LastMessage;
                var otherUserId = lastMessage != null && lastMessage.SenderId == userId 
                    ? lastMessage.ReceiverId 
                    : (lastMessage?.SenderId ?? 0);
                var otherUser = _db.Users.Find(otherUserId);

                return new
                {
                    c.TaskId,
                    TaskTitle = task?.Title ?? "",
                    OtherUser = new
                    {
                        Id = otherUser?.Id ?? 0,
                        Name = otherUser?.FullName ?? "",
                        Role = otherUser?.Role ?? ""
                    },
                    LastMessage = lastMessage?.Content ?? "",
                    LastMessageTime = lastMessage?.CreatedAt ?? DateTime.Now,
                    UnreadCount = c.UnreadCount
                };
            }).ToList();

            return Ok(enrichedConversations);
        }

        // Send a message
        [HttpPost("send")]
        public IActionResult SendMessage([FromBody] Message message)
        {
            if (message == null)
                return BadRequest("Message data is required");

            message.CreatedAt = DateTime.UtcNow;
            message.IsRead = false;

            _db.Messages.Add(message);
            _db.SaveChanges();

            return Ok(message);
        }

        // Mark messages as read
        [HttpPost("mark-read")]
        public IActionResult MarkAsRead(int taskId, int userId)
        {
            var messages = _db.Messages
                .Where(m => m.TaskId == taskId && m.ReceiverId == userId && !m.IsRead)
                .ToList();

            foreach (var msg in messages)
            {
                msg.IsRead = true;
            }

            _db.SaveChanges();
            return Ok(new { count = messages.Count });
        }
    }
}
