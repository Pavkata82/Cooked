using System.ComponentModel.DataAnnotations;

namespace Cooked.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters.")]
        public string Name { get; set; }

        // Навигационно свойство
        public ICollection<Recipe> Recipes { get; set; }
    }

}
