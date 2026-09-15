namespace Thinkfeed.Models
{
    public class Like
    {
        public int LikeId { get; set; }

        public int BlogPostId { get; set; }

        public BlogPost? BlogPost { get; set; }

        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}