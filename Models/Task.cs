namespace FixItUp.Models
{
    public class TaskEntity
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int? AssignedWorkerId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }

        public string Location { get; set; }
        public decimal MinBudget { get; set; }
        public decimal MaxBudget { get; set; }

        public bool IsUrgent { get; set; }
        public string Status { get; set; } // Posted, Bidding, Accepted, InProgress, Completed, UnderDispute

        public string BeforeImageURL { get; set; }
        public string AfterImageURL { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
