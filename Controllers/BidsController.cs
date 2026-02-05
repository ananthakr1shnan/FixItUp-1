using FixItUp.Data;
using FixItUp.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace FixItUp.Controllers
{
    [ApiController]
    [Route("api/bids")]
    public class BidsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public BidsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("place")]
        public IActionResult Place(Bid bid)
        {
            var workerBusy = _db.Tasks.Any(t =>
                t.AssignedWorkerId == bid.WorkerId &&
                (t.Status == "Accepted" || t.Status == "InProgress"));

            if (workerBusy)
                return BadRequest("Worker already busy");

            // Check if worker has the required skill
            var taskCategory = _db.Tasks
                .Where(t => t.Id == bid.TaskId)
                .Select(t => t.Category)
                .FirstOrDefault();

            if (taskCategory != null)
            {
                var hasSkill = _db.WorkerSkills
                    .Any(ws => ws.UserId == bid.WorkerId && ws.Category.Name == taskCategory);

                if (!hasSkill)
                    return BadRequest("Worker does not have the required skill for this task.");
            }

            bid.CreatedAt = DateTime.UtcNow;
            _db.Bids.Add(bid);
            _db.SaveChanges();

            return Ok();
        }

        [HttpPost("{bidId}/accept")]
        public IActionResult Accept(int bidId)
        {
            var bid = _db.Bids.Find(bidId);
            if (bid == null) return NotFound();

            var task = _db.Tasks.Find(bid.TaskId);
            if (task == null) return NotFound();

            task.AssignedWorkerId = bid.WorkerId;
            task.AcceptedBidId = bidId; // Store the accepted bid ID
            task.Status = "Accepted";

            var worker = _db.Users.Find(bid.WorkerId);
            worker.PendingClearance += bid.Amount;

            _db.SaveChanges();
            return Ok();
        }
    }
}
