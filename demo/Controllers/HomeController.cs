using System.Diagnostics;
using Contract_Monthly_Claim_System_CMCS.Models;
using demo.Models;
using Microsoft.AspNetCore.Mvc;

namespace Contract_Monthly_Claim_System_CMCS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ClaimValidationRules _validationRules;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _validationRules = new ClaimValidationRules();
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.WelcomeMessage = "Welcome to the Contract Monthly Claim System";
            ViewBag.SystemDescription = "Streamline your claim submission and approval process with our efficient system.";

            // Initialize system
            try
            {
                created_queries create = new created_queries();
                create.InitializeSystem();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "System initialization warning");
            }

            return View();
        }

        [HttpGet]
        public IActionResult TestDatabase()
        {
            try
            {
                created_queries queries = new created_queries();
                var testClaims = queries.GetPendingClaims();
                return Content($"Database connection successful. Pending claims: {testClaims.Count}");
            }
            catch (Exception ex)
            {
                return Content($"Database error: {ex.Message}");
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(login user)
        {
            if (ModelState.IsValid && IsValidEmail(user.Email_Address))
            {
                try
                {
                    created_queries check = new created_queries();
                    bool found = check.login_user(user.Email_Address, user.Password, user.Role);

                    if (found)
                    {
                        // Store user information in session
                        HttpContext.Session.SetString("UserEmail", user.Email_Address);
                        HttpContext.Session.SetString("UserRole", user.Role);
                        HttpContext.Session.SetString("IsAuthenticated", "true");

                        TempData["SuccessMessage"] = "Login successful!";

                        Console.WriteLine($"Login successful for: {user.Email_Address}, Role: {user.Role}");

                        // Redirect based on role
                        if (user.Role.ToLower() == "lecturer")
                        {
                            return RedirectToAction("Dashboard");
                        }
                        else if (user.Role.ToLower() == "pc" || user.Role.ToLower() == "admin")
                        {
                            return RedirectToAction("Approval");
                        }
                        else
                        {
                            return RedirectToAction("Dashboard");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid email, password, or role. Please try again.");
                        Console.WriteLine("Login failed: Invalid credentials");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred during login. Please try again.");
                    _logger.LogError(ex, "Error during login");
                    Console.WriteLine($"Login error: {ex.Message}");
                }
            }
            else
            {
                ModelState.AddModelError("", "Please enter valid email and password.");
            }
            return View(user);
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        [HttpGet]
        public IActionResult Claim()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["ErrorMessage"] = "Please login to submit a claim.";
                return RedirectToAction("Login");
            }

            // dropdowns for the claim view
            ViewBag.Faculties = new List<string> {
                "ICT", "Education", "Law", "Commerce", "Humanities", "Finance and Accounting"
            };
            ViewBag.Modules = new List<string> {
                "Programming", "Database", "Information Security", "Web Development",
                "Software Engineering", "Network Fundamentals"
            };
            ViewBag.UserEmail = userEmail;

            // Return the claim view
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Claim(claim claims, List<IFormFile> supportingFiles)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["ErrorMessage"] = "Please login to submit a claim.";
                return RedirectToAction("Login");
            }

            // dropdowns
            ViewBag.Faculties = new List<string> {
                "ICT", "Education", "Law", "Commerce", "Humanities", "Finance and Accounting"
            };
            ViewBag.Modules = new List<string> {
                "Programming", "Database", "Information Security", "Web Development",
                "Software Engineering", "Network Fundamentals"
            };
            ViewBag.UserEmail = userEmail;

            // Ensure the email is from session
            claims.Email_Address = userEmail;

            // AUTOMATION: Auto-calculate amount before validation
            if (claims.Hours_Worked > 0 && claims.Hourly_Rate > 0)
            {
                // This will use the auto-calculated property
                _logger.LogInformation("Auto-calculated amount: {Hours} * {Rate} = {Amount}",
                    claims.Hours_Worked, claims.Hourly_Rate, claims.Calculated_Amount);
            }

            if (ModelState.IsValid)
            {
                // AUTOMATION: Enhanced validation
                var validationResult = ValidateClaim(claims);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                    return View(claims);
                }

                try
                {
                    // Handle file upload
                    var uploadedFileNames = new List<string>();

                    if (supportingFiles != null && supportingFiles.Count > 0)
                    {
                        foreach (var file in supportingFiles)
                        {
                            if (file.Length > 0 && file.Length < 5 * 1024 * 1024) // 5MB limit
                            {
                                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                                if (!Directory.Exists(uploadsFolder))
                                {
                                    Directory.CreateDirectory(uploadsFolder);
                                }

                                var filePath = Path.Combine(uploadsFolder, fileName);

                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    file.CopyTo(stream);
                                }
                                uploadedFileNames.Add(fileName);
                            }
                        }
                    }

                    claims.Supporting_Documents = uploadedFileNames.Any() ?
                        string.Join(",", uploadedFileNames) : "No documents";

                    // Save claim to database
                    created_queries saveClaim = new created_queries();
                    saveClaim.store_claim(
                        claims.Email_Address,
                        claims.Claim_Date,
                        claims.Faculty,
                        claims.Module,
                        claims.Hours_Worked,
                        claims.Hourly_Rate,
                        claims.Calculated_Amount, // Using auto-calculated value
                        claims.Supporting_Documents
                    );

                    // AUTOMATION: Log successful submission with details
                    _logger.LogInformation("Claim submitted successfully - User: {User}, Amount: {Amount}, Faculty: {Faculty}",
                        userEmail, claims.Calculated_Amount, claims.Faculty);

                    TempData["SuccessMessage"] = "Claim submitted successfully! It is now pending approval.";
                    return RedirectToAction("Dashboard");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error submitting claim for user {UserEmail}", userEmail);
                    ModelState.AddModelError("", $"An error occurred while submitting your claim: {ex.Message}");
                }
            }

            return View(claims);
        }

        // AUTOMATION: Enhanced claim validation method
        private ValidationResult ValidateClaim(claim claim)
        {
            var result = new ValidationResult();

            // Check hourly rate against department limits
            if (_validationRules.DepartmentRateLimits.ContainsKey(claim.Faculty))
            {
                var maxRate = _validationRules.DepartmentRateLimits[claim.Faculty];
                if (claim.Hourly_Rate > maxRate)
                {
                    result.Errors.Add($"Hourly rate for {claim.Faculty} cannot exceed R{maxRate}");
                }
            }

            // Check monthly hour limits
            if (claim.Hours_Worked > _validationRules.MaxHoursPerMonth)
            {
                result.Errors.Add($"Hours worked cannot exceed {_validationRules.MaxHoursPerMonth} per month");
            }

            // Check future dates
            if (claim.Claim_Date > DateTime.Now)
            {
                result.Errors.Add("Claim date cannot be in the future");
            }

            // Check for reasonable hourly rates
            if (claim.Hourly_Rate < 50)
            {
                result.Warnings.Add("Hourly rate seems low for academic work");
            }

            return result;
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login");
            }

            // Get claims for the dashboard
            created_queries queries = new created_queries();
            var claimStats = queries.GetClaimStatistics(userEmail);
            var userClaims = claimStats?.Claims ?? new List<claim>();

            ViewBag.RecentClaims = userClaims.Take(5).ToList();
            ViewBag.TotalClaims = userClaims.Count;
            ViewBag.PendingClaims = userClaims.Count(c => c.Status == "Pending");
            ViewBag.ApprovedClaims = userClaims.Count(c => c.Status == "Approved");

            // Calculate total approved amount
            var totalApproved = userClaims
                .Where(c => c.Status == "Approved")
                .Sum(c => c.Calculated_Amount);
            ViewBag.TotalApprovedAmount = totalApproved;

            return View();
        }

        [HttpGet]
        public IActionResult Status()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login"); 
            }

            created_queries queries = new created_queries();
            var userClaims = queries.GetClaimStatistics(userEmail);
            return View(userClaims);
        }

        [HttpGet]
        public IActionResult Approval()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (userRole != "pc" && userRole != "admin")
            {
                TempData["ErrorMessage"] = "Access denied. You do not have permission to view this page.";
                return RedirectToAction("Dashboard");
            }

            created_queries queries = new created_queries();
            var pendingClaims = queries.GetPendingClaims();

            ViewBag.UserRole = userRole;
            ViewBag.UserEmail = userEmail;

            return View(pendingClaims);
        }

        // AUTOMATION: Provide validation insights for coordinators
        private string GetValidationInsight(claim claim)
        {
            var insights = new List<string>();

            if (claim.Hourly_Rate > 800)
            {
                insights.Add("High hourly rate - verify with department standards");
            }

            if (claim.Hours_Worked > 40)
            {
                insights.Add("High hours - confirm with timesheets");
            }

            if (claim.Calculated_Amount > 5000)
            {
                insights.Add("Large amount - may require additional approval");
            }

            return insights.Any() ? string.Join("; ", insights) : "Meets standard criteria";
        }
    }
}

    // AUTOMATION: Validation result class
    public class ValidationResult
    {
        public bool IsValid => !Errors.Any();
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }
