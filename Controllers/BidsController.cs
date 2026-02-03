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
            task.Status = "Accepted";

            var worker = _db.Users.Find(bid.WorkerId);
            worker.PendingClearance += bid.Amount;

            _db.SaveChanges();
            return Ok();
        }
    }
}
