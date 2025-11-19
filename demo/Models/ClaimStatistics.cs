namespace Contract_Monthly_Claim_System_CMCS.Models
{
    public class ClaimStatistics
    {
        public int TotalClaims { get; set; }
        public int PendingClaims { get; set; }
        public int ApprovedClaims { get; set; }
        public int RejectedClaims { get; set; }
        public decimal TotalApprovedAmount { get; set; }
        public decimal AverageApprovedAmount { get; set; }
    }
}
