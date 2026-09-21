using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Thinkfeed.ViewModels
{
    public class BlogPostViewModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Article { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        public IFormFile? Image { get; set; }
    }
}