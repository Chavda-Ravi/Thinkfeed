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

            return RedirectToAction(
                "Index",
                "Home");
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var blog = await _context.BlogPosts
                .Include(b => b.User)
                .Include(b => b.Category)
                .Include(b => b.Comments)
                    .ThenInclude(c => c.User)
                .Include(b => b.Likes)
                .FirstOrDefaultAsync(
                    b => b.BlogPostId == id);

            if (blog == null)
            {
                return NotFound();
            }

            bool isLiked = false;

            if (User.Identity != null &&
                User.Identity.IsAuthenticated)
            {
                var currentUser = await _userManager
                    .GetUserAsync(User);

                if (currentUser != null)
                {
                    isLiked = await _context.Likes
                        .AnyAsync(l =>
                            l.BlogPostId == id &&
                            l.UserId == currentUser.Id);
                }
            }

            ViewBag.IsLiked = isLiked;

            return View(blog);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var blog = await _context.BlogPosts
                .FirstOrDefaultAsync(
                    b => b.BlogPostId == id);

            if (blog == null)
            {
                return NotFound();
            }

            if (blog.UserId != user.Id)
            {
                return Forbid();
            }

            ViewBag.Categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            var model = new BlogPostViewModel
            {
                Title = blog.Title,
                Article = blog.Article,
                CategoryId = blog.CategoryId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            BlogPostViewModel model)
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

            var blog = await _context.BlogPosts
                .FirstOrDefaultAsync(
                    b => b.BlogPostId == id);

            if (blog == null)
            {
                return NotFound();
            }

            if (blog.UserId != user.Id)
            {
                return Forbid();
            }

            blog.Title = model.Title;
            blog.Article = model.Article;
            blog.CategoryId = model.CategoryId;

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

                blog.ImagePath = "/Images/" + fileName;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                new { id = blog.BlogPostId });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var blog = await _context.BlogPosts
                .FirstOrDefaultAsync(
                    b => b.BlogPostId == id);

            if (blog == null)
            {
                return NotFound();
            }

            if (blog.UserId != user.Id)
            {
                return Forbid();
            }

            return View(blog);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var blog = await _context.BlogPosts
                .FirstOrDefaultAsync(
                    b => b.BlogPostId == id);

            if (blog == null)
            {
                return NotFound();
            }

            if (blog.UserId != user.Id)
            {
                return Forbid();
            }

            _context.BlogPosts.Remove(blog);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Index",
                "Home");
        }
    }
}