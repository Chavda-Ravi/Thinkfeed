using System.Collections.Generic;
using System.Threading.Tasks;
using Thinkfeed.Models;
using Thinkfeed.ViewModels;

namespace Thinkfeed.Services.Interfaces
{
    public interface IBlogService
    {
        Task<List<BlogPost>> GetAllAsync();
        Task<List<Category>> GetCategoriesAsync();
        Task CreateAsync(BlogPostViewModel model, ApplicationUser user);
        Task<BlogPost?> GetByIdAsync(int id);
        Task<(BlogPost? blog, bool isLiked)> GetDetailsAsync(int id, string? currentUserId);
        Task<List<BlogPost>> GetByUserIdAsync(string userId);
        Task<bool> CanEditAsync(int id, string userId);
        Task EditAsync(int id, BlogPostViewModel model, string userId);
        Task DeleteAsync(int id, string userId);
    }
}