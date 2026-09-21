using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Thinkfeed.Data;
using Thinkfeed.Models;
using Thinkfeed.ViewModels;

namespace Thinkfeed.Controllers
{
    [Authorize]
    public class BlogPostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BlogPostsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var blogs = await _context.BlogPosts
                .Include(b => b.User)
                .Include(b => b.Category)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(blogs);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BlogPostViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            string? imagePath = null;

            if (model.Image != null)
            {
                string uploadsFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "Images");

                Directory.CreateDirectory(uploadsFolder);

                string fileName = Guid.NewGuid().ToString()
                    + Path.GetExtension(model.Image.FileName);

                string filePath = Path.Combine(
                    uploadsFolder,
                    fileName);

                using (var stream = new FileStream(
                    filePath,
                    FileMode.Create))
                {
                    await model.Image.CopyToAsync(stream);
                }

                imagePath = "/Images/" + fileName;
            }

            var blogPost = new BlogPost
            {
                Title = model.Title,
                Article = model.Article,
                CategoryId = model.CategoryId,
                ImagePath = imagePath,
                UserId = user.Id,
                CreatedAt = DateTime.Now
            };

            _context.BlogPosts.Add(blogPost);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var blog = await _context.BlogPosts
                .Include(b => b.User)
                .Include(b => b.Category)
                .Include(b => b.Comments)
                .Include(b => b.Likes)
                .FirstOrDefaultAsync(b => b.BlogPostId == id);

            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var blog = await _context.BlogPosts
                .FirstOrDefaultAsync(b => b.BlogPostId == id);

            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var blog = await _context.BlogPosts
                .FirstOrDefaultAsync(b => b.BlogPostId == id);

            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }
    }
}