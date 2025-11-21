using System.ComponentModel.DataAnnotations;

namespace Contract_Monthly_Claim_System_CMCS.Models
{
    // Updated claim model with automation
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
                CalculateTotal();
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
                CalculateTotal();
            }
        }

        // AUTO-CALCULATION: Automatically computed based on hours and rate
        public decimal Calculated_Amount { get; set; }

        // Automation: Auto-validation status
        public string AutoValidationStatus { get; set; } = "Pending";
        public List<string> ValidationMessages { get; set; } = new List<string>();

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
                    "Pending" => AutoValidationStatus == "Valid" ? "Awaiting Review" : "Requires Attention",
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
                    "Pending" => AutoValidationStatus == "Valid" ? 50 : 25,
                    "Approved" => 75,
                    "Rejected" => 100,
                    _ => 0
                };
            }
        }

        // Automation: Auto-validation method
        public void PerformAutoValidation()
        {
            ValidationMessages.Clear();

            // Faculty-based rate validation
            var rateLimits = new ClaimValidationRules().DepartmentRateLimits;
            if (rateLimits.ContainsKey(Faculty) && Hourly_Rate > rateLimits[Faculty])
            {
                ValidationMessages.Add($"Hourly rate exceeds recommended limit for {Faculty} (Max: R{rateLimits[Faculty]})");
            }

            // Hours validation
            if (Hours_Worked > 40)
            {
                ValidationMessages.Add("High hours worked - may require additional verification");
            }

            // Amount validation
            if (Calculated_Amount > 5000)
            {
                ValidationMessages.Add("Large claim amount - will be prioritized for review");
            }

            // Set validation status
            AutoValidationStatus = ValidationMessages.Any() ? "Requires Review" : "Valid";
        }

        // Method to auto calculate total amount
        private void CalculateTotal()
        {
            Calculated_Amount = Hours_Worked * Hourly_Rate;
            PerformAutoValidation(); // Auto-validate when values change
        }
    }
}
