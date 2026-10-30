using Thinkfeed.Models;

namespace Thinkfeed.Repositories.Interfaces
{
    public interface IBookmarkRepository
    {
        Task<Bookmark?> GetByBlogAndUserAsync(int blogPostId, string userId);
        Task<List<BlogPost>> GetByUserAsync(string userId);
        Task AddAsync(Bookmark bookmark);
        void Remove(Bookmark bookmark);
        Task SaveAsync();
    }
}
