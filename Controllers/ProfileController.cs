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
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var model = new ProfileViewModel
            {
                UserName = user.UserName,
                FullName = user.FullName ?? string.Empty,
                Bio = user.Bio,
                ProfileImage = user.ProfileImage
            };

            ViewBag.BlogCount = await _context.BlogPosts
                .CountAsync(b => b.UserId == user.Id);

            ViewBag.FollowerCount = await _context.Follows
                .CountAsync(f => f.FollowingId == user.Id);

            ViewBag.FollowingCount = await _context.Follows
                .CountAsync(f => f.FollowerId == user.Id);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var model = new ProfileViewModel
            {
                UserName = user.UserName,
                FullName = user.FullName ?? string.Empty,
                Bio = user.Bio,
                ProfileImage = user.ProfileImage
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            user.FullName = model.FullName;
            user.Bio = model.Bio;

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

                user.ProfileImage = "/Images/" + fileName;
            }

            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ViewUser(string id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);

            bool isFollowing = false;

            if (currentUser != null)
            {
                isFollowing = await _context.Follows
                    .AnyAsync(f =>
                        f.FollowerId == currentUser.Id &&
                        f.FollowingId == user.Id);
            }

            ViewBag.IsFollowing = isFollowing;

            ViewBag.BlogCount = await _context.BlogPosts
                .CountAsync(b => b.UserId == user.Id);

            ViewBag.FollowerCount = await _context.Follows
                .CountAsync(f => f.FollowingId == user.Id);

            ViewBag.FollowingCount = await _context.Follows
                .CountAsync(f => f.FollowerId == user.Id);

            return View(user);
        }
    }
}