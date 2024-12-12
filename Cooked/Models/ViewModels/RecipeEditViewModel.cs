using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Cooked.Models.ViewModels
{
    public class RecipeEditViewModel
    {
        public int Id { get; set; }  // Add this for Edit functionality
        public string Title { get; set; }
        public string Description { get; set; }
        public string Instructions { get; set; }
        public string IngredientsText { get; set; }
        public int CategoryId { get; set; }
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<IFormFile> NewImages { get; set; } = new List<IFormFile>();
        public List<string> ExistingImages { get; set; } = new List<string>();
        public List<string> ImagesToDelete { get; set; } = new List<string>();
    }
}
