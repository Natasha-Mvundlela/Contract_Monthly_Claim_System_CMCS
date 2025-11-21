using System.ComponentModel.DataAnnotations;

namespace Contract_Monthly_Claim_System_CMCS.Models
{
    public class register
    {
        [Required(ErrorMessage = "Full name is required")]
        public string Full_Name { get; set; }

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email_Address { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; }
    }
}
