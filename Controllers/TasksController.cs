using FixItUp.Data;
using FixItUp.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace FixItUp.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _db;

        public TasksController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("create")]
        public IActionResult Create(TaskEntity task)
        {
            task.Status = "Posted";
            task.CreatedAt = DateTime.UtcNow;

            _db.Tasks.Add(task);
            _db.SaveChanges();

            return Ok(task);
        }

        [HttpGet("nearby")]
        public IActionResult Nearby()
        {
            return Ok(_db.Tasks.Where(t => t.Status == "Posted").ToList());
        }

        [HttpPatch("{id}/status")]
        public IActionResult UpdateStatus(int id, string status, string afterImageUrl)
        {
            var task = _db.Tasks.Find(id);
            if (task == null) return NotFound();

            if (status == "Completed" && string.IsNullOrEmpty(afterImageUrl))
                return BadRequest("Completion image required");

            task.Status = status;
            task.AfterImageURL = afterImageUrl;

            _db.SaveChanges();
            return Ok();
        }
    }
}
