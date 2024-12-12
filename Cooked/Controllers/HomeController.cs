using Cooked.Data;
using Cooked.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Cooked.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index(string searchQuery = null)
        {
            // Store searchQuery in ViewData to pass it to the view
            ViewData["SearchQuery"] = searchQuery;

            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
