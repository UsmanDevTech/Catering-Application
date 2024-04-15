using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Contracts
{
    public class GetOrgMealContract
    {
        public int Id { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        //public DateTime Date { get; set; } //
        public WeekDays WeekDay { get; set; }

        //Meal
        public int MealTypeId { get; set; }//FK
        public string MealName { get; set; }
        public string MealPictureUrl { get; set; }

        public List<string> TimeSlots { get; set; } = new();

        public List<OrgMealCategoryContract> Categories { get; set; } = new();
    }

    public class OrgMealCategoryContract
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? ImageUrl { get; set; }

        public List<string> Items { get; set; } = new();
    }

}
