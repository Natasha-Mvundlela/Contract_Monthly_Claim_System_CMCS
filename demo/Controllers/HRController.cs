// HRController.cs
using System.Text;
using Contract_Monthly_Claim_System_CMCS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contract_Monthly_Claim_System_CMCS.Controllers
{
    [Authorize(Roles = "admin,hr")]
    public class HRController : Controller
    {
        private readonly created_queries _queries = new created_queries();

        public IActionResult Dashboard()
        {
            try
            {
                var dashboard = new HRModel
                {
                    LecturerSummaries = _queries.GetLecturerSummaries(),
                    RecentPayments = _queries.GetApprovedClaimsForPayment(DateTime.Now.AddDays(-30), DateTime.Now)
                };

                dashboard.TotalLecturers = dashboard.LecturerSummaries.Count;
                dashboard.ActiveClaims = dashboard.RecentPayments.Count;
                dashboard.ClaimsThisMonth = _queries.GeneratePaymentReport(
                    new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                    DateTime.Now
                ).Count;
                dashboard.TotalPaymentsThisMonth = dashboard.RecentPayments.Sum(p => p.Amount);

                return View(dashboard);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading HR dashboard: {ex.Message}";
                return View(new HRModel());
            }
        }

        public IActionResult PaymentReports()
        {
            var model = new ReportRequest();
            return View(model);
        }

        [HttpPost]
        public IActionResult GenerateReport(ReportRequest request)
        {
            try
            {
                var reportData = _queries.GeneratePaymentReport(request.StartDate, request.EndDate, request.Faculty);

                if (request.ReportType == "CSV")
                {
                    return GenerateCsvReport(reportData, request);
                }
                else if (request.ReportType == "PDF")
                {
                    return GeneratePdfReport(reportData, request);
                }
                else
                {
                    ViewBag.ReportData = reportData;
                    ViewBag.ReportRequest = request;
                    return View("PaymentReportView", reportData);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error generating report: {ex.Message}";
                return RedirectToAction("PaymentReports");
            }
        }

        public IActionResult LecturerManagement()
        {
            var lecturers = _queries.GetLecturerSummaries();
            return View(lecturers);
        }

        [HttpPost]
        public IActionResult UpdateLecturer(string email, string fullName, string faculty, string contactInfo)
        {
            try
            {
                var result = _queries.UpdateLecturerInfo(email, fullName, faculty, contactInfo);
                if (result)
                {
                    TempData["SuccessMessage"] = "Lecturer information updated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update lecturer information.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating lecturer: {ex.Message}";
            }

            return RedirectToAction("LecturerManagement");
        }

        public IActionResult GenerateInvoices()
        {
            var approvedClaims = _queries.GetApprovedClaimsForPayment(DateTime.Now.AddMonths(-1), DateTime.Now);
            var invoices = new List<Invoice>();

            foreach (var claim in approvedClaims)
            {
                var invoice = new Invoice
                {
                    InvoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{claim.LecturerEmail.GetHashCode():X}",
                    InvoiceDate = DateTime.Now,
                    LecturerName = claim.LecturerName,
                    LecturerEmail = claim.LecturerEmail,
                    Faculty = claim.Faculty,
                    TotalAmount = claim.Amount,
                    Items = new List<InvoiceItem>
                    {
                        new InvoiceItem
                        {
                            Description = $"Teaching services - {claim.ClaimDate:MMMM yyyy}",
                            Hours = (int)claim.Amount / 100, // Example calculation
                            Rate = 100, // Example rate
                            Amount = claim.Amount,
                            ServiceDate = claim.ClaimDate
                        }
                    }
                };
                invoices.Add(invoice);
            }

            return View(invoices);
        }

        // CSV Report Generation
        private IActionResult GenerateCsvReport(List<PaymentReport> reportData, ReportRequest request)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Lecturer Name,Email,Faculty,Amount,Claim Date,Processed Date,Status");

            foreach (var item in reportData)
            {
                csv.AppendLine($"\"{item.LecturerName}\",\"{item.LecturerEmail}\",\"{item.Faculty}\",{item.Amount},{item.ClaimDate:yyyy-MM-dd},{item.ProcessedDate:yyyy-MM-dd},{item.Status}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"PaymentReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        // Simple PDF Report Generation (using HTML to PDF concept)
        private IActionResult GeneratePdfReport(List<PaymentReport> reportData, ReportRequest request)
        {
            // In a real implementation, you would use a library like iTextSharp, QuestPDF, or Rotativa
            // This is a simplified version that returns HTML that can be printed as PDF
            var htmlContent = GeneratePdfHtml(reportData, request);
            var bytes = Encoding.UTF8.GetBytes(htmlContent);
            return File(bytes, "text/html", $"PaymentReport_{DateTime.Now:yyyyMMdd_HHmmss}.html");
        }

        private string GeneratePdfHtml(List<PaymentReport> reportData, ReportRequest request)
        {
            var html = new StringBuilder();
            html.AppendLine($@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 20px; }}
                        .header {{ text-align: center; margin-bottom: 30px; }}
                        .table {{ width: 100%; border-collapse: collapse; }}
                        .table th, .table td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
                        .table th {{ background-color: #f2f2f2; }}
                        .total {{ font-weight: bold; margin-top: 20px; }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <h1>Payment Report</h1>
                        <p>Period: {request.StartDate:dd MMM yyyy} to {request.EndDate:dd MMM yyyy}</p>
                        <p>Generated on: {DateTime.Now:dd MMM yyyy HH:mm}</p>
                    </div>
                    <table class='table'>
                        <thead>
                            <tr>
                                <th>Lecturer Name</th>
                                <th>Email</th>
                                <th>Faculty</th>
                                <th>Amount</th>
                                <th>Claim Date</th>
                                <th>Processed Date</th>
                                <th>Status</th>
                            </tr>
                        </thead>
                        <tbody>");

            foreach (var item in reportData)
            {
                html.AppendLine($@"
                    <tr>
                        <td>{item.LecturerName}</td>
                        <td>{item.LecturerEmail}</td>
                        <td>{item.Faculty}</td>
                        <td>R {item.Amount:F2}</td>
                        <td>{item.ClaimDate:dd MMM yyyy}</td>
                        <td>{item.ProcessedDate:dd MMM yyyy}</td>
                        <td>{item.Status}</td>
                    </tr>");
            }

            html.AppendLine($@"
                        </tbody>
                    </table>
                    <div class='total'>
                        Total Records: {reportData.Count}<br>
                        Total Amount: R {reportData.Where(x => x.Status == "Approved").Sum(x => x.Amount):F2}
                    </div>
                </body>
                </html>");

            return html.ToString();
        }
    }
}
