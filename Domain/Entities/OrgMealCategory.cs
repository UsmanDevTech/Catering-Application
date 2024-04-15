using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class OrgMealCategory
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedDate { get; set; }

        public int OrgMealId { get; set; }
        public OrgMeal OrgMeal { get; set; }

        //private readonly List<OrgMealCatAndOrgMealitem> _orgMealCategoryandOrgMealItem =new();
        //public IReadOnlyCollection<OrgMealCatAndOrgMealitem> orgMealCatAndOrgMealitems =_orgMealCategoryandOrgMealItem.AsReadOnly();


        // Use a property instead of a field
        public List<OrgMealCatAndOrgMealitem> OrgMealCatAndOrgMealitems { get; set; } = new();


    }
}
