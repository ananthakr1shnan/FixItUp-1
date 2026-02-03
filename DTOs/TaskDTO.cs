namespace FixItUp.DTOs
{
    public class TaskDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }

        public bool IsUrgent { get; set; }
        public string Location { get; set; }

        public string TimeAgo { get; set; }
        public double DistanceInMiles { get; set; }
    }

}
