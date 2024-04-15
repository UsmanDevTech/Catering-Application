using Domain.Common;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class MealType: SoftDeletableEntity
    {
        [Key]
        public int Id { get; set; }
        public string PictureUrl { get; set; } = null;
        public string Name { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
       
        public UserType CreatedByType { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property to represent the owning side of the relationship
        //private readonly List<MealCategory> _mealcategories = new();
        public List<MealCategory> MealCategories { get; set; } = new();
        
        // Navigation property to represent the owning side of the relationship
        private readonly List<OrgMeal> _orgMeals = new();
        public IReadOnlyCollection<OrgMeal> OrgMeals => _orgMeals.AsReadOnly();

    }
}
