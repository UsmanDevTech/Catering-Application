using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Contracts
{
    public class OrderHistoryContract
    {
        public int Id { get; set; }
        public string OrderID { get; set; }
        public string MealName { get; set; }
        public string? SpecialRequest { get; set; }
        public string TimeSlot { get; set; }
        public StatusEnum OrderStatus { get; set; }
        public int? Rating { get; set; }
        public List<OrderItemContract> OrderItems { get; set; } = new ();

    }
}
