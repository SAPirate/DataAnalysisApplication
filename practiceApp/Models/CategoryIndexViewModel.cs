namespace practiceApp.Models
{
    public class CategoryIndexViewModel
    {
        public List<CategoryModel> ActiveCategories { get; set; }
        public List<CategoryModel> DeletedCategories { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
