namespace FixItUp.DTOs
{
    public class BidDTO
    {
        public int BidId { get; set; }
        public decimal Amount { get; set; }
        public int EstimatedHours { get; set; }

        public string WorkerName { get; set; }
        public int TrustScore { get; set; }

        public string CompetitivenessScore { get; set; }
        public bool IsTopRated { get; set; }
    }

}
