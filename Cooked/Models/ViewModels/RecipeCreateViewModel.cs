using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Cooked.Models.ViewModels
{
    public class RecipeCreateViewModel
    {
        [Required(ErrorMessage = "Recipe title is required.")]
        [StringLength(200, ErrorMessage = "Title can't be longer than 200 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, ErrorMessage = "Description can't be longer than 1000 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Instructions are required.")]
        [StringLength(2000, ErrorMessage = "Instructions can't be longer than 2000 characters.")]
        public string Instructions { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Ingredients are required.")]
        [StringLength(1000, ErrorMessage = "Ingredients text can't be longer than 1000 characters.")]
        public string IngredientsText { get; set; }

        // Added property for image files
        [Required(ErrorMessage = "At least one image is required.")]
        public IFormFile[] Images { get; set; }  // To handle multiple file uploads

        [ValidateNever]
        public IEnumerable<Category> Categories { get; set; }
    }
}
