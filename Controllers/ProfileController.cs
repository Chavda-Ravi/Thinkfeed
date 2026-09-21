using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Thinkfeed.Models;
using Thinkfeed.ViewModels;

namespace Thinkfeed.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
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
    }
}