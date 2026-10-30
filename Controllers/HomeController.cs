using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Thinkfeed.Models;
using Thinkfeed.Services.Interfaces;
using System.Diagnostics;

namespace Thinkfeed.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBlogService _blogService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IBlogService blogService,
            ILogger<HomeController> logger)
        {
            _blogService = blogService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var blogs = await _blogService.GetAllAsync();

            return View(blogs);
        }

        [AllowAnonymous]
        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            var exception = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;

            if (exception != null)
            {
                _logger.LogError(
                    exception,
                    "Unhandled exception occurred while processing request {RequestPath}",
                    HttpContext.Request.Path);
            }

            return View(
                new Thinkfeed.Models.ErrorViewModel
                {
                    RequestId = Activity.Current?.Id
                        ?? HttpContext.TraceIdentifier
                });
        }
    }
}