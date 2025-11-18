using Contract_Monthly_Claim_System_CMCS.Models;
using Contract_Monthly_Claim_System_CMCS.Views;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace ClaimsManagementApp.Tests
{
    public class AutomationTests
    {
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly AutomatedVerificationService _verificationService;

        public AutomationTests()
        {
            _mockConfig = new Mock<IConfiguration>();
            SetupConfiguration();
            _verificationService = new AutomatedVerificationService(_mockConfig.Object);
        }

        [Fact]
        public async Task VerifyClaim_ValidClaim_ReturnsAutoApproved()
        {
            // Arrange
            var claim = new Claim
            {
                HoursWorked = 8,
                HourlyRate = 50,
                SubmissionDate = DateTime.Now.AddDays(-1)
            };

            // Act
            var result = await _verificationService.VerifyClaimAsync(claim);

            // Assert
            Assert.True(result.IsApproved);
            Assert.Equal("Auto-Approved", result.Status);
        }

        [Fact]
        public async Task VerifyClaim_ExcessiveHours_ReturnsAutoRejected()
        {
            // Arrange
            var claim = new Claim
            {
                HoursWorked = 15, // Exceeds maximum
                HourlyRate = 50,
                SubmissionDate = DateTime.Now.AddDays(-1)
            };

            // Act
            var result = await _verificationService.VerifyClaimAsync(claim);

            // Assert
            Assert.False(result.IsApproved);
            Assert.Equal("Auto-Rejected", result.Status);
        }

        [Fact]
        public async Task CalculateRecommendedAmount_Overtime_AppliesMultiplier()
        {
            // Arrange
            var claim = new Claim
            {
                HoursWorked = 10,
                HourlyRate = 50
            };

            // Act
            var result = await _verificationService.CalculateRecommendedAmountAsync(claim);

            // Assert
            var expected = (8 * 50) + (2 * 50 * 1.25m); // Normal + Overtime
            Assert.Equal(expected, result);
        }

        private void SetupConfiguration()
        {
            var configurationSection = new Mock<IConfigurationSection>();
            configurationSection.Setup(x => x.Value).Returns("12");

            _mockConfig.Setup(x => x.GetSection("ClaimAutomationSettings:MaxHoursPerDay"))
                      .Returns(configurationSection.Object);
        }
    }
}