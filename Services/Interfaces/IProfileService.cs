using System.Collections.Generic;
using System.Threading.Tasks;
using Thinkfeed.Models;

namespace Thinkfeed.Services.Interfaces
{
    public interface IProfileService
    {
        Task<List<BlogPost>> GetUserBlogsAsync(string userId);
        Task<int> GetBlogCountAsync(string userId);
        Task<int> GetFollowerCountAsync(string userId);
        Task<int> GetFollowingCountAsync(string userId);
        Task<bool> IsFollowingAsync(string currentUserId, string targetUserId);
    }
}