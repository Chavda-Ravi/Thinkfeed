using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thinkfeed.Data;
using Thinkfeed.Models;
using Thinkfeed.Services.Interfaces;

namespace Thinkfeed.Services
{
    public class SearchService : ISearchService
    {
        private readonly ApplicationDbContext _context;

        public SearchService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<List<BlogPost>> SearchAsync(string? query, int? categoryId, string? username)
        {
            var blogs = _context.BlogPosts
                .Include(b => b.User)
                .Include(b => b.Category)
                .Include(b => b.Images)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                blogs = blogs.Where(b => b.Title.Contains(query) || b.Article.Contains(query));
            }

            if (categoryId.HasValue)
            {
                blogs = blogs.Where(b => b.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(username))
            {
                blogs = blogs.Where(b => b.User != null && (b.User.UserName!.Contains(username) || b.User.FullName!.Contains(username)));
            }

            return await blogs.OrderByDescending(b => b.CreatedAt).ToListAsync();
        }
    }
}
