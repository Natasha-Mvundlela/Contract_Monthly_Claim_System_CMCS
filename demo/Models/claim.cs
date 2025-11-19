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
        [DataType(DataType.Date)]
        public DateTime Claim_Date { get; set; }

        [Required(ErrorMessage = "Faculty is required")]
        public string Faculty { get; set; }

        [Required(ErrorMessage = "Module is required")]
        public string Module { get; set; }

        private int _hoursWorked;
        [Required(ErrorMessage = "Hours worked is required")]
        [Range(1, 200, ErrorMessage = "Hours worked must be between 1 and 200")]
        public int Hours_Worked
        {
            get => _hoursWorked;
            set
            {
                _hoursWorked = value;
                calculateTotal();
            }
        }

        private decimal _hourlyRate;
        [Required(ErrorMessage = "Hourly rate is required")]
        [Range(1, 1000, ErrorMessage = "Hourly rate must be between R1 and R1000")]
        public decimal Hourly_Rate
        {
            get => _hourlyRate;
            set
            {
                _hourlyRate = value;
                calculateAmount();
            }
        }
        // AUTO-CALCULATION: Automatically computed based on hours and rate
        public decimal Calculated_Amount { get; set; } // <-- Change from read-only to settable

        public string Supporting_Documents { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime SubmittedDate { get; set; } = DateTime.Now;
        public DateTime? ProcessedDate { get; set; }
        public string ProcessedBy { get; set; }

        // Automation: Detailed status tracking
        public string DetailedStatus
        {
            get
            {
                return Status switch
                {
                    "Pending" => "Awaiting Review",
                    "Approved" => ProcessedDate.HasValue ? "Approved - Awaiting Payment" : "Approved",
                    "Rejected" => "Rejected",
                    _ => Status
                };
            }
        }

        // Automation: Progress tracking
        public int ProgressPercentage
        {
            get
            {
                return Status switch
                {
                    "Pending" => 25,
                    "Approved" => 75,
                    "Rejected" => 100,
                    _ => 0
                };
            }
        }

        //  method to auto calculate total amount
        private void calculateTotal()
        {
            Calculated_Amount = Hours_Worked * Hourly_Rate;
        }

        // calculateAmount to call calculateTotal for consistency
        private void calculateAmount()
        {
            calculateTotal();
        }
    }
}
