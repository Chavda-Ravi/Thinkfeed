using Microsoft.AspNetCore.Mvc;
using Thinkfeed.Models;
using Thinkfeed.Services.Interfaces;
using System.Diagnostics;

namespace Thinkfeed.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBlogService _blogService;

        public HomeController(IBlogService blogService)
        {
            _blogService = blogService;
        }

        public async Task<IActionResult> Index()
        {
            var blogs = await _blogService.GetAllAsync();

            return View(blogs);
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new Thinkfeed.Models.ErrorViewModel
                {
                    RequestId = Activity.Current?.Id
                        ?? HttpContext.TraceIdentifier
                });
        }
    }
}