using System;
using System.Threading.Tasks;
using Thinkfeed.Models;
using Thinkfeed.Repositories.Interfaces;
using Thinkfeed.Services.Interfaces;

namespace Thinkfeed.Services
{
    public class LikeService : ILikeService
    {
        private readonly ILikeRepository _likeRepository;

        public LikeService(ILikeRepository likeRepository)
        {
            _likeRepository = likeRepository;
        }

        public async Task ToggleAsync(int blogPostId, string userId)
        {
            var existing = await _likeRepository.GetByBlogAndUserAsync(blogPostId, userId);

            if (existing != null)
            {
                _likeRepository.Remove(existing);
            }
            else
            {
                var like = new Like
                {
                    BlogPostId = blogPostId,
                    UserId = userId,
                    CreatedAt = DateTime.Now
                };

                await _likeRepository.AddAsync(like);
            }

            await _likeRepository.SaveAsync();
        }
    }
}
