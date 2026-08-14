namespace TaskFlow.API.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int ProjectId { get; set; }
        public int? DeveloperId { get; set; } // Boş bırakılabilir (Atanmamış olabilir)
        public int? TesterId { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }
}