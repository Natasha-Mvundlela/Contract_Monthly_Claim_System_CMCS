using System.Diagnostics;
using Contract_Monthly_Claim_System_CMCS.Models;
using demo.Models;
using Microsoft.AspNetCore.Mvc;

namespace Contract_Monthly_Claim_System_CMCS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.WelcomeMessage = "Welcome to the Contract Monthly Claim System";
            ViewBag.SystemDescription = "Streamline your claim submission and approval process with our efficient system.";

            created_queries create = new created_queries();
            create.InitializeSystem();
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
        public IActionResult Claim()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["ErrorMessage"] = "Please login to submit a claim.";
                return RedirectToAction("Login");
            }

            ViewBag.Faculties = new List<string> { "ICT", "Education", "Law", "Commerce", "Humanities", "Finance and Accounting" };
            ViewBag.Modules = new List<string> { "Programming", "Database", "Information Security", "Web Development", "Software Engineering", "Network Fundamentals" };
            ViewBag.CurrentDate = DateTime.Now.ToString("yyyy-MM-dd");
            ViewBag.UserEmail = userEmail;

            return View(new claim { Email_Address = userEmail });
        }

        [HttpPost]
        public IActionResult Claim(claim claims)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["ErrorMessage"] = "Please login to submit a claim.";
                return RedirectToAction("Login");
            }

            ViewBag.Faculties = new List<string> { "ICT", "Education", "Law", "Commerce", "Humanities", "Finance and Accounting" };
            ViewBag.Modules = new List<string> { "Programming", "Database", "Information Security", "Web Development", "Software Engineering", "Network Fundamentals" };

            // Ensure the email is from session
            claims.Email_Address = userEmail;

            if (ModelState.IsValid)
            {
                // Validate claim date
                if (claims.Claim_Date > DateTime.Now)
                {
                    ModelState.AddModelError("Claim_Date", "Claim date cannot be in the future.");
                    return View(claims);
                }

                try
                {
                    // Calculate amount
                    claims.Calculated_Amount = claims.Hours_Worked * claims.Hourly_Rate;

                    // Save claim to database
                    created_queries saveClaim = new created_queries();
                    saveClaim.store_claim(claims.Email_Address, claims.Claim_Date, claims.Faculty,
                                        claims.Module, claims.Hours_Worked, claims.Hourly_Rate,
                                        claims.Calculated_Amount, claims.Supporting_Documents);


                    TempData["SuccessMessage"] = "Claim submitted successfully!";

                    // Redirect based on role
                    var userRole = HttpContext.Session.GetString("UserRole");
                    if (userRole?.ToLower() == "lecturer")
                    {
                        return RedirectToAction("Status");
                    }
                    else
                    {
                        return RedirectToAction("Approval");
                    }

                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while submitting your claim. Please try again.");
                    _logger.LogError(ex, "Error submitting claim");
                }
            }
            return View(claims);
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

            // Get recent claims for the dashboard
            created_queries queries = new created_queries();
            var userClaims = queries.GetUserClaims(userEmail);

            ViewBag.RecentClaims = userClaims.Take(5).ToList();
            ViewBag.TotalClaims = userClaims.Count;
            ViewBag.PendingClaims = userClaims.Count(c => c.Status == "Pending");
            ViewBag.ApprovedClaims = userClaims.Count(c => c.Status == "Approved");

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
            var userClaims = queries.GetUserClaims(userEmail);
            return View(userClaims);
        }

        [HttpGet]
        public IActionResult Approval()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "pc" && userRole != "admin")
            {
                TempData["ErrorMessage"] = "Access denied. You do not have permission to view this page.";
                return RedirectToAction("Dashboard");
            }

            created_queries queries = new created_queries();
            var pendingClaims = queries.GetPendingClaims();
            return View(pendingClaims);
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
        public IActionResult RejectClaim(int claimId, string reason)
        {
            try
            {
                var processedBy = HttpContext.Session.GetString("UserEmail") ?? "System";
                created_queries reject = new created_queries();
                bool success = reject.RejectClaim(claimId, reason, processedBy);

                if (success)
                {
                    TempData["SuccessMessage"] = "Claim rejected successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to reject claim. Please try again.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while rejecting the claim.";
                _logger.LogError(ex, "Error rejecting claim");
            }
            return RedirectToAction("Approval");
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}