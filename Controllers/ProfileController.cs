using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Thinkfeed.Models;
using Thinkfeed.Services.Interfaces;
using Thinkfeed.ViewModels;
using System.IO;

namespace Thinkfeed.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProfileService _profileService;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            IProfileService profileService)
        {
            _userManager = userManager;
            _profileService = profileService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var userBlogs = await _profileService.GetUserBlogsAsync(user.Id);

            var model = new ProfileViewModel
            {
                UserName = user.UserName,
                FullName = user.FullName ?? string.Empty,
                Bio = user.Bio,
                ProfileImage = user.ProfileImage,
                UserBlogs = userBlogs
            };

            ViewBag.BlogCount = await _profileService.GetBlogCountAsync(user.Id);
            ViewBag.FollowerCount = await _profileService.GetFollowerCountAsync(user.Id);
            ViewBag.FollowingCount = await _profileService.GetFollowingCountAsync(user.Id);
            ViewBag.UserBlogs = userBlogs;

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

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
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            user.FullName = model.FullName;
            user.Bio = model.Bio;

            if (model.Image != null)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Profiles");
                Directory.CreateDirectory(uploadsFolder);

                if (!string.IsNullOrEmpty(user.ProfileImage))
                {
                    string oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfileImage.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath)) System.IO.File.Delete(oldImagePath);
                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Image.CopyToAsync(stream);
                }

                user.ProfileImage = "/Images/Profiles/" + fileName;
            }

            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ViewUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            bool isFollowing = false;
            if (currentUser != null)
            {
                isFollowing = await _profileService.IsFollowingAsync(currentUser.Id, user.Id);
            }

            ViewBag.IsFollowing = isFollowing;
            ViewBag.BlogCount = await _profileService.GetBlogCountAsync(user.Id);
            ViewBag.FollowerCount = await _profileService.GetFollowerCountAsync(user.Id);
            ViewBag.FollowingCount = await _profileService.GetFollowingCountAsync(user.Id);
            ViewBag.UserBlogs = await _profileService.GetUserBlogsAsync(user.Id);

            return View(user);
        }
    }
}
