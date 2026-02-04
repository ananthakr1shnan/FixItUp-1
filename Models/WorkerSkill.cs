namespace FixItUp.Models
{
    public class WorkerSkill
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public int CategoryId { get; set; }
        public ServiceCategory Category { get; set; }
    }
}
