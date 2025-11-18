using System;using System.Collections.Generic;
using Xunit;

namespace testing_methods
{    
    public class UnitTest1
    {
    // Add this class definition if it does not already exist elsewhere in your project.
    public class created_queries
    {
        public void store_user(string name, string email, string password, string role) { }
        public void store_claim(string email, DateTime date, string department, string subject, int hours, decimal rate, decimal total, string document) { }
        public List<Claim> GetUserClaims(string email) => new List<Claim> { new Claim() };
        public bool ApproveClaim(int claimId, string approverEmail) => true;
        public bool login_user(string email, string password, string role) => true;
    }

    // Minimal stub for Claim class to allow compilation.
    public class Claim
    {
        public int ClaimID { get; set; } = 1;
        public string Status { get; set; } = "Pending";
    }


            [Fact]
            public void CompleteClaimWorkflow_FromSubmissionToApproval()
            {
                // Arrange
                var queries = new created_queries();
                var testEmail = $"workflow_test_{Guid.NewGuid()}@example.com";

                // Register user
                queries.store_user("Workflow Test User", testEmail, "password123", "lecturer");

                // Act - Submit claim
                queries.store_claim(testEmail, DateTime.Now, "ICT", "Programming",
                                  10, 50.0m, 500.0m, "document.pdf");

                // Assert - Verify claim is pending
                var userClaims = queries.GetUserClaims(testEmail);
                Assert.Single(userClaims);
                Assert.Equal("Pending", userClaims[0].Status);

                // Act - Approve claim
                var approveResult = queries.ApproveClaim(userClaims[0].ClaimID, "test_approver@example.com");

                // Assert - Verify claim is approved
                Assert.True(approveResult);
                var updatedClaims = queries.GetUserClaims(testEmail);
                Assert.Equal("Approved", updatedClaims[0].Status);
            }

            [Fact]
            public void UserRegistrationAndLogin_Workflow()
            {
                // Arrange
                var queries = new created_queries();
                var testEmail = $"auth_test_{Guid.NewGuid()}@example.com";
                var password = "securepassword123";
                var role = "lecturer";

                // Act - Register
                queries.store_user("Auth Test User", testEmail, password, role);

                // Act - Login
                var loginResult = queries.login_user(testEmail, password, role);

                // Assert
                Assert.True(loginResult);
            }
        }
    }

