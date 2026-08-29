namespace TaskFlow.API
{
    public class DeveloperDashboardDto
    {
        public int ToplamGorev { get; set; }
        public int TamamlananGorev { get; set; }
        public List<GorevDto> AtananGorevler { get; set; } = new();
        public List<BugDto> BugRaporlari { get; set; } = new();
        public List<YorumDto> GecmisYorumlar { get; set; } = new();
    }

    public class GorevDto
    {
        public int TaskId { get; set; }
        public string ProjectName { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }

    public class BugDto
    {
        public int TaskId { get; set; } // Buglar da Task tablosunda tutuluyorsa
        public string Description { get; set; }
        public string SenderName { get; set; }
    }

    public class YorumDto
    {
        public string SenderRole { get; set; }
        public string SenderName { get; set; }
        public string ProjectName { get; set; }
        public string Content { get; set; }
        public string TimeAgo { get; set; }
    }

    public class YorumEkleDto
    {
        public int TaskId { get; set; }
        public string Content { get; set; }
    }
}