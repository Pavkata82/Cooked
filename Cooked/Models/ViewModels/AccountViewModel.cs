namespace Cooked.Models.ViewModels
{
    public class AccountViewModel
    {
        public ApplicationUser User { get; set; }
        public List<Recipe> Recipes { get; set; }
    }

}
