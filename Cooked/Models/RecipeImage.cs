using System.ComponentModel.DataAnnotations;

namespace Cooked.Models
{
    public class RecipeImage
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Image URL is required.")]
        [StringLength(200, ErrorMessage = "Image URL cannot exceed 200 characters.")]
        public string ImageUrl { get; set; }

        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }
    }

}
