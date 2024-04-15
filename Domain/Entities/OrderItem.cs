using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }
        public string Category { get; set; }
        public string CategoryImage { get; set; }

        // i think use MealCategoryId here instead of category
        public string Name { get; set; }

        //
        public int OrderId { get; set; }//fk
        public Order Order { get; set; }//Navigation Property

    }
}
