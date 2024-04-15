using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common
{
    public class CustomInvalidOperationException:Exception
    {
        public CustomInvalidOperationException():base("Invalid Password") { }
        public CustomInvalidOperationException(string message):base(message)
        {
            
        }
    }
}
