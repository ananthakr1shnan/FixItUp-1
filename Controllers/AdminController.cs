using FixItUp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace FixItUp.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AdminController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("users")]
        public IActionResult GetAllUsers()
        {
            var users = _db.Users.Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.Phone,
                u.Role,
                u.State,
                u.City,
                u.TrustScore,
                u.JobCompletionRate,
                u.IsActive,
                u.CreatedAt,
                JobsCompleted = _db.Tasks.Count(t => t.AssignedWorkerId == u.Id && t.Status == "Completed"),
                TasksPosted = _db.Tasks.Count(t => t.CustomerId == u.Id),
                Rating = u.TrustScore / 20.0, // Convert 0-100 to 0-5 scale
                OnTimeRate = u.JobCompletionRate,
                Earnings = 0, // Can be calculated from completed tasks
                TotalSpent = 0, // Can be calculated from user's completed tasks
                CompletedTasks = _db.Tasks.Count(t => t.CustomerId == u.Id && t.Status == "Completed")
            }).ToList();

            return Ok(users);
        }

        [HttpGet("monitor")]
        public IActionResult Monitor()
        {
            return Ok(new
            {
                ActiveTasks = _db.Tasks.Count(t => t.Status != "Completed"),
                Disputes = _db.Disputes.Count(d => d.Status == "Open")
            });
        }

        [HttpPost("resolve-dispute")]
        public IActionResult Resolve(int disputeId, bool releaseToWorker)
        {
            var dispute = _db.Disputes.Find(disputeId);
            if (dispute == null) return NotFound();

            dispute.Status = "Resolved";
            _db.SaveChanges();

            return Ok();
        }
    }
}
