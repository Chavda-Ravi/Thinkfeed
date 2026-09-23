using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Thinkfeed.Data;
using Thinkfeed.ViewModels;

namespace Thinkfeed.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? query,
            int? categoryId,
            string? username)
        {
            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            var blogs = _context.BlogPosts
                .Include(b => b.User)
                .Include(b => b.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                blogs = blogs.Where(b =>
                    b.Title.Contains(query) ||
                    b.Article.Contains(query));
            }

            if (categoryId.HasValue)
            {
                blogs = blogs.Where(b =>
                    b.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(username))
            {
                blogs = blogs.Where(b =>
                    b.User != null &&
                    (b.User.UserName!.Contains(username) ||
                     b.User.FullName!.Contains(username)));
            }

            var results = await blogs
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var model = new SearchViewModel
            {
                Query = query,
                CategoryId = categoryId,
                Username = username,
                Categories = categories,
                Results = results
            };

            return View(model);
        }
    }
}