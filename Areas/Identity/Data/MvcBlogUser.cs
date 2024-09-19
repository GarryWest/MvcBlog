using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace MvcBlog.Areas.Identity.Data;

// Add profile data for application users by adding properties to the MvcBlogUser class
public class MvcBlogUser : IdentityUser
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;


}

