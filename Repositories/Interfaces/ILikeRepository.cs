using System.Threading.Tasks;
using Thinkfeed.Models;

namespace Thinkfeed.Repositories.Interfaces
{
    public interface ILikeRepository
    {
        Task<Like?> GetByBlogAndUserAsync(int blogPostId, string userId);
        Task AddAsync(Like like);
        void Remove(Like like);
        Task<int> CountByBlogAsync(int blogPostId);
        Task SaveAsync();
    }
}