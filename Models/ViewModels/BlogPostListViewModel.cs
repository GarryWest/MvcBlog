using MvcBlog.Infrastructure;

namespace MvcBlog.Models
{
    public class BlogPostListViewModel
    {
        public PaginatedList<MvcBlog.Models.BlogPost>? BlogPosts { get; set; }

    }
}
