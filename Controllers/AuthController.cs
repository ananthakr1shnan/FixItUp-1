using FixItUp.DTOs;
using FixItUp.Data;
using FixItUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace FixItUp.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AuthController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterUserDTO request)
        {
            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                Role = request.Role,
                State = request.State,
                City = request.City,
                PasswordHash = request.Password // In real world, hash this!
            };

            _db.Users.Add(user);
            _db.SaveChanges(); // Save to generate Id

            if (request.Role == "Worker" && request.CategoryIds != null && request.CategoryIds.Any())
            {
                foreach (var catId in request.CategoryIds)
                {
                    _db.WorkerSkills.Add(new WorkerSkill
                    {
                        UserId = user.Id,
                        CategoryId = catId
                    });
                }
                _db.SaveChanges();
            }

            return Ok();
        }

        [HttpPost("login")]
        public IActionResult Login(string email, string password)
        {
            var user = _db.Users.FirstOrDefault(x => x.Email == email);
            if (user == null) return Unauthorized();

            return Ok(new { user.Id, user.Role });
        }
    }
}
