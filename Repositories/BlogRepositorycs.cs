using Microsoft.EntityFrameworkCore;
using Thinkfeed.Data;
using Thinkfeed.Models;
using Thinkfeed.Repositories.Interfaces;

namespace Thinkfeed.Repositories
{
    public class BlogRepository : IBlogRepository
    {
        private readonly ApplicationDbContext _context;

        public BlogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<BlogPost>> GetAllAsync()
        {
            return await _context.BlogPosts
                .Include(b => b.User)
                .Include(b => b.Category)
                .Include(b => b.Likes)
                .Include(b => b.Comments)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<BlogPost?> GetByIdAsync(int id)
        {
            return await _context.BlogPosts
                .Include(b => b.User)
                .Include(b => b.Category)
                .Include(b => b.Likes)
                .Include(b => b.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(b => b.BlogPostId == id);
        }

        public async Task<List<BlogPost>> GetByUserIdAsync(string userId)
        {
            return await _context.BlogPosts
                .Include(b => b.Category)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(BlogPost blogPost)
        {
            await _context.BlogPosts.AddAsync(blogPost);
        }

        public void Update(BlogPost blogPost)
        {
            _context.BlogPosts.Update(blogPost);
        }

        public void Delete(BlogPost blogPost)
        {
            _context.BlogPosts.Remove(blogPost);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}