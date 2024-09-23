using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MvcBlog.Data;
using MvcBlog.Infrastructure;
using MvcBlog.Models;

namespace MvcBlog.Controllers
{
    [Authorize(Policy = "Admin")]
    public class BlogPostController : Controller
    {
        private readonly MvcBlogContext _context;

        public BlogPostController(MvcBlogContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        // GET: BlogPost
        public async Task<IActionResult> Index(
            string sortOrder, 
            string currentFilter,
            string searchString,
            string currentTag,
            string searchTag,
            int? pageNumber
            )
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["TitleSortParm"] = sortOrder == "Title" ? "title_desc" : "Title";
            ViewData["DateSortParm"] = String.IsNullOrEmpty(sortOrder) ? "Date" : "";

            // entering a new searchstring affects the number of pages, so 
            // reset to page 1
            // otherwise, persist the saved search string
            if (searchString != null || searchTag != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
                searchTag = currentTag;
            }

            // save the current search for if they arrow through the results
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentTag"] = searchTag;
            
            var posts = from p in _context.BlogPost select p;
            
            if (!String.IsNullOrEmpty(searchString))
            {
                posts = posts.Where(p => p.Title.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "Date":
                    posts = posts.OrderBy(p => p.CreateDate);
                    break;
                case "Title":
                    posts = posts.OrderBy(p => p.Title);
                    break;
                case "title_desc":
                    posts = posts.OrderByDescending(p => p.Title);
                    break;
                default: // sort by date descending
                    posts = posts.OrderByDescending(p => p.CreateDate);
                    break;
            }

            int pageSize = 3;
            IQueryable<BlogPost> postsQuery = posts
                .Include(bp => bp.Tags)
                .AsNoTracking();

            if (!String.IsNullOrEmpty(searchTag))
            {
                var tags = from t in _context.BlogPostTag select t;
                List<int> tagList = tags.Where(t => t.TagName == searchTag).Select(t => t.BlogPostId).ToList<int>();
                postsQuery = postsQuery.Where(p => tagList.Contains(p.Id));
            }

            return View(new BlogPostListViewModel() { BlogPosts = await PaginatedList<BlogPost>.CreateAsync(
                postsQuery, pageNumber ?? 1, pageSize)
                });
        }

        // POST: AddTag
        // Todo: Add Authorize
        [HttpPost]
        public async Task<IActionResult> AddTag(int postid, string newtag, bool editscreen = false)
        {
            var postTag = await _context.BlogPostTag
                .FirstOrDefaultAsync(m => m.TagName == newtag && m.BlogPostId == postid);
            if (postTag == null)
            {
                var blogPost = await _context.BlogPost
                    .FirstOrDefaultAsync(bp => bp.Id == postid);
                _context.Add(new BlogPostTag() { BlogPostId = postid, TagName = newtag, BlogPost = blogPost});
                await _context.SaveChangesAsync();
            }
            if (editscreen) // return to the edit screen, passing in my post ID
            {
                return RedirectToAction(nameof(Edit), new { id = postid });
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
            
        }

        // POST: DeleteTag
        // Todo: Add Authorize
        [HttpPost]
        public async Task<IActionResult> DeleteTag(int tagid, int? postid)
        {
            var postTag = await _context.BlogPostTag
                .FirstOrDefaultAsync(m => m.Id == tagid);
            if (postTag != null)
            {
                _context.Remove(postTag);
                await _context.SaveChangesAsync();
            }
            if (postid == null)
            {
                return RedirectToAction(nameof(Index));
            } 
            else // We are coming from the Edit Screen
            {
                return RedirectToAction(nameof(Edit), new { id = postid });
            }
            
        }

        [AllowAnonymous]
        // GET: BlogPost/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blogPost = await _context.BlogPost.Include(bp => bp.Tags)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blogPost == null)
            {
                return NotFound();
            }

            return View(blogPost);
        }

        // GET: BlogPost/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BlogPost/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Author,CreateDate,Title,Content,Image")] BlogPost blogPost)
        {
            if (ModelState.IsValid)
            {
                _context.Add(blogPost);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(blogPost);
        }

        // GET: BlogPost/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blogPost = await _context.BlogPost.Include(_bp => _bp.Tags)
                .AsNoTracking()
                .FirstOrDefaultAsync(_bp => _bp.Id == id);
            if (blogPost == null)
            {
                return NotFound();
            }
            return View(blogPost);
        }

        // POST: BlogPost/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Author,CreateDate,Title,Content,Image")] BlogPost blogPost)
        {
            if (id != blogPost.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(blogPost);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogPostExists(blogPost.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(blogPost);
        }

        // GET: BlogPost/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blogPost = await _context.BlogPost.AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blogPost == null)
            {
                return NotFound();
            }

            return View(blogPost);
        }

        // POST: BlogPost/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var blogPost = await _context.BlogPost.FindAsync(id);
            if (blogPost != null)
            {
                _context.BlogPost.Remove(blogPost);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BlogPostExists(int id)
        {
            return _context.BlogPost.Any(e => e.Id == id);
        }
    }
}
