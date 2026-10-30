using Microsoft.EntityFrameworkCore;
using Thinkfeed.Data;
using Thinkfeed.Models;
using Thinkfeed.Repositories.Interfaces;

namespace Thinkfeed.Repositories
{
    public class BookmarkRepository : IBookmarkRepository
    {
        private readonly ApplicationDbContext _context;

        public BookmarkRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Bookmark?> GetByBlogAndUserAsync(int blogPostId, string userId)
        {
            return await _context.Bookmarks
                .FirstOrDefaultAsync(b => b.BlogPostId == blogPostId && b.UserId == userId);
        }

        public async Task<List<BlogPost>> GetByUserAsync(string userId)
        {
            return await _context.Bookmarks
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .Include(b => b.BlogPost!)
                    .ThenInclude(p => p.User)
                .Include(b => b.BlogPost!)
                    .ThenInclude(p => p.Category)
                .Include(b => b.BlogPost!)
                    .ThenInclude(p => p.Images)
                .Select(b => b.BlogPost!)
                .ToListAsync();
        }

        public async Task AddAsync(Bookmark bookmark)
        {
            await _context.Bookmarks.AddAsync(bookmark);
        }

        public void Remove(Bookmark bookmark)
        {
            _context.Bookmarks.Remove(bookmark);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
