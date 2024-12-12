using System.ComponentModel.DataAnnotations;

namespace Cooked.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Recipe ID is required.")]
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }

        [Required(ErrorMessage = "User ID is required.")]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required(ErrorMessage = "Content is required.")]
        [StringLength(1000, ErrorMessage = "Content must be less than 1000 characters.")]
        public string Content { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int? Rating { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
