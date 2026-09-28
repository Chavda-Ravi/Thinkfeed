using System.Collections.Generic;
using System.Threading.Tasks;
using Thinkfeed.Models;
using Thinkfeed.Repositories.Interfaces;
using Thinkfeed.Services.Interfaces;

namespace Thinkfeed.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IBlogRepository _blogRepository;
        private readonly IFollowRepository _followRepository;

        public ProfileService(IBlogRepository blogRepository, IFollowRepository followRepository)
        {
            _blogRepository = blogRepository;
            _followRepository = followRepository;
        }

        public async Task<List<BlogPost>> GetUserBlogsAsync(string userId)
        {
            return await _blogRepository.GetByUserIdAsync(userId);
        }

        public async Task<int> GetBlogCountAsync(string userId)
        {
            var blogs = await _blogRepository.GetByUserIdAsync(userId);
            return blogs.Count;
        }

        public async Task<int> GetFollowerCountAsync(string userId)
        {
            return await _followRepository.CountFollowersAsync(userId);
        }

        public async Task<int> GetFollowingCountAsync(string userId)
        {
            return await _followRepository.CountFollowingAsync(userId);
        }

        public async Task<bool> IsFollowingAsync(string currentUserId, string targetUserId)
        {
            if (string.IsNullOrEmpty(currentUserId)) return false;
            var f = await _followRepository.GetAsync(currentUserId, targetUserId);
            return f != null;
        }
    }
}
