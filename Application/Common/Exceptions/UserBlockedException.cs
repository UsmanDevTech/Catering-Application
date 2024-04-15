using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Exceptions;

public class UserBlockedException : Exception
{
    public UserBlockedException()
        : base("Your account is blocked. Please contact admin.")
    {
    }

}
