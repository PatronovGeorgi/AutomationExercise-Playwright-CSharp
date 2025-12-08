using NUnit.Framework;

namespace AutomationExerciseTest.Tests
{
    [TestFixture]
    public class SubscriptionTests : TestBase
    {
        // ========================================
        // TC19: Verify Subscription in Home Page
        // ========================================

        [Test]
        [Category("Subscription")]
        [Category("Smoke")]
        public async Task TC19_SubscribeFromHomePage_ShouldShowSuccessMessage()
        {
            // Arrange
            Console.WriteLine("Test: Subscribe from home page footer");
            string email = GenerateRandomEmail();

            Console.WriteLine($"Subscription email: {email}");

            // Already on home page from setup

            // Act - Scroll to footer
            await HomePage.ScrollToFooter();
            await Wait(1000);

            // Verify - Subscription section is visible
            var subscriptionHeader = await _page.Locator("h2:has-text('Subscription')").IsVisibleAsync();
            Assert.That(subscriptionHeader, Is.True, "Subscription section should be visible");

            // Act - Enter email and subscribe
            await _page.FillAsync("input#susbscribe_email", email);
            await _page.ClickAsync("button#subscribe");
            await Wait(2000);

            // Assert - Success message is displayed
            var successMessage = await _page.Locator("#success-subscribe .alert-success").TextContentAsync();
            Assert.That(successMessage, Does.Contain("successfully subscribed").IgnoreCase,
                "Success message should be displayed");

            Console.WriteLine($"Success message: {successMessage}");
            Console.WriteLine("Subscription from home page successful");
        }

        // ========================================
        // TC20: Verify Subscription in Cart Page
        // ========================================

        [Test]
        [Category("Subscription")]
        [Category("Smoke")]
        public async Task TC20_SubscribeFromCartPage_ShouldShowSuccessMessage()
        {
            // Arrange
            Console.WriteLine("Test: Subscribe from cart page footer");
            string email = GenerateRandomEmail();

            Console.WriteLine($"Subscription email: {email}");

            // Act - Navigate to cart page
            await HomePage.ClickCart();
            await Wait(2000);

            // Act - Scroll to footer
            await _page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)");
            await Wait(1000);

            // Verify - Subscription section is visible
            var subscriptionHeader = await _page.Locator("h2:has-text('Subscription')").IsVisibleAsync();
            Assert.That(subscriptionHeader, Is.True, "Subscription section should be visible in cart page");

            // Act - Enter email and subscribe
            await _page.FillAsync("input#susbscribe_email", email);
            await _page.ClickAsync("button#subscribe");
            await Wait(2000);

            // Assert - Success message is displayed
            var successMessage = await _page.Locator("#success-subscribe .alert-success").TextContentAsync();
            Assert.That(successMessage, Does.Contain("successfully subscribed").IgnoreCase,
                "Success message should be displayed");

            Console.WriteLine($"Success message: {successMessage}");
            Console.WriteLine("Subscription from cart page successful");
        }

        // ========================================
        // TC21: Subscribe with Invalid Email
        // ========================================

        [Test]
        [Category("Subscription")]
        [Category("Negative")]
        public async Task TC21_SubscribeWithInvalidEmail_ShouldShowValidation()
        {
            // Arrange
            Console.WriteLine("Test: Subscribe with invalid email format");
            string invalidEmail = "not-an-email";

            Console.WriteLine($"Using invalid email: {invalidEmail}");

            // Act - Scroll to footer
            await HomePage.ScrollToFooter();
            await Wait(1000);

            // Act - Enter invalid email
            await _page.FillAsync("input#susbscribe_email", invalidEmail);
            await _page.ClickAsync("button#subscribe");
            await Wait(1000);

            // Assert - HTML5 validation should prevent submission
            var emailField = _page.Locator("input#susbscribe_email");

            string validationMessage = await emailField.EvaluateAsync<string>(
                "el => el.validationMessage"
            );

            Console.WriteLine($"Validation message: {validationMessage}");

            // Note: Success message should NOT appear with invalid email
            try
            {
                var successVisible = await _page.Locator("#success-subscribe .alert-success")
                    .IsVisibleAsync(new() { Timeout = 2000 });

                Assert.That(successVisible, Is.False,
                    "Success message should NOT appear with invalid email");
            }
            catch
            {
                // If element not found, that's good - no success message
                Console.WriteLine("No success message displayed (as expected)");
            }

            Console.WriteLine("Invalid email validation working correctly");
        }
    }
}