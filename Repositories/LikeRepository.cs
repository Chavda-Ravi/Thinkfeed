using Microsoft.EntityFrameworkCore;
using Thinkfeed.Data;
using Thinkfeed.Models;
using Thinkfeed.Repositories.Interfaces;

namespace Thinkfeed.Repositories
{
    public class LikeRepository : ILikeRepository
    {
        private readonly ApplicationDbContext _context;

        public LikeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Like?> GetByBlogAndUserAsync(int blogPostId, string userId)
        {
            return await _context.Likes
                .FirstOrDefaultAsync(l => l.BlogPostId == blogPostId && l.UserId == userId);
        }

        public async Task AddAsync(Like like)
        {
            await _context.Likes.AddAsync(like);
        }

        public void Remove(Like like)
        {
            _context.Likes.Remove(like);
        }

        public async Task<int> CountByBlogAsync(int blogPostId)
        {
            return await _context.Likes.CountAsync(l => l.BlogPostId == blogPostId);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
