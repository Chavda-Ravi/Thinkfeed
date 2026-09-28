using System.Threading.Tasks;

namespace Thinkfeed.Services.Interfaces
{
    public interface ICommentService
    {
        Task AddAsync(int blogPostId, string userId, string content);
        Task EditAsync(int commentId, string userId, string content);
        Task DeleteAsync(int commentId, string userId);
    }
}