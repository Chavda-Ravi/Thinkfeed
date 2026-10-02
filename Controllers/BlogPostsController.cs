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
            ViewBag.Categories =
                await _blogService.GetCategoriesAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            BlogPostViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories =
                    await _blogService.GetCategoriesAsync();

                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            try
            {
                await _blogService.CreateAsync(
                    model,
                    user);

                return RedirectToAction(
                    "Index",
                    "Home");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Categories = await _blogService.GetCategoriesAsync();
                return View(model);
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            string? currentUserId = null;

            if (User.Identity != null &&
                User.Identity.IsAuthenticated)
            {
                var currentUser =
                    await _userManager.GetUserAsync(User);

                currentUserId = currentUser?.Id;
            }

            var result = await _blogService
                .GetDetailsAsync(id, currentUserId);

            if (result.blog == null)
            {
                return NotFound();
            }

            ViewBag.IsLiked = result.isLiked;

            return View(result.blog);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var blog = await _blogService.GetByIdAsync(id);

            if (blog == null)
            {
                return NotFound();
            }

            if (blog.UserId != user.Id)
            {
                return Forbid();
            }

            ViewBag.Categories =
                await _blogService.GetCategoriesAsync();

            // Provide existing images (use Images collection if present; otherwise fallback to old ImagePath)
            var existing = new List<string>();
            if (blog.Images != null && blog.Images.Any())
            {
                existing.AddRange(blog.Images.Where(i => !string.IsNullOrEmpty(i.ImagePath)).Select(i => i.ImagePath));
            }
            else if (!string.IsNullOrEmpty(blog.ImagePath))
            {
                existing.Add(blog.ImagePath);
            }
            ViewBag.ExistingImages = existing;

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
                ViewBag.Categories =
                    await _blogService.GetCategoriesAsync();

                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            try
            {
                await _blogService.EditAsync(
                    id,
                    model,
                    user.Id);
            }
            catch (InvalidOperationException ex)
            {
                // validation error or not found - show message in form
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Categories = await _blogService.GetCategoriesAsync();
                return View(model);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }

            return RedirectToAction(
                "Details",
                new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var blog = await _blogService.GetByIdAsync(id);

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

            try
            {
                await _blogService.DeleteAsync(
                    id,
                    user.Id);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }

            return RedirectToAction(
                "Index",
                "Home");
        }
    }
}