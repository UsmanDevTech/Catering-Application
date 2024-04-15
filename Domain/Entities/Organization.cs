using Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Organization : SoftDeletableEntity
    {
        [Key]
        public int Id { get; set; }
        public string CompanyId { get; set; }
        public string ImageUrl { get; set; }
        public string Name { get; set; }
        public string ContactName { get; set; }
        public string Email { get; set; }
        public string Number { get; set; }
        public int Employees { get; set; }
        public string? ChefId { get; set; }//FK

        // Navigation property to represent the owning side of the relationship
        private readonly List<OrgMeal> _orgMeals = new();
        public IReadOnlyCollection<OrgMeal> OrgMeals => _orgMeals.AsReadOnly();
        
    }
}
