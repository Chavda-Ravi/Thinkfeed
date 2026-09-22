using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Thinkfeed.Data;
using Thinkfeed.Models;

namespace Thinkfeed.Controllers
{
    [Authorize]
    public class LikesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LikesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
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

            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l =>
                    l.BlogPostId == blogPostId &&
                    l.UserId == user.Id);

            if (existingLike != null)
            {
                _context.Likes.Remove(existingLike);
            }
            else
            {
                var like = new Like
                {
                    BlogPostId = blogPostId,
                    UserId = user.Id,
                    CreatedAt = DateTime.Now
                };

                _context.Likes.Add(like);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                "BlogPosts",
                new { id = blogPostId });
        }
    }
}