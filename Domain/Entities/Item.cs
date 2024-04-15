using System.ComponentModel.DataAnnotations;
using Domain.Common;

namespace Domain.Entities
{
    public class Item: SoftDeletableEntity
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedDate { get; set; }

        private readonly List<OrgMealCatAndOrgMealitem> _orgMealCategoryandOrgMealItems = new();
        
        public IReadOnlyCollection<OrgMealCatAndOrgMealitem> OrgMealCatAndOrgMealitems { get; }

        public Item()
        {
            // Initialize the property inside the constructor
            OrgMealCatAndOrgMealitems = _orgMealCategoryandOrgMealItems.AsReadOnly();
        }
    }
}
