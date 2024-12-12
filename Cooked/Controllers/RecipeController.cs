using Microsoft.AspNetCore.Mvc;
using Cooked.Models;
using Cooked.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Cooked.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Cooked.Controllers
{
    public class RecipeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public RecipeController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: Recipe/Index 
        public IActionResult Index(string searchQuery = null)
        {
            // Store searchQuery in ViewData to pass it to the view
            ViewData["SearchQuery"] = searchQuery;

            // If there's a search query, filter the recipes based on title or description
            var recipesQuery = _context.Recipes.Include(r => r.RecipeImages).AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.ToLower(); // Transform input to lowercase

                // Filter recipes based on the search query (by title or description)
                recipesQuery = recipesQuery.Where(r => r.Title.ToLower().Contains(searchQuery) || r.Description.ToLower().Contains(searchQuery));
            }

            // Fetch the filtered recipes or all recipes if no search query
            var recipes = recipesQuery.ToList();

            return View(recipes);
        }

        // GET: Recipe/Create
        [Authorize]
        public IActionResult Create()
        {
            var viewModel = new RecipeCreateViewModel
            {
                Categories = _context.Categories.ToList()  // Populating categories for the dropdown list
            };
            return View(viewModel);
        }

        // POST: Recipe/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RecipeCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Create a new recipe
                var recipe = new Recipe
                {
                    Title = model.Title,
                    Description = model.Description,
                    Instructions = model.Instructions,
                    IngredientsText = model.IngredientsText,
                    CategoryId = model.CategoryId,
                    CreatedByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                };

                // Add the recipe to the database
                _context.Recipes.Add(recipe);
                await _context.SaveChangesAsync();  // Save to generate RecipeId

                // Process the images
                if (model.Images != null && model.Images.Length > 0)
                {
                    foreach (var image in model.Images)
                    {
                        // Generate a unique file name for each image
                        var fileName = Path.GetFileNameWithoutExtension(image.FileName) + "_" + Guid.NewGuid() + Path.GetExtension(image.FileName);
                        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "recipes", fileName);

                        // Save the image to wwwroot/images/recipes
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        // Create the RecipeImage object to store the image URL
                        var recipeImage = new RecipeImage
                        {
                            RecipeId = recipe.Id,
                            ImageUrl = "/images/recipes/" + fileName // Store the relative path to the image
                        };

                        // Add the image record to the database
                        _context.RecipeImages.Add(recipeImage);
                    }

                    await _context.SaveChangesAsync(); // Save the images to the database
                }

                return RedirectToAction(nameof(Index));  // Redirect to the Index or Recipe listing page
            }

            // If model validation fails, reload categories
            model.Categories = _context.Categories.ToList();
            return View(model);
        }

        // GET: Recipe/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var recipe = await _context.Recipes
                .Include(r => r.RecipeImages)
                .Include(r => r.Category)  // Load the Category data
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
            {
                return NotFound();
            }

            var viewModel = new RecipeEditViewModel
            {
                Id = recipe.Id,
                Title = recipe.Title,
                Description = recipe.Description,
                Instructions = recipe.Instructions,
                IngredientsText = recipe.IngredientsText,
                CategoryId = recipe.CategoryId,
                Categories = _context.Categories.ToList(),  // Populating categories for dropdown
                ExistingImages = recipe.RecipeImages.Select(i => i.ImageUrl).ToList()  // Fetch existing images
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RecipeEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var recipe = await _context.Recipes
                    .Include(r => r.RecipeImages)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (recipe == null)
                {
                    return NotFound();
                }

                // Update recipe properties
                recipe.Title = model.Title;
                recipe.Description = model.Description;
                recipe.Instructions = model.Instructions;
                recipe.IngredientsText = model.IngredientsText;
                recipe.CategoryId = model.CategoryId;

                _context.Update(recipe);

                // Process images to delete
                if (model.ImagesToDelete != null && model.ImagesToDelete.Count > 0)
                {
                    foreach (var imageUrl in model.ImagesToDelete)
                    {
                        // Find the image in the database
                        var image = recipe.RecipeImages.FirstOrDefault(img => img.ImageUrl == imageUrl);
                        if (image != null)
                        {
                            // Remove from the database
                            _context.RecipeImages.Remove(image);

                            // Delete the file from the server
                            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, image.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);
                            }
                        }
                    }
                }

                // Process new images
                if (model.NewImages != null && model.NewImages.Count > 0)
                {
                    foreach (var image in model.NewImages)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(image.FileName) + "_" + Guid.NewGuid() + Path.GetExtension(image.FileName);
                        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "recipes", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        var recipeImage = new RecipeImage
                        {
                            RecipeId = recipe.Id,
                            ImageUrl = "/images/recipes/" + fileName
                        };

                        _context.RecipeImages.Add(recipeImage);
                    }
                }

                // Save changes to the database
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Account");
            }

            // If model validation fails, reload categories and existing images
            model.Categories = _context.Categories.ToList();
            var recipeWithImages = await _context.Recipes
                .Include(r => r.RecipeImages)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipeWithImages != null)
            {
                model.ExistingImages = recipeWithImages.RecipeImages.Select(i => i.ImageUrl).ToList();
            }

            return View(model);
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
            return RedirectToAction("Index", "Account");
        }



        // GET: Recipe/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var recipe = await _context.Recipes
                .Include(r => r.RecipeImages)
                .Include(r => r.Category)  // Include Category information for display
                .Include(r => r.Reviews)   // Include reviews for the recipe
                .ThenInclude(r => r.User)  // Ensure user details for reviews are loaded
                .Include(r => r.CreatedByUser) // Include the user who created the recipe
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
            {
                return NotFound();
            }

            // Pass the recipe and user details to the view
            return View(recipe);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int recipeId, string content, int rating)
        {
            // Find the recipe by ID
            var recipe = await _context.Recipes
                .Include(r => r.Reviews)
                .FirstOrDefaultAsync(r => r.Id == recipeId);

            if (recipe == null)
            {
                return NotFound();
            }

            // Create a new review
            var review = new Review
            {
                RecipeId = recipeId,
                Content = content,
                Rating = rating,
                UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                CreatedAt = DateTime.UtcNow
            };

            // Add the review to the database
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Redirect back to the details page
            return RedirectToAction(nameof(Details), new { id = recipeId });
        }



    }
}
