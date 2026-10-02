using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Thinkfeed.ViewModels
{
    public class BlogPostViewModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Article { get; set; } = string.Empty;

        public List<IFormFile> Images { get; set; } = new List<IFormFile>();

        [Required]
        public int CategoryId { get; set; }
    }
}