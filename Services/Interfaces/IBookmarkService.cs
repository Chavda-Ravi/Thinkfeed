using Thinkfeed.Models;

namespace Thinkfeed.Services.Interfaces
{
    public interface IBookmarkService
    {
        Task ToggleAsync(int blogPostId, string userId);
        Task<List<BlogPost>> GetSavedPostsAsync(string userId);
        Task<bool> IsBookmarkedAsync(int blogPostId, string userId);
    }
}
