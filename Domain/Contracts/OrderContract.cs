using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Contracts
{
    public class OrderContract
    {
        public int OrderId { get; set; }
        public string SpecialRequest { get; set; }
        public string TimeSlot { get; set; }
        public StatusEnum OrderStatus { get; set; }
    }
}
