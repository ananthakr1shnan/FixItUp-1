using FixItUp.Data;
using FixItUp.Models; // Assuming Dispute is in Models
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FixItUp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DisputesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public DisputesController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public IActionResult CreateDispute(Dispute dispute)
        {
            // Set initial status and dates
            dispute.CreatedAt = DateTime.UtcNow;
            dispute.Status = "Open";

            _db.Disputes.Add(dispute);
            _db.SaveChanges();

            return Ok(new { message = "Dispute reported successfully", disputeId = dispute.Id });
        }

        [HttpGet("{id}")]
        public IActionResult GetDispute(int id)
        {
            var dispute = _db.Disputes.Find(id);
            if (dispute == null) return NotFound();
            return Ok(dispute);
        }
        
        [HttpGet("user/{userId}")]
        public IActionResult GetUserDisputes(int userId)
        {
            var disputes = _db.Disputes
                .Where(d => _db.Tasks.Any(t => t.Id == d.TaskId && (t.CustomerId == userId || t.AssignedWorkerId == userId)))
                .Select(d => new
                {
                    d.Id,
                    d.TaskId,
                    d.RaisedByRole,
                    d.Type,
                    d.Description,
                    d.Status,
                    d.CreatedAt,
                    TaskTitle = _db.Tasks.Where(t => t.Id == d.TaskId).Select(t => t.Title).FirstOrDefault()
                })
                .OrderByDescending(d => d.CreatedAt)
                .ToList();

            return Ok(disputes);
        }
    }
}
