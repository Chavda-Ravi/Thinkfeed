using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Thinkfeed.Models;
using Thinkfeed.Services.Interfaces;

namespace Thinkfeed.Controllers
{
    [Authorize]
    public class CommentsController : Controller
    {
        private readonly ICommentService _commentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentsController(
            ICommentService commentService,
            UserManager<ApplicationUser> userManager)
        {
            _commentService = commentService;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int blogPostId, string content)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (string.IsNullOrWhiteSpace(content))
            {
                return RedirectToAction("Details", "BlogPosts", new { id = blogPostId });
            }

            await _commentService.AddAsync(blogPostId, user.Id, content);

            return RedirectToAction("Details", "BlogPosts", new { id = blogPostId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string content)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null) return NotFound();

            if (string.IsNullOrWhiteSpace(content))
            {
                return RedirectToAction("Details", "BlogPosts", new { id = comment.BlogPostId });
            }

            await _commentService.EditAsync(id, user.Id, content);

            return RedirectToAction("Details", "BlogPosts", new { id = comment.BlogPostId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null) return NotFound();

            await _commentService.DeleteAsync(id, user.Id);

            return RedirectToAction("Details", "BlogPosts", new { id = comment.BlogPostId });
        }
    }
}
