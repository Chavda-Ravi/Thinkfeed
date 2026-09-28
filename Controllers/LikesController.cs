using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Thinkfeed.Models;
using Thinkfeed.Services.Interfaces;

namespace Thinkfeed.Controllers
{
    [Authorize]
    public class LikesController : Controller
    {
        private readonly ILikeService _likeService;
        private readonly UserManager<ApplicationUser> _userManager;

        public LikesController(
            ILikeService likeService,
            UserManager<ApplicationUser> userManager)
        {
            _likeService = likeService;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int blogPostId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            await _likeService.ToggleAsync(blogPostId, user.Id);

            return RedirectToAction("Details", "BlogPosts", new { id = blogPostId });
        }
    }
}
