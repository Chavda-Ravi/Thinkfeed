using Microsoft.AspNetCore.Identity;

namespace Thinkfeed.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }

        public string? ProfileImage { get; set; }

        public string? Bio { get; set; }

        public ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public ICollection<Like> Likes { get; set; } = new List<Like>();

        public ICollection<Follow> Followers { get; set; } = new List<Follow>();

        public ICollection<Follow> Following { get; set; } = new List<Follow>();
    }
}