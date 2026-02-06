namespace FixItUp.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int CustomerId { get; set; }
        public int WorkerId { get; set; }

        public int Rating { get; set; } // 1-5 stars
        public string Comment { get; set; } = string.Empty;
        
        public bool IsVerified { get; set; } = true; // Verified if task was completed
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual TaskEntity Task { get; set; }
        public virtual User Customer { get; set; }
        public virtual User Worker { get; set; }
    }
}
