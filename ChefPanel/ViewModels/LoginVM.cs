using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace ChefPanel.ViewModels
{
    public class LoginVM
    {
        //public string Username { get; set; }

        [Required]
        //[EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required]
        //[DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool IsRemember { get; set; }
        //public bool IsActive { get; set; }

    }
}
