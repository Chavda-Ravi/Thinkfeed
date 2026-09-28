using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Thinkfeed.Models;
using Thinkfeed.Services.Interfaces;

namespace Thinkfeed.Controllers
{
    [Authorize]
    public class FollowController : Controller
    {
        private readonly IFollowService _followService;
        private readonly UserManager<ApplicationUser> _userManager;

        public FollowController(
            IFollowService followService,
            UserManager<ApplicationUser> userManager)
        {
            _followService = followService;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            if (currentUser.Id == userId) return RedirectToAction("Index", "Profile");

            await _followService.ToggleAsync(currentUser.Id, userId);

            return RedirectToAction("ViewUser", "Profile", new { id = userId });
        }
    }
}
