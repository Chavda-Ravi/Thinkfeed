using Microsoft.EntityFrameworkCore;
using Thinkfeed.Data;
using Thinkfeed.Models;
using Thinkfeed.Repositories.Interfaces;

namespace Thinkfeed.Repositories
{
    public class FollowRepository : IFollowRepository
    {
        private readonly ApplicationDbContext _context;

        public FollowRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Follow?> GetAsync(string followerId, string followingId)
        {
            return await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
        }

        public async Task AddAsync(Follow follow)
        {
            await _context.Follows.AddAsync(follow);
        }

        public void Remove(Follow follow)
        {
            _context.Follows.Remove(follow);
        }

        public async Task<int> CountFollowersAsync(string userId)
        {
            return await _context.Follows.CountAsync(f => f.FollowingId == userId);
        }

        public async Task<int> CountFollowingAsync(string userId)
        {
            return await _context.Follows.CountAsync(f => f.FollowerId == userId);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
