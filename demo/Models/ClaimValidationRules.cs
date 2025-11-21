namespace Contract_Monthly_Claim_System_CMCS.Models
{
    public class ClaimValidationRules
    {
        public decimal MaxHourlyRate { get; set; } = 1000;
        public int MaxHoursPerMonth { get; set; } = 200;
        public int MinHours { get; set; } = 1;
        public decimal MinHourlyRate { get; set; } = 1;

        // Department-specific rate limits for automation
        public Dictionary<string, decimal> DepartmentRateLimits => new()
        {
            { "ICT", 850 },
            { "Education", 650 },
            { "Law", 900 },
            { "Commerce", 800 },
            { "Humanities", 600 },
            { "Finance and Accounting", 950 }
        };

        // Automated validation method
        public ValidationResult ValidateClaim(claim claim)
        {
            var result = new ValidationResult();

            // Rate validation
            if (claim.Hourly_Rate < MinHourlyRate)
                result.Errors.Add($"Hourly rate must be at least R{MinHourlyRate}");

            if (claim.Hourly_Rate > MaxHourlyRate)
                result.Errors.Add($"Hourly rate cannot exceed R{MaxHourlyRate}");

            if (DepartmentRateLimits.ContainsKey(claim.Faculty) &&
                claim.Hourly_Rate > DepartmentRateLimits[claim.Faculty])
                result.Warnings.Add($"Rate exceeds typical range for {claim.Faculty}");

            // Hours validation
            if (claim.Hours_Worked < MinHours)
                result.Errors.Add($"Hours worked must be at least {MinHours}");

            if (claim.Hours_Worked > MaxHoursPerMonth)
                result.Errors.Add($"Hours worked cannot exceed {MaxHoursPerMonth} per month");

            if (claim.Hours_Worked > 40)
                result.Warnings.Add("High hours worked - ensure proper documentation");

            return result;
        }
    }
}
