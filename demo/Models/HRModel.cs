namespace Contract_Monthly_Claim_System_CMCS.Models
{
    public class HRModel
    {
        public int TotalLecturers { get; set; }
        public int ActiveClaims { get; set; }
        public int ClaimsThisMonth { get; set; }
        public decimal TotalPaymentsThisMonth { get; set; }
        public List<PaymentReport> RecentPayments { get; set; } = new List<PaymentReport>();
        public List<LecturerSummary> LecturerSummaries { get; set; } = new List<LecturerSummary>();
    }

    public class PaymentReport
    {
        public string LecturerName { get; set; }
        public string LecturerEmail { get; set; }
        public string Faculty { get; set; }
        public decimal Amount { get; set; }
        public DateTime ClaimDate { get; set; }
        public DateTime ProcessedDate { get; set; }
        public string Status { get; set; }
    }

    public class LecturerSummary
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Faculty { get; set; }
        public int TotalClaims { get; set; }
        public decimal TotalEarnings { get; set; }
        public DateTime LastClaimDate { get; set; }
    }

    public class ReportRequest
    {
        public DateTime StartDate { get; set; } = DateTime.Now.AddMonths(-1);
        public DateTime EndDate { get; set; } = DateTime.Now;
        public string Faculty { get; set; } = "All";
        public string ReportType { get; set; } = "PaymentSummary";
    }

    public class Invoice
    {
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string LecturerName { get; set; }
        public string LecturerEmail { get; set; }
        public string Faculty { get; set; }
        public List<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Generated";
    }

    public class InvoiceItem
    {
        public string Description { get; set; }
        public int Hours { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public DateTime ServiceDate { get; set; }
    }
}