namespace MvcBlog.Models
{
    public class BlogPostTag
    {
        public int Id { get; set; }
        public int BlogPostId { get; set; }
        public string? TagName { get; set; }

        public required BlogPost BlogPost { get; set; }
    }
}
