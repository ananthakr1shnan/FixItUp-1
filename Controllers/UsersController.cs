using FixItUp.Data;
using FixItUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace FixItUp.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;

        public UsersController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("{id}/profile")]
        public IActionResult GetProfile(int id)
        {
            var user = _db.Users
                .Include(u => u.SpecializedSkills)
                .ThenInclude(ws => ws.Category)
                .FirstOrDefault(u => u.Id == id);

            if (user == null) return NotFound();

            // Transform if needed or return user with skills
            // To prevent cycle, might need DTO, but for now relying on ReferenceLoopHandling=Ignore if configured,
            // or projecting strictly. Let's project to simple object to be safe.

            var response = new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Role,
                user.State,
                user.City,
                user.TrustScore,
                user.JobCompletionRate,
                user.IsTopRated,
                user.IsVerifiedPro,
                SpecializedSkills = user.SpecializedSkills.Select(s => new { s.Category.Id, s.Category.Name }).ToList()
            };

            return Ok(response);
        }
    }
}
