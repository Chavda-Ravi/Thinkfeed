using System.ComponentModel.DataAnnotations;

namespace Thinkfeed.Models
{
    public class Comment
    {
        public int CommentId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int BlogPostId { get; set; }

        public BlogPost? BlogPost { get; set; }

        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }
    }
}