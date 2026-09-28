using Microsoft.EntityFrameworkCore;
using Thinkfeed.Data;
using Thinkfeed.Models;
using Thinkfeed.Repositories.Interfaces;

namespace Thinkfeed.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ApplicationDbContext _context;

        public CommentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
        }

        public async Task<Comment?> GetByIdAsync(int id)
        {
            return await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == id);
        }

        public void Remove(Comment comment)
        {
            _context.Comments.Remove(comment);
        }

        public async Task<List<Comment>> GetByBlogIdAsync(int blogPostId)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Where(c => c.BlogPostId == blogPostId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
