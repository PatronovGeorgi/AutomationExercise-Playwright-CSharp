using NUnit.Framework;

namespace AutomationExerciseTest.Tests
{
    [TestFixture]
    public class ContactUsTests : TestBase
    {
        // ========================================
        // TC17: Contact Us with Valid Data
        // ========================================

        [Test]
        [Category("ContactUs")]
        [Category("Smoke")]
        public async Task TC17_ContactUs_WithValidData_ShouldSubmitSuccessfully()
        {
            // Arrange
            Console.WriteLine("Test: Submit contact us form with valid data");

            string name = GenerateRandomName();
            string email = GenerateRandomEmail();
            string subject = "Test Inquiry - Automation";
            string message = "This is an automated test message. Please ignore.";

            Console.WriteLine($"Contact Details:");
            Console.WriteLine($"Name: {name}");
            Console.WriteLine($"Email: {email}");
            Console.WriteLine($"Subject: {subject}");

            // Act - Navigate to Contact Us page
            await HomePage.ClickContactUs();
            await Wait(2000);

            // Verify - Contact Us page loaded
            string pageTitle = await _page.Locator(".contact-form h2.title.text-center").First.TextContentAsync();
            Assert.That(pageTitle, Does.Contain("GET IN TOUCH").IgnoreCase,
                "Contact Us page title should be visible");

            // Act - Fill contact form
            await _page.FillAsync("input[data-qa='name']", name);
            await _page.FillAsync("input[data-qa='email']", email);
            await _page.FillAsync("input[data-qa='subject']", subject);
            await _page.FillAsync("textarea[data-qa='message']", message);
            await Wait(500);

            // Act - Submit form (handle alert)
            _page.Dialog += async (_, dialog) =>
            {
                Console.WriteLine($"Alert message: {dialog.Message}");
                await dialog.AcceptAsync();
            };

            await _page.ClickAsync("input[data-qa='submit-button']");
            await Wait(3000);

            // Assert - Success message is displayed
            var successMessage = await _page.Locator(".status.alert.alert-success").TextContentAsync();
            Assert.That(successMessage, Does.Contain("Success! Your details have been submitted successfully").IgnoreCase,
                "Success message should be displayed");

            Console.WriteLine($"Success message: {successMessage}");
            Console.WriteLine("Contact form submitted successfully");
        }

        // ========================================
        // TC18: Contact Us with Invalid Email
        // ========================================

        [Test]
        [Category("ContactUs")]
        [Category("Negative")]
        public async Task TC18_ContactUs_WithInvalidEmail_ShouldShowValidation()
        {
            // Arrange
            Console.WriteLine("Test: Submit contact us form with invalid email");

            string name = GenerateRandomName();
            string invalidEmail = "invalid-email-format";
            string subject = "Test Subject";
            string message = "Test message";

            Console.WriteLine($"Using invalid email: {invalidEmail}");

            // Act - Navigate to Contact Us page
            await HomePage.ClickContactUs();
            await Wait(2000);

            // Act - Fill form with invalid email
            await _page.FillAsync("input[data-qa='name']", name);
            await _page.FillAsync("input[data-qa='email']", invalidEmail);
            await _page.FillAsync("input[data-qa='subject']", subject);
            await _page.FillAsync("textarea[data-qa='message']", message);
            await Wait(500);

            // Act - Try to submit
            await _page.ClickAsync("input[data-qa='submit-button']");
            await Wait(1000);

            // Assert - HTML5 validation should prevent submission
            var emailField = _page.Locator("input[data-qa='email']");

            // Check if field has validation error
            string validationMessage = await emailField.EvaluateAsync<string>(
                "el => el.validationMessage"
            );

            Console.WriteLine($"Validation message: {validationMessage}");

            // Assert - Form should not be submitted (still on contact page)
            string currentUrl = GetCurrentUrl();
            Assert.That(currentUrl, Does.Contain("/contact_us"),
                "Should remain on contact us page due to validation error");

            Console.WriteLine("Invalid email validation working correctly");
        }
    }
}