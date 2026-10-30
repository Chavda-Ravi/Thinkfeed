using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Thinkfeed.Models;
using Thinkfeed.Services.Interfaces;

namespace Thinkfeed.Controllers
{
    [Authorize]
    public class BookmarksController : Controller
    {
        private readonly IBookmarkService _bookmarkService;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookmarksController(
            IBookmarkService bookmarkService,
            UserManager<ApplicationUser> userManager)
        {
            _bookmarkService = bookmarkService;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int blogPostId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            await _bookmarkService.ToggleAsync(blogPostId, user.Id);

            return RedirectToAction("Details", "BlogPosts", new { id = blogPostId });
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var posts = await _bookmarkService.GetSavedPostsAsync(user.Id);
            return View(posts);
        }
    }
}
