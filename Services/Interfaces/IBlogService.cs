using Microsoft.AspNetCore.Http;
using Thinkfeed.Models;
using Thinkfeed.ViewModels;

namespace Thinkfeed.Services.Interfaces
{
    public interface IBlogService
    {
        Task<List<BlogPost>> GetAllAsync();

        Task<BlogPost?> GetByIdAsync(int id);

        Task<List<BlogPost>> GetByUserIdAsync(
            string userId);

        Task<List<Category>> GetCategoriesAsync();

        Task CreateAsync(
            BlogPostViewModel model,
            ApplicationUser user);

        Task<(BlogPost? blog, bool isLiked)> GetDetailsAsync(
            int id,
            string? currentUserId);

        Task<bool> CanEditAsync(
            int id,
            string userId);

        Task EditAsync(
            int id,
            BlogPostViewModel model,
            string userId);

        Task DeleteAsync(
            int id,
            string userId);
    }
}