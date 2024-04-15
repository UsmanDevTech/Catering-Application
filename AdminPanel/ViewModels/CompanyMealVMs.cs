using Domain.Enums;

namespace AdminPanel.ViewModels
{
    public class MealTypeGetVM
    {
        public int Id { get; set; }
        public string PictureUrl { get; set; }
        public string Name { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }

    public class OrgMealCreateVM
    {
        public List<int> mealTypeIds { get; set; }
        //public int MealTypeId { get; set; }
        public int WeekDay { get; set; }
        public int companyId { get; set; }
    }

    public class SelectedItemCreateVM
    {
        public string ItemId { get; set; }
        public string CategoryId { get; set; }
    }


    public class EditTimeVM
    {
        public int OrgMealId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int WeekDay { get; set; }
    }

    public class OrgMealEditVM
    {
        public string OrgMealId { get; set; }
        public string MealTypeName { get; set; }
        public string WeekDay { get; set; }
        public int companyId { get; set; }

    }
    public class OrgMealItems
    {
        public int itemId { get; set; }
        public int categoryId { get; set; }
    }

    public class OrgMealCopyVM
    {
        public List<int> WeekDays { get; set; } = new();
        public int OrgMealId { get; set; }

    }

}
