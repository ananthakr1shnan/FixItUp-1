namespace FixItUp.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int CustomerId { get; set; }
        public int WorkerId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Completed, Released
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public string? TransactionId { get; set; }
        
        // Navigation properties
        public virtual TaskEntity Task { get; set; }
        public virtual User Customer { get; set; }
        public virtual User Worker { get; set; }
    }
}
