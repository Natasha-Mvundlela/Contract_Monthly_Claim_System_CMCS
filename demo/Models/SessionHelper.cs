using Microsoft.AspNetCore.Http;

namespace Contract_Monthly_Claim_System_CMCS.Models
{
    public static class SessionHelper
    {
        public static void SetUserSession(HttpContext context, string email, string role)
        {
            try
            {
                context.Session.SetString("UserEmail", email);
                context.Session.SetString("UserRole", role);
                context.Session.SetString("IsAuthenticated", "true");
            }
            catch (InvalidOperationException)
            {
                // If session fails, use TempData as fallback
                context.Items["UserEmail"] = email;
                context.Items["UserRole"] = role;
            }
        }

        public static string GetUserEmail(HttpContext context)
        {
            try
            {
                return context.Session.GetString("UserEmail") ??
                       context.Items["UserEmail"] as string ??
                       string.Empty;
            }
            catch (InvalidOperationException)
            {
                return context.Items["UserEmail"] as string ?? string.Empty;
            }
        }

        public static string GetUserRole(HttpContext context)
        {
            try
            {
                return context.Session.GetString("UserRole") ??
                       context.Items["UserRole"] as string ??
                       string.Empty;
            }
            catch (InvalidOperationException)
            {
                return context.Items["UserRole"] as string ?? string.Empty;
            }
        }

        public static bool IsAuthenticated(HttpContext context)
        {
            try
            {
                var auth = context.Session.GetString("IsAuthenticated");
                return auth == "true" || context.Items.ContainsKey("UserEmail");
            }
            catch (InvalidOperationException)
            {
                return context.Items.ContainsKey("UserEmail");
            }
        }

        public static void ClearSession(HttpContext context)
        {
            try
            {
                context.Session.Clear();
            }
            catch (InvalidOperationException)
            {
                // Ignore session errors during logout
            }
            context.Items.Clear();
        }
    }
}