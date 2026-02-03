namespace FixItUp.Models
{
    public class TaskChecklistItem
    {
        public int Id { get; set; }
        public int TaskId { get; set; }

        public string TaskItem { get; set; }
        public bool IsDone { get; set; }
    }

}
