using Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Domain.Models
{
    public class ApplicationUser:IdentityUser
    {
        public string? OrganizationId { get; set; }
        public string FullName { get; set; }
        public string? ImageUrl { get; set; }
        public UserType LoginRole { get; set; }
       // public string Password { get; set; } = null!;
        public bool RequestForPermanentDelete { get; set; }
        public string? TimezoneId { get; set; }
        public string? fcmToken { get; set; }
        public bool IsUserBlocked { get; set; }
        public UserApprovalStatusEnum UserApprovalStatus { get; set; }
        public string? RejectionReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
