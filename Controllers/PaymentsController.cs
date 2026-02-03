using FixItUp.Data;
using FixItUp.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FixItUp.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public PaymentsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("summary")]
        public IActionResult Summary(int workerId)
        {
            var u = _db.Users.Find(workerId);
            if (u == null) return NotFound();

            return Ok(new PaymentSummaryDTO
            {
                AvailableBalance = u.AvailableBalance,
                PendingClearance = u.PendingClearance,
                TotalEarned = u.AvailableBalance + u.PendingClearance
            });
        }

        [HttpPost("release")]
        public IActionResult Release(int workerId, decimal amount)
        {
            var u = _db.Users.Find(workerId);
            if (u == null) return NotFound();

            u.PendingClearance -= amount;
            u.AvailableBalance += amount;

            _db.SaveChanges();
            return Ok();
        }
    }
}
