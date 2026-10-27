using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Thinkfeed.Models;
using Thinkfeed.Repositories.Interfaces;
using Thinkfeed.Services.Interfaces;

namespace Thinkfeed.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IBlogRepository _blogRepository;
        private readonly IFollowRepository _followRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public ProfileService(
            IBlogRepository blogRepository,
            IFollowRepository followRepository,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env)
        {
            _blogRepository = blogRepository;
            _followRepository = followRepository;
            _userManager = userManager;
            _env = env;
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

        public async Task DeleteProfileImageAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            if (string.IsNullOrWhiteSpace(user.ProfileImage))
            {
                return;
            }

            var physicalPath = GetSafePhysicalPath(user.ProfileImage);
            if (!string.IsNullOrEmpty(physicalPath) && File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }

            user.ProfileImage = null;
            await _userManager.UpdateAsync(user);
        }

        private string? GetSafePhysicalPath(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return null;
            }

            string normalized = relativePath.Trim();
            normalized = normalized.Replace('\\', '/');
            if (normalized.StartsWith("~/"))
            {
                normalized = normalized.Substring(2);
            }
            if (normalized.StartsWith("/"))
            {
                normalized = normalized.Substring(1);
            }

            string rootPath = Path.GetFullPath(_env.WebRootPath);
            string candidatePath = Path.GetFullPath(Path.Combine(rootPath, normalized));

            if (!candidatePath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return candidatePath;
        }
    }
}
