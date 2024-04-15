using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }
     //   public int CartItemId { get; set; } //FK
        public string CategoryName { get; set; }
        public string CategoryImage { get; set; }
        public string ItemName { get; set; }

        //Navigation Property
        public int CartId { get; set; }
        public Cart Cart { get; set; }
    }
}
