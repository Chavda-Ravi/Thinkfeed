using System.Threading.Tasks;
using Thinkfeed.Models;

namespace Thinkfeed.Repositories.Interfaces
{
    public interface IFollowRepository
    {
        Task<Follow?> GetAsync(string followerId, string followingId);
        Task AddAsync(Follow follow);
        void Remove(Follow follow);
        Task<int> CountFollowersAsync(string userId);
        Task<int> CountFollowingAsync(string userId);
        Task SaveAsync();
    }
}