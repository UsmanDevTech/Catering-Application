using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Contracts
{
    public class CartItemContract
    {
        public string Category { get; set; }
        public string CategoryImage { get; set; }
        public string ItemName { get; set; }    
    }
}
