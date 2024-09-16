using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcBlog.Data;

namespace MvcBlog.Views.Shared.Components
{
    public class TagNavigationViewComponent : ViewComponent
    {
        private readonly MvcBlogContext _context;

        public TagNavigationViewComponent(MvcBlogContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string? selectedTag)
        {
            ViewBag.SelectedTag = selectedTag;
            List<string?> items = await GetItemsAsync();
            return View(items);
        }

        private Task<List<String?>> GetItemsAsync()
        {
            return _context!.BlogPostTag!
                .Select(t => t.TagName)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        }
    }
}
