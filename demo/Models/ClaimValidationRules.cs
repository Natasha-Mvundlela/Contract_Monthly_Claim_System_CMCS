namespace Contract_Monthly_Claim_System_CMCS.Models
{
    // model for validation rules
    public class ClaimValidationRules
    {
        public decimal MaxHourlyRate { get; set; } = 1000;
        public int MaxHoursPerMonth { get; set; } = 200;
        public int MinHours { get; set; } = 1;
        public decimal MinHourlyRate { get; set; } = 1;

        // Department-specific rate limits
        public Dictionary<string, decimal> DepartmentRateLimits => new()
        {
            { "ICT", 850 },
            { "Education", 650 },
            { "Law", 900 },
            { "Commerce", 800 },
            { "Humanities", 600 },
            { "Finance and Accounting", 950 }
        };
    }
}
