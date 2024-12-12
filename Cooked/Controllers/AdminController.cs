using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Cooked.Data;
using Cooked.Models;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        return View();
    }

    // View all users
    public IActionResult Users()
    {
        var users = _userManager.Users.ToList();
        return View(users);
    }

    // Delete user and their data
    [HttpPost]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            // Delete user's recipes
            var recipes = _context.Recipes.Where(r => r.CreatedByUserId == id);
            _context.Recipes.RemoveRange(recipes);

            // Delete user
            await _userManager.DeleteAsync(user);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Users));
    }

    // View all reviews
    public IActionResult Reviews()
    {
        var reviews = _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Recipe)
            .ToList();
        return View(reviews);
    }

    // Delete review
    [HttpPost]
    public IActionResult DeleteReview(int id)
    {
        var review = _context.Reviews.Find(id);
        if (review != null)
        {
            _context.Reviews.Remove(review);
            _context.SaveChanges();
        }
        return RedirectToAction(nameof(Reviews));
    }

    // View all recipes
    public IActionResult Recipes()
    {
        var recipes = _context.Recipes
            .Include(r => r.CreatedByUser)
            .Include(r => r.RecipeImages)
            .ToList();
        return View(recipes);
    }

    // Delete recipe
    [HttpPost]
    public IActionResult DeleteRecipe(int id)
    {
        var recipe = _context.Recipes.Find(id);
        if (recipe != null)
        {
            _context.Recipes.Remove(recipe);
            _context.SaveChanges();
        }
        return RedirectToAction(nameof(Recipes));
    }
}
