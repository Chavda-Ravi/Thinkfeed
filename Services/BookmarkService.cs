using Thinkfeed.Models;
using Thinkfeed.Repositories.Interfaces;
using Thinkfeed.Services.Interfaces;

namespace Thinkfeed.Services
{
    public class BookmarkService : IBookmarkService
    {
        private readonly IBookmarkRepository _bookmarkRepository;

        public BookmarkService(IBookmarkRepository bookmarkRepository)
        {
            _bookmarkRepository = bookmarkRepository;
        }

        public async Task ToggleAsync(int blogPostId, string userId)
        {
            var existing = await _bookmarkRepository.GetByBlogAndUserAsync(blogPostId, userId);

            if (existing != null)
            {
                _bookmarkRepository.Remove(existing);
            }
            else
            {
                await _bookmarkRepository.AddAsync(new Bookmark
                {
                    BlogPostId = blogPostId,
                    UserId = userId,
                    CreatedAt = DateTime.Now
                });
            }

            await _bookmarkRepository.SaveAsync();
        }

        public Task<List<BlogPost>> GetSavedPostsAsync(string userId)
        {
            return _bookmarkRepository.GetByUserAsync(userId);
        }

        public async Task<bool> IsBookmarkedAsync(int blogPostId, string userId)
        {
            return await _bookmarkRepository.GetByBlogAndUserAsync(blogPostId, userId) != null;
        }
    }
}
