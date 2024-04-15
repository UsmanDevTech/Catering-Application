using Domain.Enums;

namespace ChefPanel.ViewModels
{
    public class OrganizationsMealsVM
    {
        public int id { get; set; }
        public string PictureUrl { get; set; }
        public string Name { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public WeekDays WeekDay { get; set; }
    }

    public class OrganizationCategoryVM
    {
        public int id { get; set; }
        public string Name { get; set; }
        public int MealTypeId { get; set; }
        public List<MealItems> mealItems { get; set; }
    }

    public class MealItems
    {
        public int MealCatId { get; set; }
        public int id { get; set; }
        public string ItemName { get; set; }
    }
}
