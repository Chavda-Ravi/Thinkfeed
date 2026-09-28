using System.Threading.Tasks;

namespace Thinkfeed.Services.Interfaces
{
    public interface IFollowService
    {
        Task ToggleAsync(string followerId, string followingId);
        Task<int> GetFollowerCountAsync(string userId);
        Task<int> GetFollowingCountAsync(string userId);
    }
}