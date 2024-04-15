using Domain.Enums;

namespace ChefPanel.ViewModels
{
    public class MealsGetVM
    {
        public int id { get; set; }
        public string picture { get; set; }
        public string name { get; set; }
        public UserType createrType { get; set; }
        public string timmings { get; set; }
        public string createdAt { get; set; }
        public bool isDeleted { get; set; }
    }

    public class MealCreateVM
    {
        public string mealPic { get; set; }
        public string mealName { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public List<MealCatVM> categories { get; set; }
        
    }

    public class MealCatVM
    {
        public string catImage { get; set; }
        public string catName { get; set; }
    }

    public class MealGetVM
    {
        public int id { get; set; }
        public string mealPic { get; set; }
        public string mealName { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public List<mealCatGetVM> categories { get; set; }

    }


    public class mealCatGetVM
    {
        public int catId { get; set; }
        public string catImage { get; set; }
        public string catName { get; set; }
    }

    public class MealUpdateVM
    {
        public int id { get; set; }
        public string mealPic { get; set; }
        public string mealName { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public List<MealCatVM> categories { get; set; }

    }
}
