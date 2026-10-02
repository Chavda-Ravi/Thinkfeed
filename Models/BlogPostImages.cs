namespace Thinkfeed.Models
{
    public class BlogPostImage
    {
        public int BlogPostImageId { get; set; }

        public string ImagePath { get; set; } = string.Empty;

        public int BlogPostId { get; set; }

        public BlogPost? BlogPost { get; set; }
    }
}