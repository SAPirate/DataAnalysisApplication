using Microsoft.EntityFrameworkCore;
using practiceApp.Models;

namespace practiceApp.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
                
        }
        public DbSet<CategoryModel> categoryModels { get; set; }
        public DbSet<DataUpload> DataUploads { get; set; }

    }
}
