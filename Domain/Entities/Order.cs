using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        public string OrderID { get; set; }
        public StatusEnum Status { get; set; } 
        public string TimeSlot { get; set; }
        public string? SpecialRequest { get; set; }
        public string UserId { get; set; }//Fk

        public string MealName { get; set; }//In case org meal is deleted
        public int OrgMealId { get; set; }//Fk

        public OrderRating Rating { get; set; } //navigation

        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new();

        //private readonly List<OrderItem> _orderItems = new();
        //public IReadOnlyCollection<OrderItem> OrderItems;

        //public Order()
        //{
        //    OrderItems = _orderItems.AsReadOnly();
        //}   

    }
}
