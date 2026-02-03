using FixItUp.Data;
using Microsoft.AspNetCore.Mvc;
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
