using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Cooked.Models;
using Microsoft.EntityFrameworkCore;
using Cooked.Data;
using Cooked.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _context = context;
    }

    // Account Overview / Index Page
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var user = await _userManager.FindByIdAsync(userId);
        var recipes = await _context.Recipes
                                     .Where(r => r.CreatedByUserId == userId)
                                     .Include(r => r.RecipeImages) // Ensure recipe images are included
                                     .ToListAsync();

        var model = new AccountViewModel
        {
            User = user,
            Recipes = recipes
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Profile(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var user = await _userManager.Users
                                     .Include(u => u.Recipes) // Include recipes created by the user
                                     .ThenInclude(r => r.RecipeImages) // Include recipe images
                                     .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }



    // Login Action
    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password)
    {
        if (!ModelState.IsValid)
        {
            Console.WriteLine("ModelState is invalid");
            return View();
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Email not found.");
            return View();
        }

        // Check the password
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
        Console.WriteLine($"Password Valid: {isPasswordValid}");

        // Check if the user is locked out
        var isLockedOut = await _userManager.IsLockedOutAsync(user);
        Console.WriteLine($"Locked Out: {isLockedOut}");

        // Check if two-factor is required
        var requiresTwoFactor = await _userManager.GetTwoFactorEnabledAsync(user);
        Console.WriteLine($"Requires Two-Factor: {requiresTwoFactor}");

        // If all checks pass, proceed to sign in manually
        if (isPasswordValid && !isLockedOut && !requiresTwoFactor)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index");
        }
        else
        {
            Console.WriteLine("Some check failed.");
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View();
        }

    }


    // Register Action
    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(string email, string password, string firstName, string lastName, IFormFile profileImage)
    {
        // Create the user object
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };

        // Handle image upload
        if (profileImage != null && profileImage.Length > 0)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "users");
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save the image
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await profileImage.CopyToAsync(fileStream);
            }

            // Set the ImageUrl property
            user.ImageUrl = "/images/users/" + fileName;
        }

        // Create the user
        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index");
        }

        // Handle errors if any
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View();
    }


    // Logout Action
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }


    // Delete Account and Associated Recipes
    [HttpPost]
    public async Task<IActionResult> DeleteAccount()
    {
        var userId = _userManager.GetUserId(User);
        var user = await _userManager.FindByIdAsync(userId);

        if (user != null)
        {
            // Delete all recipes created by the user
            var recipes = _context.Recipes.Where(r => r.CreatedByUserId == userId);
            _context.Recipes.RemoveRange(recipes);

            // Remove user
            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError(string.Empty, "Error deleting the account.");
        }

        return RedirectToAction("Index");
    }
}
