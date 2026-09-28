using System.Threading.Tasks;
using Thinkfeed.Models;
using System.Collections.Generic;

namespace Thinkfeed.Repositories.Interfaces
{
    public interface ICommentRepository
    {
        Task AddAsync(Comment comment);
        Task<Comment?> GetByIdAsync(int id);
        void Remove(Comment comment);
        Task<List<Comment>> GetByBlogIdAsync(int blogPostId);
        Task SaveAsync();
    }
}