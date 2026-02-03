namespace FixItUp.DTOs
{
    public class PaymentSummaryDTO
    {
        public decimal AvailableBalance { get; set; }
        public decimal PendingClearance { get; set; }
        public decimal TotalEarned { get; set; }
    }

}
