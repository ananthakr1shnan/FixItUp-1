using FixItUp.Data;
using FixItUp.Models;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Register(User user)
        {
            _db.Users.Add(user);
            _db.SaveChanges();
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
