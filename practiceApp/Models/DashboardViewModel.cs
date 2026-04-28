namespace practiceApp.Models
{
    public class DashboardViewModel
    {
        public int TotalCategories { get; set; }
        public int DeletedCategories { get; set; }
        public int CurrentCategories { get; set; }
        public int TotalRestores { get; set; }
        public string MostRestoredUsername { get; set; }
        public int MostRestoredCount { get; set; }
    }
}
