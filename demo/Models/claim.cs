using System.ComponentModel.DataAnnotations;

namespace Contract_Monthly_Claim_System_CMCS.Models
{
    public class claim
    {
        public int ClaimID { get; set; }

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email_Address { get; set; }

        [Required(ErrorMessage = "Claim date is required")]
        public DateTime Claim_Date { get; set; }

        [Required(ErrorMessage = "Faculty is required")]
        public string Faculty { get; set; }

        [Required(ErrorMessage = "Module is required")]
        public string Module { get; set; }

        [Required(ErrorMessage = "Hours worked is required")]
        [Range(1, 200, ErrorMessage = "Hours worked must be between 1 and 200")]
        public int Hours_Worked { get; set; }

        [Required(ErrorMessage = "Hourly rate is required")]
        [Range(1, 1000, ErrorMessage = "Hourly rate must be between R1 and R1000")]
        public decimal Hourly_Rate { get; set; }

        public decimal Calculated_Amount { get; set; }
        public string Supporting_Documents { get; set; }
        public string Status { get; set; } = "Pending";
        public string RejectionReason { get; set; }
        public DateTime SubmittedDate { get; set; } = DateTime.Now;
        public DateTime? ProcessedDate { get; set; }
        public string ProcessedBy { get; set; }
    }
}