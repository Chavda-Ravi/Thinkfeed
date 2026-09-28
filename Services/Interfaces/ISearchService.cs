using System.Collections.Generic;
using System.Threading.Tasks;
using Thinkfeed.Models;

namespace Thinkfeed.Services.Interfaces
{
    public interface ISearchService
    {
        Task<List<Category>> GetCategoriesAsync();
        Task<List<BlogPost>> SearchAsync(string? query, int? categoryId, string? username);
    }
}