using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using Thinkfeed.Models;
using Thinkfeed.Repositories.Interfaces;
using Thinkfeed.Services;
using Thinkfeed.ViewModels;

namespace Thinkfeed.Tests.Services;

public class BlogServiceTests
{
    [Fact]
    public async Task CreateAsync_ValidBlog_AddsBlogAndSaves()
    {
        // Arrange
        var blogRepository = new Mock<IBlogRepository>();
        var likeRepository = new Mock<ILikeRepository>();
        var webHostEnvironment = CreateWebHostEnvironment();
        var service = new BlogService(blogRepository.Object, likeRepository.Object, webHostEnvironment.Object);
        var user = new ApplicationUser { Id = "user-1", UserName = "tester" };
        var model = new BlogPostViewModel
        {
            Title = "My First Blog",
            Article = "This is the article text.",
            CategoryId = 3
        };

        BlogPost? capturedBlog = null;
        blogRepository.Setup(r => r.AddAsync(It.IsAny<BlogPost>()))
            .Callback<BlogPost>(blog => capturedBlog = blog)
            .Returns(Task.CompletedTask);
        blogRepository.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        // Act
        await service.CreateAsync(model, user);

        // Assert
        Assert.NotNull(capturedBlog);
        Assert.Equal("My First Blog", capturedBlog!.Title);
        Assert.Equal("This is the article text.", capturedBlog.Article);
        Assert.Equal(3, capturedBlog.CategoryId);
        Assert.Equal("user-1", capturedBlog.UserId);
        blogRepository.Verify(r => r.AddAsync(It.IsAny<BlogPost>()), Times.Once);
        blogRepository.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithMultipleImages_AddsImageRecordsAndSaves()
    {
        // Arrange
        var blogRepository = new Mock<IBlogRepository>();
        var likeRepository = new Mock<ILikeRepository>();
        var webHostEnvironment = CreateWebHostEnvironment();
        var service = new BlogService(blogRepository.Object, likeRepository.Object, webHostEnvironment.Object);
        var user = new ApplicationUser { Id = "user-1", UserName = "tester" };
        var model = new BlogPostViewModel
        {
            Title = "Image Blog",
            Article = "Body",
            CategoryId = 2,
            Images = new List<IFormFile>
            {
                CreateImageFile("first.png", "image/png"),
                CreateImageFile("second.png", "image/png")
            }
        };

        BlogPost? capturedBlog = null;
        blogRepository.Setup(r => r.AddAsync(It.IsAny<BlogPost>()))
            .Callback<BlogPost>(blog => capturedBlog = blog)
            .Returns(Task.CompletedTask);
        blogRepository.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        // Act
        await service.CreateAsync(model, user);

        // Assert
        Assert.NotNull(capturedBlog);
        Assert.Equal(2, capturedBlog!.Images.Count);
        Assert.All(capturedBlog.Images, image => Assert.Contains("/Images/Blogs/", image.ImagePath));
        blogRepository.Verify(r => r.AddAsync(It.IsAny<BlogPost>()), Times.Once);
        blogRepository.Verify(r => r.SaveAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task CreateAsync_InvalidImageExtension_ThrowsInvalidOperationException()
    {
        // Arrange
        var blogRepository = new Mock<IBlogRepository>();
        var likeRepository = new Mock<ILikeRepository>();
        var webHostEnvironment = CreateWebHostEnvironment();
        var service = new BlogService(blogRepository.Object, likeRepository.Object, webHostEnvironment.Object);
        var user = new ApplicationUser { Id = "user-1", UserName = "tester" };
        var model = new BlogPostViewModel
        {
            Title = "Bad Image",
            Article = "Body",
            CategoryId = 1,
            Images = new List<IFormFile>
            {
                CreateImageFile("bad.txt", "text/plain")
            }
        };

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(model, user));

        // Assert
        Assert.Contains("unsupported file extension", exception.Message);
        blogRepository.Verify(r => r.AddAsync(It.IsAny<BlogPost>()), Times.Never);
    }

    [Fact]
    public async Task GetDetailsAsync_UserLikedBlog_ReturnsLikedTrue()
    {
        // Arrange
        var blogRepository = new Mock<IBlogRepository>();
        var likeRepository = new Mock<ILikeRepository>();
        var webHostEnvironment = CreateWebHostEnvironment();
        var service = new BlogService(blogRepository.Object, likeRepository.Object, webHostEnvironment.Object);
        var blog = new BlogPost { BlogPostId = 7, Title = "Liked Blog", UserId = "owner-1", CategoryId = 2 };

        blogRepository.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(blog);
        likeRepository.Setup(r => r.GetByBlogAndUserAsync(7, "user-2")).ReturnsAsync(new Like { BlogPostId = 7, UserId = "user-2" });

        // Act
        var result = await service.GetDetailsAsync(7, "user-2");

        // Assert
        Assert.NotNull(result.blog);
        Assert.True(result.isLiked);
    }

    [Fact]
    public async Task GetDetailsAsync_UserDidNotLikeBlog_ReturnsLikedFalse()
    {
        // Arrange
        var blogRepository = new Mock<IBlogRepository>();
        var likeRepository = new Mock<ILikeRepository>();
        var webHostEnvironment = CreateWebHostEnvironment();
        var service = new BlogService(blogRepository.Object, likeRepository.Object, webHostEnvironment.Object);
        var blog = new BlogPost { BlogPostId = 8, Title = "Not Liked", UserId = "owner-1", CategoryId = 2 };

        blogRepository.Setup(r => r.GetByIdAsync(8)).ReturnsAsync(blog);
        likeRepository.Setup(r => r.GetByBlogAndUserAsync(8, "user-2")).ReturnsAsync((Like?)null);

        // Act
        var result = await service.GetDetailsAsync(8, "user-2");

        // Assert
        Assert.NotNull(result.blog);
        Assert.False(result.isLiked);
    }

    [Fact]
    public async Task GetDetailsAsync_NoCurrentUser_DoesNotCheckLikeRepository()
    {
        // Arrange
        var blogRepository = new Mock<IBlogRepository>();
        var likeRepository = new Mock<ILikeRepository>();
        var webHostEnvironment = CreateWebHostEnvironment();
        var service = new BlogService(blogRepository.Object, likeRepository.Object, webHostEnvironment.Object);
        var blog = new BlogPost { BlogPostId = 9, Title = "Anonymous", UserId = "owner-1", CategoryId = 2 };

        blogRepository.Setup(r => r.GetByIdAsync(9)).ReturnsAsync(blog);

        // Act
        var result = await service.GetDetailsAsync(9, null);

        // Assert
        Assert.NotNull(result.blog);
        Assert.False(result.isLiked);
        likeRepository.Verify(r => r.GetByBlogAndUserAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_BlogExists_ReturnsBlog()
    {
        // Arrange
        var blogRepository = new Mock<IBlogRepository>();
        var likeRepository = new Mock<ILikeRepository>();
        var webHostEnvironment = CreateWebHostEnvironment();
        var service = new BlogService(blogRepository.Object, likeRepository.Object, webHostEnvironment.Object);
        var blog = new BlogPost { BlogPostId = 12, Title = "Existing Blog", UserId = "owner-1", CategoryId = 4 };

        blogRepository.Setup(r => r.GetByIdAsync(12)).ReturnsAsync(blog);

        // Act
        var result = await service.GetByIdAsync(12);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Existing Blog", result!.Title);
    }

    [Fact]
    public async Task GetByIdAsync_BlogDoesNotExist_ReturnsNull()
    {
        // Arrange
        var blogRepository = new Mock<IBlogRepository>();
        var likeRepository = new Mock<ILikeRepository>();
        var webHostEnvironment = CreateWebHostEnvironment();
        var service = new BlogService(blogRepository.Object, likeRepository.Object, webHostEnvironment.Object);

        blogRepository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((BlogPost?)null);

        // Act
        var result = await service.GetByIdAsync(99);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUserIdAsync_UserHasBlogs_ReturnsBlogs()
    {
        // Arrange
        var blogRepository = new Mock<IBlogRepository>();
        var likeRepository = new Mock<ILikeRepository>();
        var webHostEnvironment = CreateWebHostEnvironment();
        var service = new BlogService(blogRepository.Object, likeRepository.Object, webHostEnvironment.Object);
        var blogs = new List<BlogPost>
        {
            new() { BlogPostId = 1, Title = "First", UserId = "user-3", CategoryId = 1 },
            new() { BlogPostId = 2, Title = "Second", UserId = "user-3", CategoryId = 2 }
        };

        blogRepository.Setup(r => r.GetByUserIdAsync("user-3")).ReturnsAsync(blogs);

        // Act
        var result = await service.GetByUserIdAsync("user-3");

        // Assert
        Assert.IsType<List<BlogPost>>(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("First", result[0].Title);
    }

    [Fact]
    public async Task GetByUserIdAsync_UserHasNoBlogs_ReturnsEmptyList()
    {
        // Arrange
        var blogRepository = new Mock<IBlogRepository>();
        var likeRepository = new Mock<ILikeRepository>();
        var webHostEnvironment = CreateWebHostEnvironment();
        var service = new BlogService(blogRepository.Object, likeRepository.Object, webHostEnvironment.Object);

        blogRepository.Setup(r => r.GetByUserIdAsync("empty-user")).ReturnsAsync(new List<BlogPost>());

        // Act
        var result = await service.GetByUserIdAsync("empty-user");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCategoriesAsync_ReturnsCategories()
    {
        // Arrange
        var blogRepository = new Mock<IBlogRepository>();
        var likeRepository = new Mock<ILikeRepository>();
        var webHostEnvironment = CreateWebHostEnvironment();
        var service = new BlogService(blogRepository.Object, likeRepository.Object, webHostEnvironment.Object);
        var categories = new List<Category>
        {
            new() { CategoryId = 1, Name = "Technology" },
            new() { CategoryId = 2, Name = "Lifestyle" }
        };

        blogRepository.Setup(r => r.GetCategoriesAsync()).ReturnsAsync(categories);

        // Act
        var result = await service.GetCategoriesAsync();

        // Assert
        Assert.IsType<List<Category>>(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Technology", result[0].Name);
    }

    [Fact]
    public async Task GetCategoriesAsync_EmptyCollection_ReturnsEmptyList()
    {
        // Arrange
        var blogRepository = new Mock<IBlogRepository>();
        var likeRepository = new Mock<ILikeRepository>();
        var webHostEnvironment = CreateWebHostEnvironment();
        var service = new BlogService(blogRepository.Object, likeRepository.Object, webHostEnvironment.Object);

        blogRepository.Setup(r => r.GetCategoriesAsync()).ReturnsAsync(new List<Category>());

        // Act
        var result = await service.GetCategoriesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task CanEditAsync_UserOwnsBlog_ReturnsTrue()
    {
        // Arrange
        var blogRepository = new Mock<IBlogRepository>();
        var likeRepository = new Mock<ILikeRepository>();
        var webHostEnvironment = CreateWebHostEnvironment();
        var service = new BlogService(blogRepository.Object, likeRepository.Object, webHostEnvironment.Object);
        var blog = new BlogPost { BlogPostId = 3, Title = "Owned", UserId = "owner-1" };

        blogRepository.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(blog);

        // Act
        var result = await service.CanEditAsync(3, "owner-1");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanEditAsync_UserDoesNotOwnBlog_ReturnsFalse()
    {
        // Arrange
        var blogRepository = new Mock<IBlogRepository>();
        var likeRepository = new Mock<ILikeRepository>();
        var webHostEnvironment = CreateWebHostEnvironment();
        var service = new BlogService(blogRepository.Object, likeRepository.Object, webHostEnvironment.Object);
        var blog = new BlogPost { BlogPostId = 4, Title = "Owned by other", UserId = "owner-2" };

        blogRepository.Setup(r => r.GetByIdAsync(4)).ReturnsAsync(blog);

        // Act
        var result = await service.CanEditAsync(4, "owner-1");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CanEditAsync_BlogDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var blogRepository = new Mock<IBlogRepository>();
        var likeRepository = new Mock<ILikeRepository>();
        var webHostEnvironment = CreateWebHostEnvironment();
        var service = new BlogService(blogRepository.Object, likeRepository.Object, webHostEnvironment.Object);

        blogRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((BlogPost?)null);

        // Act
        var result = await service.CanEditAsync(999, "owner-1");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task EditAsync_NoNewImages_KeepsExistingImages()
    {
        // Arrange
        var blogRepository = new Mock<IBlogRepository>();
        var likeRepository = new Mock<ILikeRepository>();
        var webHostEnvironment = CreateWebHostEnvironment();
        var service = new BlogService(blogRepository.Object, likeRepository.Object, webHostEnvironment.Object);
        var blog = new BlogPost
        {
            BlogPostId = 20,
            Title = "Old Title",
            Article = "Old Article",
            CategoryId = 1,
            UserId = "owner-1",
            Images = new List<BlogPostImage>
            {
                new() { ImagePath = "/Images/Blogs/old.jpg" }
            }
        };

        blogRepository.Setup(r => r.GetByIdAsync(20)).ReturnsAsync(blog);
        blogRepository.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        var model = new BlogPostViewModel
        {
            Title = "New Title",
            Article = "New Article",
            CategoryId = 5,
            Images = new List<IFormFile>()
        };

        // Act
        await service.EditAsync(20, model, "owner-1");

        // Assert
        Assert.Equal("New Title", blog.Title);
        Assert.Equal("New Article", blog.Article);
        Assert.Equal(5, blog.CategoryId);
        Assert.Single(blog.Images);
        Assert.Equal("/Images/Blogs/old.jpg", blog.Images.First().ImagePath);
        blogRepository.Verify(r => r.Update(blog), Times.Once);
        blogRepository.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task EditAsync_NewImage_AddsImageWithoutRemovingExistingImages()
    {
        // Arrange
        var blogRepository = new Mock<IBlogRepository>();
        var likeRepository = new Mock<ILikeRepository>();
        var webHostEnvironment = CreateWebHostEnvironment();
        var service = new BlogService(blogRepository.Object, likeRepository.Object, webHostEnvironment.Object);
        var blog = new BlogPost
        {
            BlogPostId = 21,
            Title = "Old Title",
            Article = "Old Article",
            CategoryId = 1,
            UserId = "owner-1",
            Images = new List<BlogPostImage>
            {
                new() { ImagePath = "/Images/Blogs/existing.jpg" }
            }
        };

        blogRepository.Setup(r => r.GetByIdAsync(21)).ReturnsAsync(blog);
        blogRepository.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        var model = new BlogPostViewModel
        {
            Title = "Updated Title",
            Article = "Updated Article",
            CategoryId = 6,
            Images = new List<IFormFile>
            {
                CreateImageFile("new.png", "image/png")
            }
        };

        // Act
        await service.EditAsync(21, model, "owner-1");

        // Assert
        Assert.Equal(2, blog.Images.Count);
        Assert.Contains(blog.Images, image => image.ImagePath == "/Images/Blogs/existing.jpg");
        Assert.Contains(blog.Images, image => image.ImagePath.Contains("/Images/Blogs/"));
        Assert.NotEqual("Old Title", blog.Title);
    }

    [Fact]
    public async Task EditAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var blogRepository = new Mock<IBlogRepository>();
        var likeRepository = new Mock<ILikeRepository>();
        var webHostEnvironment = CreateWebHostEnvironment();
        var service = new BlogService(blogRepository.Object, likeRepository.Object, webHostEnvironment.Object);
        var blog = new BlogPost { BlogPostId = 30, Title = "Test", Article = "Body", CategoryId = 2, UserId = "owner-2" };

        blogRepository.Setup(r => r.GetByIdAsync(30)).ReturnsAsync(blog);

        var model = new BlogPostViewModel
        {
            Title = "Changed",
            Article = "Changed body",
            CategoryId = 3
        };

        // Act
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.EditAsync(30, model, "owner-1"));

        // Assert
        Assert.NotNull(exception);
    }

    [Fact]
    public async Task EditAsync_BlogDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var blogRepository = new Mock<IBlogRepository>();
        var likeRepository = new Mock<ILikeRepository>();
        var webHostEnvironment = CreateWebHostEnvironment();
        var service = new BlogService(blogRepository.Object, likeRepository.Object, webHostEnvironment.Object);
        var model = new BlogPostViewModel
        {
            Title = "Missing",
            Article = "Body",
            CategoryId = 1
        };

        blogRepository.Setup(r => r.GetByIdAsync(404)).ReturnsAsync((BlogPost?)null);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.EditAsync(404, model, "owner-1"));

        // Assert
        Assert.Equal("Blog not found", exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_UserOwnsBlog_DeletesBlog()
    {
        // Arrange
        var blogRepository = new Mock<IBlogRepository>();
        var likeRepository = new Mock<ILikeRepository>();
        var webRootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(webRootPath, "Images", "Blogs"));

        var filePath = Path.Combine(webRootPath, "Images", "Blogs", "delete-me.jpg");
        await File.WriteAllBytesAsync(filePath, "test"u8.ToArray());

        var webHostEnvironment = new Mock<IWebHostEnvironment>();
        webHostEnvironment.SetupGet(e => e.WebRootPath).Returns(webRootPath);

        var service = new BlogService(blogRepository.Object, likeRepository.Object, webHostEnvironment.Object);
        var blog = new BlogPost
        {
            BlogPostId = 50,
            Title = "Delete Me",
            Article = "Body",
            UserId = "owner-1",
            CategoryId = 3,
            Images = new List<BlogPostImage>
            {
                new() { ImagePath = "/Images/Blogs/delete-me.jpg" }
            }
        };

        blogRepository.Setup(r => r.GetByIdAsync(50)).ReturnsAsync(blog);
        blogRepository.Setup(r => r.Delete(blog));
        blogRepository.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        // Act
        await service.DeleteAsync(50, "owner-1");

        // Assert
        Assert.False(File.Exists(filePath));
        blogRepository.Verify(r => r.Delete(blog), Times.Once);
        blogRepository.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var blogRepository = new Mock<IBlogRepository>();
        var likeRepository = new Mock<ILikeRepository>();
        var webHostEnvironment = CreateWebHostEnvironment();
        var service = new BlogService(blogRepository.Object, likeRepository.Object, webHostEnvironment.Object);
        var blog = new BlogPost { BlogPostId = 51, Title = "Owned", Article = "Body", UserId = "owner-2", CategoryId = 2 };

        blogRepository.Setup(r => r.GetByIdAsync(51)).ReturnsAsync(blog);

        // Act
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.DeleteAsync(51, "owner-1"));

        // Assert
        Assert.NotNull(exception);
    }

    [Fact]
    public async Task DeleteAsync_BlogDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var blogRepository = new Mock<IBlogRepository>();
        var likeRepository = new Mock<ILikeRepository>();
        var webHostEnvironment = CreateWebHostEnvironment();
        var service = new BlogService(blogRepository.Object, likeRepository.Object, webHostEnvironment.Object);

        blogRepository.Setup(r => r.GetByIdAsync(404)).ReturnsAsync((BlogPost?)null);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.DeleteAsync(404, "owner-1"));

        // Assert
        Assert.Equal("Blog not found", exception.Message);
    }

    private static Mock<IWebHostEnvironment> CreateWebHostEnvironment()
    {
        var rootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(rootPath, "Images", "Blogs"));

        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(e => e.WebRootPath).Returns(rootPath);
        return environment;
    }

    private static IFormFile CreateImageFile(string fileName, string contentType)
    {
        var content = "fake image bytes";
        var bytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);

        return new FormFile(stream, 0, bytes.Length, fileName, fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
