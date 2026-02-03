namespace FixItUp.Models
{
    public class Bid
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int WorkerId { get; set; }

        public decimal Amount { get; set; }
        public int EstimatedHours { get; set; }

        public DateTime CreatedAt { get; set; }
    }

}
