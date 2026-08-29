namespace TaskFlow.API // Projenin namespace'i farklıysa orayı kendi projene göre bırak
{
    public class DashboardDto
    {
        public int ToplamProje { get; set; }
        public int TamamlananProje { get; set; }
        public int ToplamBug { get; set; }
    }
}