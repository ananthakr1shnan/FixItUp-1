namespace FixItUp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } // Customer, Worker, Admin
        public string Phone { get; set; } = string.Empty;

        // Location fields
        public string State { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;

        public bool IsAcceptingJobs { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int TrustScore { get; set; }
        public double JobCompletionRate { get; set; }
        public double OnTimeArrivalRate { get; set; }
        public int AvgResponseTime { get; set; }

        public bool IsTopRated { get; set; }
        public bool IsVerifiedPro { get; set; }
        public bool IsFastBidder { get; set; }

        public decimal AvailableBalance { get; set; }
        public decimal PendingClearance { get; set; }

        public virtual ICollection<WorkerSkill> SpecializedSkills { get; set; }
    }
}
