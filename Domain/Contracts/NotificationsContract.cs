using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Contracts
{
    public class NotificationsContract
    {
        public int Id { get; set; }
        public string Description { get; set; }
        //public string CreatedDate { get; set; }
        public bool IsSeen { get; set; }
        public NotificationType NotifType { get; set; }
    }
}
