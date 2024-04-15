using NetTopologySuite.Noding;

namespace ChefPanel.ViewModels
{
    public class OrderVM
    {
        public int id { get; set; }
        public string orderId { get; set; }
        public string userId { get; set; }
        public string userPic { get; set; }
        public string userName { get; set; }
        public string companyName { get; set; }
        public string chefName { get; set; }
        public string? timeSlot { get; set; }
        public string? orderReceived { get; set; }
        public string? orderDelivered { get; set; }
        public int? orderRating { get; set; }
        public int orderStatus { get; set; }
        public string mealName { get; set; }
        public string mealChoice { get; set; }
        public string mealSide { get; set; }
        public string specialRequest { get; set; }
    }

    public class orderItem
    {
        public int id { get; set; }
        public string itemName { get; set; }
        public string categoryIcon { get; set; }
        public string categoryName { get; set; }
    }

    public class GetOrderDetailVM
    {
        public int id { get; set; }
        public string orderId { get; set; }
        public int orderStatus { get; set; }
        public List<orderItem> orderItems { get; set; }
        public string specialRequest { get; set; }
        public string timeSlot { get; set; }
        public int? orderRating { get; set; }

    }

    public class Companies
    {
        public int id { get; set; }
        public string companyName { get; set; }
        public string companyUrl { get; set; }
    }
    public class Chefs
    {
        public string id { get; set; }
        public string chefName { get; set; }
        public string chefPic { get; set; }
    }
    public class GetOrdersCompaniesVM
    {
        public List<Companies> companies { get; set; }
        public List<Chefs> chefs { get; set; }
    }
}
