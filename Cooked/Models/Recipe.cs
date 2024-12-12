using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;  // Required for using List

namespace Cooked.Models
{
    public class Recipe
    {
        public int Id { get; set; }

        // Ensure Title is required and has a maximum length
        [Required(ErrorMessage = "Recipe title is required.")]
        [StringLength(200, ErrorMessage = "Title can't be longer than 200 characters.")]
        public string Title { get; set; }

        // Ensure Description is required and has a maximum length
        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, ErrorMessage = "Description can't be longer than 1000 characters.")]
        public string Description { get; set; }

        // Ensure Instructions are required and has a maximum length
        [Required(ErrorMessage = "Instructions are required.")]
        [StringLength(2000, ErrorMessage = "Instructions can't be longer than 2000 characters.")]
        public string Instructions { get; set; }

        // Category is required
        [Required(ErrorMessage = "Category is required.")]
        public int CategoryId { get; set; }

        // Ingredients as text (from textarea)
        [Required(ErrorMessage = "Ingredients are required.")]
        [StringLength(1000, ErrorMessage = "Ingredients text can't be longer than 1000 characters.")]
        public string IngredientsText { get; set; }

        // Navigation property for Category
        public Category Category { get; set; }

        // Navigation property for Recipe Images (one-to-many relationship)
        public List<RecipeImage> RecipeImages { get; set; }

        public string CreatedByUserId { get; set; } = null;
        public ApplicationUser CreatedByUser { get; set; }  

        public ICollection<Review> Reviews { get; set; }
    }
}
