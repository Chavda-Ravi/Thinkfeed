using Microsoft.AspNetCore.Mvc;
using Thinkfeed.Services.Interfaces;
using Thinkfeed.ViewModels;

namespace Thinkfeed.Controllers
{
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? query, int? categoryId, string? username)
        {
            var categories = await _searchService.GetCategoriesAsync();
            var results = await _searchService.SearchAsync(query, categoryId, username);

            var model = new SearchViewModel
            {
                Query = query,
                CategoryId = categoryId,
                Username = username,
                Categories = categories,
                Results = results
            };

            return View(model);
        }
    }
}
