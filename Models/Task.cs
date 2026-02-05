namespace FixItUp.Models
{
    public class TaskEntity
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int? AssignedWorkerId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        // Location fields
        public string State { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty; // Additional address/landmark
        public decimal MinBudget { get; set; }
        public decimal MaxBudget { get; set; }

        public bool IsUrgent { get; set; }
        public string Status { get; set; } = "Posted";

        public string BeforeImageURL { get; set; } = string.Empty;
        public string AfterImageURL { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        
        public int? AcceptedBidId { get; set; }
        public virtual Bid? AcceptedBid { get; set; }
    }
}
