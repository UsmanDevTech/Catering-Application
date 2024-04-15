using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsSeen { get; set; }
        public NotificationType NotifType { get; set; }

        public string NotifiedToId { get; set; }
        public string NotifiedById { get; set; }

    }
}
