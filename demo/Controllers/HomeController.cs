using System.Diagnostics;
using Contract_Monthly_Claim_System_CMCS.Models;
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

        [HttpPost]
        public IActionResult Index(register user)
        {
            if (ModelState.IsValid && IsValidEmail(user.Email_Address))
            {
                try
                {
                    created_queries create_user = new created_queries();
                    create_user.store_user(user.Full_Name, user.Email_Address, user.Password, user.Role);
                    TempData["SuccessMessage"] = "Registration successful! Please login.";
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred during registration. Please try again.");
                    _logger.LogError(ex, "Error during registration");
                }
            }
            return View(user);
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
        public IActionResult Claim(claim model, List<IFormFile> supportingFiles)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var userEmail = SessionHelper.GetUserEmail(HttpContext);
                    if (string.IsNullOrEmpty(userEmail))
                    {
                        TempData["ErrorMessage"] = "Please login to submit a claim.";
                        return RedirectToAction("Login");
                    }

                    model.Email_Address = userEmail;

                    // Auto-calculation (already done in model properties)
                    Console.WriteLine($"Auto-calculated amount: {model.Hours_Worked} * {model.Hourly_Rate} = {model.Calculated_Amount}");

                    // Perform auto-validation
                    var validator = new ClaimValidationRules();
                    var validationResult = validator.ValidateClaim(model);

                    // Store validation messages
                    if (validationResult.Warnings.Any())
                    {
                        TempData["ValidationWarnings"] = string.Join("; ", validationResult.Warnings);
                    }

                    // Handle file uploads
                    var fileNames = new List<string>();
                    if (supportingFiles != null && supportingFiles.Count > 0)
                    {
                        foreach (var file in supportingFiles)
                        {
                            if (file.Length > 0 && file.Length <= 5 * 1024 * 1024)
                            {
                                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                                fileNames.Add(fileName);
                            }
                        }
                    }

                    // Store claim
                    created_queries queries = new created_queries();
                    queries.store_claim(
                        model.Email_Address,
                        model.Claim_Date,
                        model.Faculty,
                        model.Module,
                        model.Hours_Worked,
                        model.Hourly_Rate,
                        model.Calculated_Amount,
                        string.Join(";", fileNames)
                    );

                    TempData["SuccessMessage"] = "Claim submitted successfully! Automated validation completed.";

                    if (validationResult.Warnings.Any())
                    {
                        TempData["InfoMessage"] = "Note: " + string.Join(" ", validationResult.Warnings);
                    }

                    return RedirectToAction("Status");
                }

                ViewBag.Faculties = GetFaculties();
                ViewBag.Modules = GetModules();
                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Claim submission error: {ex.Message}");
                TempData["ErrorMessage"] = $"Error submitting claim: {ex.Message}";
                ViewBag.Faculties = GetFaculties();
                ViewBag.Modules = GetModules();
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult DebugClaims()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return Content("Not logged in");
            }

            created_queries queries = new created_queries();
            var claims = queries.GetUserClaims(userEmail);

            return Content($"User: {userEmail}, Claims Count: {claims.Count}\n" +
                          string.Join("\n", claims.Select(c => $"Claim {c.ClaimID}: {c.Module} - {c.Status}")));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveClaim(int claimId)
        {
            try
            {
                var processedBy = HttpContext.Session.GetString("UserEmail") ?? "System";
                created_queries approve = new created_queries();
                bool success = approve.ApproveClaim(claimId, processedBy);

                if (success)
                {
                    TempData["SuccessMessage"] = "Claim approved successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to approve claim. Please try again.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while approving the claim.";
                _logger.LogError(ex, "Error approving claim");
            }
            return RedirectToAction("Approval");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectClaim(int claimId, string reason, string processedBy)
        {
            var userEmail = SessionHelper.GetUserEmail(HttpContext);
            var userRole = SessionHelper.GetUserRole(HttpContext);


            

            if (userRole != "pc" && userRole != "admin")
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToAction("Dashboard");
            }
             created_queries reject = new created_queries();
                var result = reject.RejectClaim(claimId, reason, processedBy);
            if (result)
            {
                TempData["SuccessMessage"] = "Claim rejected successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to reject claim.";
            }

            return RedirectToAction("Approval");
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
            try
            {
                var userEmail = SessionHelper.GetUserEmail(HttpContext);
                var userRole = SessionHelper.GetUserRole(HttpContext);

                if (string.IsNullOrEmpty(userEmail))
                {
                    TempData["ErrorMessage"] = "Please login to access the dashboard.";
                    return RedirectToAction("Login");
                }

                Console.WriteLine($"Dashboard: Loading data for user {userEmail}");

                // Get user claims
                created_queries query = new created_queries();
                var userClaims = query.GetUserClaims(userEmail);
                Console.WriteLine($"Dashboard: Retrieved {userClaims.Count} claims");

                // Get statistics
                created_queries queries = new created_queries();
                var statistics = queries.GetClaimStatistics(userEmail);
                Console.WriteLine($"Dashboard Stats - Total: {statistics.TotalClaims}, Pending: {statistics.PendingClaims}, Approved: {statistics.ApprovedClaims}, Amount: R{statistics.TotalApprovedAmount}");
                // Pass data to view
                ViewBag.TotalClaims = statistics.TotalClaims;
                ViewBag.PendingClaims = statistics.PendingClaims;
                ViewBag.ApprovedClaims = statistics.ApprovedClaims;
                ViewBag.TotalApprovedAmount = statistics.TotalApprovedAmount;
                ViewBag.AverageApprovedAmount = statistics.AverageApprovedAmount;

                // Get recent claims (last 5)
                ViewBag.RecentClaims = userClaims.Take(5).ToList();

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Dashboard error: {ex.Message}");
                TempData["ErrorMessage"] = "Error loading dashboard data.";
                return View();
            }
        }

        [HttpGet]
        public IActionResult TestDataRetrieval()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return Content("Not logged in");
            }

            try
            {
                created_queries queries = new created_queries();
                var userClaims = queries.GetUserClaims(userEmail);

                var result = $"User: {userEmail}\n";
                result += $"Total Claims: {userClaims.Count}\n";
                result += "All Claims:\n";

                foreach (var claim in userClaims)
                {
                    result += $"- ID: {claim.ClaimID}, Module: {claim.Module}, Status: {claim.Status}, Amount: R{claim.Calculated_Amount:F2}\n";
                }

                return Content(result);
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}\n\n{ex.StackTrace}");
            }
        }

        [HttpGet]
        public IActionResult TestSession()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userRole = HttpContext.Session.GetString("UserRole");
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");

            return Content($"Session Data:\nEmail: {userEmail}\nRole: {userRole}\nAuthenticated: {isAuthenticated}");
        }

        [HttpGet]
        public IActionResult Status()
        {
            try
            {
                var userEmail = SessionHelper.GetUserEmail(HttpContext);
                if (string.IsNullOrEmpty(userEmail))
                {
                    TempData["ErrorMessage"] = "Please login to view your claims.";
                    return RedirectToAction("Login");
                }

                Console.WriteLine($"Status: Loading claims for user {userEmail}");

                created_queries queries = new created_queries();
                var claims = queries.GetUserClaims(userEmail);
                Console.WriteLine($"Status: Retrieved {claims.Count} claims");

                // Debug: Check what's in the database
                queries.DebugDatabaseContents();

                return View(claims);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Status error: {ex.Message}");
                TempData["ErrorMessage"] = "Error loading your claims.";
                return View(new List<claim>());
            }
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

        [HttpPost]
        public IActionResult ApprovedClaim(int claimId)
        {
            var userEmail = SessionHelper.GetUserEmail(HttpContext);
            var userRole = SessionHelper.GetUserRole(HttpContext);

            if (userRole != "pc" && userRole != "admin")
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToAction("Dashboard");
            }
            created_queries queries = new created_queries();
            var result = queries.ApproveClaim(claimId, userEmail);
            if (result)
            {
                TempData["SuccessMessage"] = "Claim approved successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to approve claim.";
            }

            return RedirectToAction("Approval");
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

        private List<string> GetFaculties()
        {
            return new List<string> {
                "ICT", "Education", "Law", "Commerce", "Humanities", "Finance and Accounting"
            };
        }

        private List<string> GetModules()
        {
            return new List<string> {
                "Programming", "Database", "Information Security", "Web Development",
                "Software Engineering", "Network Fundamentals"
            };
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
