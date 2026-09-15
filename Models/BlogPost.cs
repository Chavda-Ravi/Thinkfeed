using System.ComponentModel.DataAnnotations;

namespace Thinkfeed.Models
{
    public class BlogPost
    {
        public int BlogPostId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Article { get; set; } = string.Empty;

        public string? ImagePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }

        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public ICollection<Like> Likes { get; set; } = new List<Like>();
    }
}