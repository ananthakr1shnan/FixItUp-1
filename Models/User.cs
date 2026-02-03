namespace FixItUp.Models
    {
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } // Customer, Worker, Admin

        public bool IsAcceptingJobs { get; set; }

        public int TrustScore { get; set; }
        public double JobCompletionRate { get; set; }
        public double OnTimeArrivalRate { get; set; }
        public int AvgResponseTime { get; set; }

        public bool IsTopRated { get; set; }
        public bool IsVerifiedPro { get; set; }
        public bool IsFastBidder { get; set; }

        public decimal AvailableBalance { get; set; }
        public decimal PendingClearance { get; set; }
    }

}
