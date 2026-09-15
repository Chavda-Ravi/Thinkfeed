namespace Thinkfeed.Models
{
    public class Follow
    {
        public int FollowId { get; set; }

        public string FollowerId { get; set; } = string.Empty;

        public ApplicationUser? Follower { get; set; }

        public string FollowingId { get; set; } = string.Empty;

        public ApplicationUser? Following { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}