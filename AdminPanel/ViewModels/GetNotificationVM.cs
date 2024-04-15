using Domain.Enums;

namespace AdminPanel.ViewModels
{
    public class GetNotificationVM
    {
        public int id { get; set; }
        public string description { get; set; }
        public string userName { get; set; }
        public string userPic { get; set; }
        public bool isSeen { get; set; }
        public NotificationType notifType { get; set; }
    }
}
