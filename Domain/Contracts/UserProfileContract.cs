using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Contracts
{
    public class UserProfileContract
    {
        public string? Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? ImageUrl { get; set; }
        public string? OrganizationId { get; set; }
        public List<string> Allergies { get; set; } = new();

        public UserApprovalStatusEnum UserApprovalStatus { get; set; }
        public string? RejectionReason { get; set; }
        public UserType LoginRole { get; set; }
        public string? fcmToken { get; set; }
        public bool IsUserBlocked { get; set; }

    }
}
