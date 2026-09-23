using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Thinkfeed.Data;
using Thinkfeed.Models;

namespace Thinkfeed.Controllers
{
    [Authorize]
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(
            int blogPostId,
            string content)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return RedirectToAction(
                    "Details",
                    "BlogPosts",
                    new { id = blogPostId });
            }

            var comment = new Comment
            {
                BlogPostId = blogPostId,
                UserId = user.Id,
                Content = content,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                "BlogPosts",
                new { id = blogPostId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.CommentId == id);

            if (comment == null)
            {
                return NotFound();
            }

            if (comment.UserId != user.Id)
            {
                return Forbid();
            }

            int blogPostId = comment.BlogPostId;

            _context.Comments.Remove(comment);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                "BlogPosts",
                new { id = blogPostId });
        }
    }
}