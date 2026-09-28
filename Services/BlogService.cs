using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Thinkfeed.Models;
using Thinkfeed.Repositories.Interfaces;
using Thinkfeed.Services.Interfaces;
using Thinkfeed.ViewModels;

namespace Thinkfeed.Services
{
    public class BlogService : IBlogService
    {
        private readonly IBlogRepository _blogRepository;
        private readonly ILikeRepository _likeRepository;
        private readonly IWebHostEnvironment _env;

public BlogService(IBlogRepository blogRepository, ILikeRepository likeRepository, IWebHostEnvironment env)
        {
            _blogRepository = blogRepository;
            _likeRepository = likeRepository;
            _env = env;
        }

        public async Task<List<BlogPost>> GetAllAsync()
        {
            return await _blogRepository.GetAllAsync();
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            // categories are accessible via blog repository's context; but repository doesn't expose categories directly.
            // Use the blog repository to fetch all blogs and extract categories — simpler and safe for current app size.
            var all = await _blogRepository.GetAllAsync();
            return all.Select(b => b.Category).Where(c => c != null).Distinct().ToList()!;
        }

        public async Task CreateAsync(BlogPostViewModel model, ApplicationUser user)
        {
            string? imagePath = null;

            if (model.Image != null)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "Images", "Blogs");
                Directory.CreateDirectory(uploadsFolder);
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Image.CopyToAsync(stream);
                }

                imagePath = "/Images/Blogs/" + fileName;
            }

            var blogPost = new BlogPost
            {
                Title = model.Title,
                Article = model.Article,
                CategoryId = model.CategoryId,
                ImagePath = imagePath,
                UserId = user.Id,
                CreatedAt = DateTime.Now
            };

            await _blogRepository.AddAsync(blogPost);
            await _blogRepository.SaveAsync();
        }

        public async Task<(BlogPost? blog, bool isLiked)> GetDetailsAsync(int id, string? currentUserId)
        {
            var blog = await _blogRepository.GetByIdAsync(id);
            bool isLiked = false;
            if (currentUserId != null)
            {
                var like = await _likeRepository.GetByBlogAndUserAsync(id, currentUserId);
                isLiked = like != null;
            }
            return (blog, isLiked);
        }

        public async Task<BlogPost?> GetByIdAsync(int id)
        {
            return await _blogRepository.GetByIdAsync(id);
        }

        public async Task<List<BlogPost>> GetByUserIdAsync(string userId)
        {
            return await _blogRepository.GetByUserIdAsync(userId);
        }

        public async Task<bool> CanEditAsync(int id, string userId)
        {
            var blog = await _blogRepository.GetByIdAsync(id);
            if (blog == null) return false;
            return blog.UserId == userId;
        }

        public async Task EditAsync(int id, BlogPostViewModel model, string userId)
        {
            var blog = await _blogRepository.GetByIdAsync(id);
            if (blog == null) throw new InvalidOperationException("Blog not found");
            if (blog.UserId != userId) throw new UnauthorizedAccessException();

            blog.Title = model.Title;
            blog.Article = model.Article;
            blog.CategoryId = model.CategoryId;

            if (model.Image != null)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "Images", "Blogs");
                Directory.CreateDirectory(uploadsFolder);

                if (!string.IsNullOrEmpty(blog.ImagePath))
                {
                    string oldImagePath = Path.Combine(_env.WebRootPath, blog.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath)) System.IO.File.Delete(oldImagePath);
                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Image.CopyToAsync(stream);
                }
                blog.ImagePath = "/Images/Blogs/" + fileName;
            }

            _blogRepository.Update(blog);
            await _blogRepository.SaveAsync();
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var blog = await _blogRepository.GetByIdAsync(id);
            if (blog == null) throw new InvalidOperationException("Blog not found");
            if (blog.UserId != userId) throw new UnauthorizedAccessException();

            if (!string.IsNullOrEmpty(blog.ImagePath))
            {
                string imagePath = Path.Combine(_env.WebRootPath, blog.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(imagePath)) System.IO.File.Delete(imagePath);
            }

            _blogRepository.Delete(blog);
            await _blogRepository.SaveAsync();
        }
    }
}