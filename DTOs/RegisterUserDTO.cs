using System.ComponentModel.DataAnnotations;

namespace FixItUp.DTOs
{
    public class RegisterUserDTO
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; } // Customer, Worker

        // For Workers only
        public List<int> CategoryIds { get; set; } = new List<int>();
    }
}
