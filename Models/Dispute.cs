namespace FixItUp.Models
{
    public class Dispute
    {
        public int Id { get; set; }
        public int TaskId { get; set; }

        public string RaisedByRole { get; set; } // Customer / Worker
        public string Type { get; set; } // Damage, Quality, Delay
        public string Description { get; set; }
        public string EvidenceUrl { get; set; }

        public string Status { get; set; } // Open, Resolved
        public DateTime CreatedAt { get; set; }
    }
}
