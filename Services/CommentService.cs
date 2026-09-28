using System;
using System.Threading.Tasks;
using Thinkfeed.Models;
using Thinkfeed.Repositories.Interfaces;
using Thinkfeed.Services.Interfaces;

namespace Thinkfeed.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;

        public CommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public async Task AddAsync(int blogPostId, string userId, string content)
        {
            var comment = new Comment
            {
                BlogPostId = blogPostId,
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.Now
            };

            await _commentRepository.AddAsync(comment);
            await _commentRepository.SaveAsync();
        }

        public async Task EditAsync(int commentId, string userId, string content)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null) throw new InvalidOperationException("Comment not found");
            if (comment.UserId != userId) throw new UnauthorizedAccessException();
            comment.Content = content;
            await _commentRepository.SaveAsync();
        }

        public async Task DeleteAsync(int commentId, string userId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null) throw new InvalidOperationException("Comment not found");
            if (comment.UserId != userId) throw new UnauthorizedAccessException();
            _commentRepository.Remove(comment);
            await _commentRepository.SaveAsync();
        }
    }
}
