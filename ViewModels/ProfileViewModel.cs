using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Thinkfeed.ViewModels
{
    public class ProfileViewModel
    {
        public string? UserName { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Bio { get; set; }

        public string? ProfileImage { get; set; }

        public IFormFile? Image { get; set; }
    }
}