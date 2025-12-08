using NUnit.Framework;

namespace AutomationExerciseTest.Tests
{
    [TestFixture]
    public class SignupTests : TestBase
    {
        // Test Data
        private string _testName;
        private string _testEmail;
        private readonly string _testPassword = "Test123456";

        // ========================================
        // SETUP - Генерираме уникални test data преди всеки тест
        // ========================================

        [SetUp]
        public new async Task SetupTest()
        {
            // Call base setup first
            await base.SetupTest();

            // Generate unique test data
            _testName = GenerateRandomName();
            _testEmail = GenerateRandomEmail();

            Console.WriteLine($"Test Data Generated:");
            Console.WriteLine($"Name: {_testName}");
            Console.WriteLine($"Email: {_testEmail}");
        }

        // ========================================
        // TC04: Register New User - Full Flow
        // ========================================

        [Test]
        [Category("Signup")]
        [Category("Smoke")]
        public async Task TC04_RegisterNewUser_WithValidData_ShouldRegisterSuccessfully()
        {
            // Arrange
            Console.WriteLine("Test: Register new user with valid data");

            // Act - Navigate to Signup page
            await HomePage.ClickSignupLogin();
            await Wait(1000);

            // Verify - Signup form is visible
            bool isSignupFormVisible = await LoginPage.IsSignupFormVisible();
            Assert.That(isSignupFormVisible, Is.True, "Signup form should be visible");

            // Act - Enter signup details (Step 1)
            await LoginPage.PerformSignup(_testName, _testEmail);
            await Wait(2000);

            // Verify - Navigated to account information page
            bool isAccountInfoVisible = await SignupPage.IsAccountInfoHeaderVisible();
            Assert.That(isAccountInfoVisible, Is.True, "Account Information page should be visible");

            // Act - Complete registration (Step 2)
            await SignupPage.CompleteRegistration(
                title: "Mr",
                password: _testPassword,
                day: "15",
                month: "5",
                year: "1990",
                firstName: "Test",
                lastName: "User",
                company: "Test Company",
                address: "123 Test Street",
                address2: "Apt 456",
                country: "India",
                state: "Test State",
                city: "Test City",
                zipcode: "12345",
                mobileNumber: GenerateRandomMobile(),
                newsletter: true,
                offers: true
            );

            await Wait(3000);

            // Assert - Account created message is visible
            bool isAccountCreated = await SignupPage.IsAccountCreatedMessageVisible();
            Assert.That(isAccountCreated, Is.True, "Account created message should be visible");

            string successMessage = await SignupPage.GetAccountCreatedMessage();
            Console.WriteLine($"Success Message: {successMessage}");
            Assert.That(successMessage, Does.Contain("ACCOUNT CREATED").IgnoreCase);

            // Act - Click Continue
            await SignupPage.ClickContinue();
            await Wait(2000);

            // Assert - User is logged in automatically
            bool isLoggedIn = await HomePage.IsUserLoggedIn();
            Assert.That(isLoggedIn, Is.True, "User should be logged in after registration");

            string username = await HomePage.GetLoggedInUsername();
            Assert.That(username, Is.EqualTo(_testName), $"Username should be '{_testName}'");
            Console.WriteLine($"Successfully registered and logged in as: {username}");
        }

        // ========================================
        // TC05: Register with Existing Email (Negative)
        // ========================================

        [Test]
        [Category("Signup")]
        [Category("Negative")]
        public async Task TC05_RegisterWithExistingEmail_ShouldShowErrorMessage()
        {
            // Arrange - First, register a user
            Console.WriteLine("Test: Register with existing email (negative test)");

            await HomePage.ClickSignupLogin();
            await LoginPage.PerformSignup(_testName, _testEmail);
            await Wait(2000);

            // Complete first registration
            await SignupPage.QuickRegistration(_testPassword, "Test", "User", GenerateRandomMobile());
            await Wait(3000);

            await SignupPage.ClickContinue();
            await Wait(2000);

            // Logout
            await HomePage.ClickLogout();
            await Wait(2000);

            // Act - Try to register again with same email
            await HomePage.ClickSignupLogin();
            await Wait(1000);

            string newName = GenerateRandomName();
            await LoginPage.PerformSignup(newName, _testEmail); // Same email!
            await Wait(2000);

            // Assert - Error message is displayed
            bool isErrorDisplayed = await LoginPage.IsEmailExistsErrorDisplayed();
            Assert.That(isErrorDisplayed, Is.True, "Email exists error should be displayed");

            Console.WriteLine("Error correctly displayed: Email already exists");

            // Verify we're still on signup/login page (not navigated to account info)
            bool isSignupFormStillVisible = await LoginPage.IsSignupFormVisible();
            Assert.That(isSignupFormStillVisible, Is.True, "Should remain on signup page");
        }

        // ========================================
        // TC06: Delete Account
        // ========================================

        [Test]
        [Category("Signup")]
        [Category("Smoke")]
        public async Task TC06_DeleteAccount_WhenLoggedIn_ShouldDeleteSuccessfully()
        {
            // Arrange - Register and login first
            Console.WriteLine("Test: Delete account after registration");

            // Register new user
            await HomePage.ClickSignupLogin();
            await LoginPage.PerformSignup(_testName, _testEmail);
            await Wait(2000);

            await SignupPage.QuickRegistration(_testPassword, "Test", "User", GenerateRandomMobile());
            await Wait(3000);

            await SignupPage.ClickContinue();
            await Wait(2000);

            // Verify user is logged in
            bool isLoggedIn = await HomePage.IsUserLoggedIn();
            Assert.That(isLoggedIn, Is.True, "User should be logged in before deletion");

            string username = await HomePage.GetLoggedInUsername();
            Console.WriteLine($"Logged in as: {username}");

            // Act - Delete account
            await HomePage.ClickDeleteAccount();
            await Wait(3000);

            // Assert - Account deleted message is visible
            bool isAccountDeleted = await LoginPage.IsAccountDeletedMessageVisible();
            Assert.That(isAccountDeleted, Is.True, "Account deleted message should be visible");

            string deleteMessage = await LoginPage.GetAccountDeletedMessage();
            Console.WriteLine($"Delete Message: {deleteMessage}");
            Assert.That(deleteMessage, Does.Contain("ACCOUNT DELETED").IgnoreCase);

            // Act - Click Continue after deletion
            await SignupPage.ClickContinue();
            await Wait(2000);

            // Assert - User is logged out
            isLoggedIn = await HomePage.IsUserLoggedIn();
            Assert.That(isLoggedIn, Is.False, "User should be logged out after account deletion");

            Console.WriteLine("Account successfully deleted");
        }
    }
}
