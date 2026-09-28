using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Thinkfeed.Models;
using Thinkfeed.Services.Interfaces;
using Thinkfeed.ViewModels;

namespace Thinkfeed.Controllers
{
    [Authorize]
    public class BlogPostsController : Controller
    {
        private readonly IBlogService _blogService;
        private readonly UserManager<ApplicationUser> _userManager;

        public BlogPostsController(
            IBlogService blogService,
            UserManager<ApplicationUser> userManager)
        {
            _blogService = blogService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var blogs = await _blogService.GetAllAsync();
            return View(blogs);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _blogService.GetCategoriesAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BlogPostViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _blogService.GetCategoriesAsync();
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            await _blogService.CreateAsync(model, user);

            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var currentUser = User.Identity != null && User.Identity.IsAuthenticated ? await _userManager.GetUserAsync(User) : null;
            var (blog, isLiked) = await _blogService.GetDetailsAsync(id, currentUser?.Id);
            if (blog == null) return NotFound();
            ViewBag.IsLiked = isLiked;
            return View(blog);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            var blog = await _blogService.GetByIdAsync(id);
            if (blog == null) return NotFound();
            if (blog.UserId != user.Id) return Forbid();
            ViewBag.Categories = await _blogService.GetCategoriesAsync();
            var model = new BlogPostViewModel { Title = blog.Title, Article = blog.Article, CategoryId = blog.CategoryId };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BlogPostViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _blogService.GetCategoriesAsync();
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            await _blogService.EditAsync(id, model, user.Id);

            return RedirectToAction("Details", new { id = id });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            var blog = await _blogService.GetByIdAsync(id);
            if (blog == null) return NotFound();
            if (blog.UserId != user.Id) return Forbid();
            return View(blog);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            await _blogService.DeleteAsync(id, user.Id);
            return RedirectToAction("Index", "Home");
        }
    }
}
