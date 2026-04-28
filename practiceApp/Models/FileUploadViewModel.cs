using System.ComponentModel.DataAnnotations;

namespace practiceApp.Models
{
    public class FileUploadViewModel
    {
        [Required]
        [Display(Name = "Upload File")]
        public IFormFile File { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Send To Email")]
        public string SentToEmail { get; set; }
    }

}
