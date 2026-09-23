using Thinkfeed.Models;

namespace Thinkfeed.ViewModels
{
    public class SearchViewModel
    {
        public string? Query { get; set; }

        public int? CategoryId { get; set; }

        public string? Username { get; set; }

        public List<Category> Categories { get; set; } = new List<Category>();

        public List<BlogPost> Results { get; set; } = new List<BlogPost>();
    }
}