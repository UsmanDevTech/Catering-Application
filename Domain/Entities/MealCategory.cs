using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class MealCategory
    {
        [Key]
        public int Id { get; set; }
        public string? ImageUrl { get; set; }
        public string Name { get; set; } = null!;
       
        public int MealTypeId { get; set; }//fk
        public MealType MealType { get; set; }

    }
}
