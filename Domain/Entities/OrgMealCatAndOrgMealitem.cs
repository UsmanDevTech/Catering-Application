using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class OrgMealCatAndOrgMealitem
    {
        [Key]
        public int Id { get; set; }
       // public int OrgCategoryId { get; set; }//FK

        public int OrgMealCategoryId { get; set; }//FK
        public OrgMealCategory OrgMealCategories { get; set; }//Navigation Property

        public int ItemId { get; set; }
        public Item Items { get; set; }
    }
}
