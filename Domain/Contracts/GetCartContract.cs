using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Contracts
{
    public class GetCartContract
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<CartItemContract> CartItems { get; set; } = new List<CartItemContract>();
        public int OrgMealId { get; set; }
        public List<string> TimeSlots { get; set; } = new();

    }
}