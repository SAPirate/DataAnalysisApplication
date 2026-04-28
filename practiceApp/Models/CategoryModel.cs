using System.ComponentModel.DataAnnotations;

namespace practiceApp.Models
{
    public class CategoryModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [System.ComponentModel.DisplayName("User Name")]
        public string Username { get; set; }
        [System.ComponentModel.DisplayName("Display Order")]
        [Range(1,100,ErrorMessage ="Display order must be between 1 and 100 only!")]
        public int DisplayOrder { get; set; }
        public DateTime CreatedDateTime { get; set; } = DateTime.Now;

        public bool IsDeleted { get; set; } = false;

        public bool WasDeleted { get; set; } = false;

        public int RestoreCount { get; set; } = 0;

        public DateTime? UpdatedDateTime { get; set; }  // Nullable, because it may not always be set
        public DateTime? DeletedDateTime { get; set; }



    }
}
