using Thinkfeed.Models;

namespace Thinkfeed.Repositories.Interfaces
{
    public interface IBlogRepository
    {
        Task<List<BlogPost>> GetAllAsync();

        Task<BlogPost?> GetByIdAsync(int id);

        Task<List<BlogPost>> GetByUserIdAsync(string userId);
        Task<List<Category>> GetCategoriesAsync();

        Task AddAsync(BlogPost blogPost);

        void Update(BlogPost blogPost);

        void Delete(BlogPost blogPost);

        Task SaveAsync();
    }
}