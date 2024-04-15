using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class OrderRating
    {
        [Key]
        public int Id { get; set; }
        public int? Value { get; set; }
        public DateTime PostedDate { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } //navigation
    }
}
