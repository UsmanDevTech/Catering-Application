using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Contracts
{
    public class RegisterContract
    {
        public int CompanyId { get; set; }
        public List<string>? Allergies { get; set; }
        [Required(ErrorMessage = "Username is required")]
        public string? FullName { get; set; }
      
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }
        //[Required]
        //[Compare("Password", ErrorMessage = "Password and Confirmation Password does not match")]
        //public string? ConfirmPassword { get; set; }
    }
}
