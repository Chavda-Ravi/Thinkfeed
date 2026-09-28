using System.Threading.Tasks;

namespace Thinkfeed.Services.Interfaces
{
    public interface ILikeService
    {
        Task ToggleAsync(int blogPostId, string userId);
    }
}