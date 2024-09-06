using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcBlog.Models
{
    public class BlogPost
    {
        public int Id { get; set; }
        public string? Author { get; set; }

        [DataType(DataType.Date)]
        public DateTime CreateDate { get; set; }

        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Image { get; set; }

        public List<BlogPostTag>? Tags { get; set; }

    }
}
