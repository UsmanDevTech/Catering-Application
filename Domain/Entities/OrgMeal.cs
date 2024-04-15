using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class OrgMeal
    {
        [Key]
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        //public DateTime Date { get; set; } //
        public WeekDays WeekDay { get; set; }

        public int MealTypeId { get; set; }//FK
        public MealType MealType { get; set; }//Navigation property

        public int OrganizationId { get; set; }//FK
        public  Organization Organization { get; set; }//Navigation Property

        // Navigation property to represent the owning side of the relationship
        public List<OrgMealCategory> OrgMealCategories { get; set; } = new();

    }
}
