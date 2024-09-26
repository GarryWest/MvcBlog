using Microsoft.AspNetCore.Mvc;
using MvcBlog.Models;
using System.Diagnostics;

namespace MvcBlog.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public ActionResult GarryImage()
        {
            return File("/Images/RetrieverHeadshot.jfif", "image/jpg", "garry.jpg");
        }

        public ActionResult BlogImage()
        {
            return File("/Images/project1.png", "image/png", "blog.png");
        }
    }
}
