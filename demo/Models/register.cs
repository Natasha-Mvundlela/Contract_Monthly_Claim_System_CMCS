using System.ComponentModel.DataAnnotations;

namespace Contract_Monthly_Claim_System_CMCS.Models
{
    public class register
    {
        [Required]
        public string Full_Name { get; set; }
        [Required]
        [EmailAddress]
        public string Email_Address { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Role { get; set; }
    }
}
