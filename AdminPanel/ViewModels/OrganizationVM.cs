using Domain.Enums;

namespace AdminPanel.ViewModels
{
    public class OrganizationGetVM
    {
        public int Id { get; set; }
        public string CompanyId { get; set; }
        public string ImageUrl { get; set; }
        public string Name { get; set; }
        public string ContactName { get; set; }
        public string Email { get; set; }
        public string Number { get; set; }
        public string TotalEmployees { private get; set; }
        public string RegisteredEmployees { private get; set; }
        public string ChefId { get; set; }//FK
        public string ChefName { get; set; }

        public bool IsDeleted { get; set; }

        public string Employees => RegisteredEmployees + "/" + TotalEmployees;
    }

    public class OrganizationCreateVM
    {
        public string ImageUrl { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Number { get; set; }
        public string ContactName { get; set; }
        public int TotalEmployees { get; set; }
    }

    public class OrganizationEditVM: OrganizationCreateVM
    {
        public int Id { get; set; }
    }

    public class OrganizationsMealsVM
    {
        public int id { get; set; }
        public string PictureUrl { get; set; }
        public string Name { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public WeekDays WeekDay { get; set; }
    }

    public class mealTypeVM
    {
        public int orgMealId { get; set; }
        public int mealTypeId { get; set; }
        public int companyId { get; set; }
    }
}
