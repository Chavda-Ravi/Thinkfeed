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

        public BlogService(
            IBlogRepository blogRepository,
            ILikeRepository likeRepository,
            IWebHostEnvironment env)
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
            return await _blogRepository.GetCategoriesAsync();
        }

        private readonly string[] _permittedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long _fileSizeLimit = 5 * 1024 * 1024; // 5 MB
        private const int _maxFilesPerUpload = 10;

        private void ValidateImages(IEnumerable<Microsoft.AspNetCore.Http.IFormFile>? images)
        {
            if (images == null) return;

            var list = images.ToList();
            if (list.Count > _maxFilesPerUpload)
            {
                throw new InvalidOperationException($"You can upload up to {_maxFilesPerUpload} images at once.");
            }

            foreach (var file in list)
            {
                if (file == null) continue;

                if (file.Length == 0)
                {
                    throw new InvalidOperationException("One of the uploaded files is empty.");
                }

                if (file.Length > _fileSizeLimit)
                {
                    throw new InvalidOperationException($"File '{file.FileName}' exceeds the maximum allowed size of 5 MB.");
                }

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(ext) || !_permittedExtensions.Contains(ext))
                {
                    throw new InvalidOperationException($"File '{file.FileName}' has an unsupported file extension.");
                }

                if (!file.ContentType.StartsWith("image/"))
                {
                    throw new InvalidOperationException($"File '{file.FileName}' is not a valid image.");
                }
            }
        }

        public async Task CreateAsync(
            BlogPostViewModel model,
            ApplicationUser user)
        {
            // validate before any DB operation to avoid partial state
            ValidateImages(model.Images);

            var blogPost = new BlogPost
            {
                Title = model.Title,
                Article = model.Article,
                CategoryId = model.CategoryId,
                UserId = user.Id,
                CreatedAt = DateTime.Now
            };

            await _blogRepository.AddAsync(blogPost);
            await _blogRepository.SaveAsync();

            if (model.Images != null && model.Images.Any())
            {
                string uploadsFolder = Path.Combine(
                    _env.WebRootPath,
                    "Images",
                    "Blogs");

                Directory.CreateDirectory(uploadsFolder);

                var createdFiles = new List<string>();

                try
                {
                    foreach (var image in model.Images)
                    {
                        if (image == null || image.Length == 0)
                        {
                            continue;
                        }

                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                        string filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        createdFiles.Add(filePath);

                        var blogPostImage = new BlogPostImage
                        {
                            ImagePath = "/Images/Blogs/" + fileName,
                            BlogPostId = blogPost.BlogPostId
                        };

                        blogPost.Images.Add(blogPostImage);
                    }

                    await _blogRepository.SaveAsync();
                }
                catch
                {
                    // cleanup any created files if DB save fails or upload fails midway
                    foreach (var f in createdFiles)
                    {
                        try { if (System.IO.File.Exists(f)) System.IO.File.Delete(f); } catch { }
                    }
                    throw;
                }
            }
        }

        public async Task<(BlogPost? blog, bool isLiked)> GetDetailsAsync(
            int id,
            string? currentUserId)
        {
            var blog = await _blogRepository.GetByIdAsync(id);

            bool isLiked = false;

            if (currentUserId != null)
            {
                var like = await _likeRepository
                    .GetByBlogAndUserAsync(id, currentUserId);

                isLiked = like != null;
            }

            return (blog, isLiked);
        }

        public async Task<BlogPost?> GetByIdAsync(int id)
        {
            return await _blogRepository.GetByIdAsync(id);
        }

        public async Task<List<BlogPost>> GetByUserIdAsync(
            string userId)
        {
            return await _blogRepository.GetByUserIdAsync(userId);
        }

        public async Task<bool> CanEditAsync(
            int id,
            string userId)
        {
            var blog = await _blogRepository.GetByIdAsync(id);

            if (blog == null)
            {
                return false;
            }

            return blog.UserId == userId;
        }

        public async Task EditAsync(
            int id,
            BlogPostViewModel model,
            string userId)
        {
            var blog = await _blogRepository.GetByIdAsync(id);

            if (blog == null)
            {
                throw new InvalidOperationException(
                    "Blog not found");
            }

            if (blog.UserId != userId)
            {
                throw new UnauthorizedAccessException();
            }

            blog.Title = model.Title;
            blog.Article = model.Article;
            blog.CategoryId = model.CategoryId;

            // Additive image behavior: validate new images and append them to existing collection without removing old images.
            if (model.Images != null && model.Images.Any())
            {
                ValidateImages(model.Images);

                string uploadsFolder = Path.Combine(
                    _env.WebRootPath,
                    "Images",
                    "Blogs");

                Directory.CreateDirectory(uploadsFolder);

                var createdFiles = new List<string>();
                try
                {
                    foreach (var image in model.Images)
                    {
                        if (image == null || image.Length == 0)
                        {
                            continue;
                        }

                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                        string filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        createdFiles.Add(filePath);

                        blog.Images.Add(
                            new BlogPostImage
                            {
                                ImagePath = "/Images/Blogs/" + fileName
                            });
                    }

                    // Save new image records
                    _blogRepository.Update(blog);
                    await _blogRepository.SaveAsync();
                }
                catch
                {
                    foreach (var f in createdFiles)
                    {
                        try { if (System.IO.File.Exists(f)) System.IO.File.Delete(f); } catch { }
                    }
                    throw;
                }
            }

            _blogRepository.Update(blog);

            await _blogRepository.SaveAsync();
        }

        public async Task DeleteAsync(
            int id,
            string userId)
        {
            var blog = await _blogRepository.GetByIdAsync(id);

            if (blog == null)
            {
                throw new InvalidOperationException(
                    "Blog not found");
            }

            if (blog.UserId != userId)
            {
                throw new UnauthorizedAccessException();
            }

            foreach (var image in blog.Images)
            {
                if (!string.IsNullOrEmpty(image.ImagePath))
                {
                    string imagePath = Path.Combine(
                        _env.WebRootPath,
                        image.ImagePath.TrimStart('/'));

                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }
            }

            // Delete old single-image file if it exists
            // for previously created blog posts.
            if (!string.IsNullOrEmpty(blog.ImagePath))
            {
                string oldImagePath = Path.Combine(
                    _env.WebRootPath,
                    blog.ImagePath.TrimStart('/'));

                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            _blogRepository.Delete(blog);

            await _blogRepository.SaveAsync();
        }
    }
}