using Domain.Enums;
using System.Runtime.CompilerServices;

namespace ChefPanel.ViewModels
{
    public class UserVM
    {
        public string id { get; set; }
        public string? pictureUrl { get; set; }
        public string fullName { get; set; }
        public string email { get; set; }
        public string phoneNumber { get; set; }
        public string companyId { get; set; }
        public string companyName { get; set; }
        public bool isBlocked { get; set; }
        public string createdAt { get; set; }
        public UserApprovalStatusEnum userStatus { get; set; }
        public string rejectionReason { get; set; }
        public List<UserAllergiesVM> allergies { get; set; }
    }

    public class UserAllergiesVM
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class UserDetailVM
    {
        public string id { get; set; }
        public string? pictureUrl { get; set; }
        public string fullName { get; set; }
        public string email { get; set; }
        public string phoneNumber { get; set; }
        public List<UserAllergiesVM> allergies { get; set; }
    }

    public class CompanyDetailVM
    {
        public string companyId { get; set; }
        public string companyName { get; set; }
        public string companyImage { get; set; }
        public string companyEmail { get; set; }
        public string companyPhoneNumber { get; set; }
    }

    public class ChefVM
    {
        public string id { get; set; }
        public string? pictureUrl { get; set; }
        public string fullName { get; set; }
        public string email { get; set; }
        public string phoneNumber { get; set; }
        public string companyId { get; set; }
        public string companyName { get; set; }
        public bool isBlocked { get; set; }
        public string createdAt { get; set; }
    }

    public class ChefCreateVM
    {
        public string? pictureUrl { get; set; }
        public string fullName { get; set; }
        public string email { get; set; }
        public string phoneNumber { get; set; }
        public string password { get; set; }

    }

    public class ChefUpdateVM
    {
        public string? id { get; set; }
        public string? pictureUrl { get; set; }
        public string fullName { get; set; }
        public string email { get; set; }
        public string phoneNumber { get; set; }
        public string password { get; set; }

    }

    public class GetChefVM
    {
        public string id { get; set; }
        public string? pictureUrl { get; set; }
        public string fullName { get; set; }
        public string email { get; set; }
        public string phoneNumber { get; set; }
        public string password { get; set; }

    }
}
