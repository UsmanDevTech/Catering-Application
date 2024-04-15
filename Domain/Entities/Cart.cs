
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }
        public int OrgMealId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UserId { get; set; } = null!; //FK AppliationUser

        //Navigation Property
        public List<CartItem> CartItems { get; } 

    }
}
