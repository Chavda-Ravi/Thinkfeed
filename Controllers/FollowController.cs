using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Thinkfeed.Data;
using Thinkfeed.Models;

namespace Thinkfeed.Controllers
{
    [Authorize]
    public class FollowController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FollowController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Challenge();
            }

            if (currentUser.Id == userId)
            {
                return RedirectToAction("Index", "Profile");
            }

            var existingFollow = await _context.Follows
                .FirstOrDefaultAsync(f =>
                    f.FollowerId == currentUser.Id &&
                    f.FollowingId == userId);

            if (existingFollow != null)
            {
                _context.Follows.Remove(existingFollow);
            }
            else
            {
                var follow = new Follow
                {
                    FollowerId = currentUser.Id,
                    FollowingId = userId,
                    CreatedAt = DateTime.Now
                };

                _context.Follows.Add(follow);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "ViewUser",
                "Profile",
                new { id = userId });
        }
    }
}