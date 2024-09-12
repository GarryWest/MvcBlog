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
using MvcBlog.Models;

namespace MvcBlog.Controllers
{
    public class BlogPostController : Controller
    {
        private readonly MvcBlogContext _context;

        public BlogPostController(MvcBlogContext context)
        {
            _context = context;
        }

        // GET: BlogPost
        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["TitleSortParm"] = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewData["CurrentFilter"] = searchString;

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
                case "date_desc":
                    posts = posts.OrderByDescending(p => p.CreateDate);
                    break;
                case "title_desc":
                    posts = posts.OrderByDescending(p => p.Title);
                    break;
                default: // sort by title ascending
                    posts = posts.OrderBy(p => p.Title);
                    break;
            }

            return View(await posts.Include(bp => bp.Tags)
                .AsNoTracking()
                .ToListAsync());
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
