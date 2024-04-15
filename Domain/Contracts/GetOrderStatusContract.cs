using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Contracts
{
    public class GetOrderStatusContract
    {
        public string OrderID { get; set; }
        public StatusEnum Status { get; set; }
    }
}
