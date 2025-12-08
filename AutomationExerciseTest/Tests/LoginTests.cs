using NUnit.Framework;

namespace AutomationExerciseTest.Tests
{
    [TestFixture]
    public class LoginTests : TestBase
    {
        // Test Data
        private string _testName;
        private string _testEmail;
        private readonly string _testPassword = "Test123456";

        // ========================================
        // SETUP - Създаваме user преди всеки login тест
        // ========================================

        [SetUp]
        public new async Task SetupTest()
        {
            // Call base setup first
            await base.SetupTest();

            // Generate unique test data
            _testName = GenerateRandomName();
            _testEmail = GenerateRandomEmail();

            Console.WriteLine($"Creating test user for login tests:");
            Console.WriteLine($"Name: {_testName}");
            Console.WriteLine($"Email: {_testEmail}");

            // Register user first (so we can login with it)
            await HomePage.ClickSignupLogin();
            await LoginPage.PerformSignup(_testName, _testEmail);
            await Wait(2000);

            await SignupPage.QuickRegistration(_testPassword, "Test", "User", GenerateRandomMobile());
            await Wait(3000);

            await SignupPage.ClickContinue();
            await Wait(2000);

            // Logout so we can test login
            await HomePage.ClickLogout();
            await Wait(2000);

            Console.WriteLine("Test user created and logged out - ready for login tests");
        }

        // ========================================
        // TC01: Valid Login Test
        // ========================================

        [Test]
        [Category("Login")]
        [Category("Smoke")]
        public async Task TC01_ValidLogin_WithCorrectCredentials_ShouldLoginSuccessfully()
        {
            // Arrange
            Console.WriteLine("Test: Valid Login with correct credentials");

            // Act - Navigate to Login page
            await HomePage.ClickSignupLogin();
            await Wait(1000);

            // Verify - Login form is visible
            bool isLoginFormVisible = await LoginPage.IsLoginFormVisible();
            Assert.That(isLoginFormVisible, Is.True, "Login form should be visible");

            // Act - Perform login
            await LoginPage.PerformLogin(_testEmail, _testPassword);
            await Wait(2000);

            // Assert - User is logged in
            bool isLoggedIn = await HomePage.IsUserLoggedIn();
            Assert.That(isLoggedIn, Is.True, "User should be logged in");

            // Assert - Username is visible
            string username = await HomePage.GetLoggedInUsername();
            Assert.That(username, Is.EqualTo(_testName), $"Username should be '{_testName}'");
            Console.WriteLine($"Logged in as: {username}");

            // Assert - Logout button is visible
            bool isLogoutVisible = await HomePage.IsLogoutVisible();
            Assert.That(isLogoutVisible, Is.True, "Logout button should be visible");
        }

        // ========================================
        // TC02: Invalid Login Test
        // ========================================

        [Test]
        [Category("Login")]
        [Category("Negative")]
        public async Task TC02_InvalidLogin_WithWrongPassword_ShouldShowErrorMessage()
        {
            // Arrange
            Console.WriteLine("Test: Invalid Login with wrong password");
            string wrongPassword = "WrongPassword123";

            // Act - Navigate to Login page
            await HomePage.ClickSignupLogin();
            await Wait(1000);

            // Act - Attempt login with wrong password
            await LoginPage.PerformLogin(_testEmail, wrongPassword);
            await Wait(2000);

            // Assert - Error message is displayed
            bool isErrorDisplayed = await LoginPage.IsLoginErrorDisplayed();
            Assert.That(isErrorDisplayed, Is.True, "Error message should be displayed");

            // Assert - Error message contains expected text
            string errorMessage = await LoginPage.GetLoginErrorMessage();
            Assert.That(errorMessage, Does.Contain("Your email or password is incorrect").IgnoreCase,
                "Error message should indicate incorrect credentials");
            Console.WriteLine($"Error message: {errorMessage}");

            // Assert - User is NOT logged in
            await HomePage.NavigateToHomePage();
            await Wait(1000);

            bool isLoggedIn = await HomePage.IsUserLoggedIn();
            Assert.That(isLoggedIn, Is.False, "User should NOT be logged in");
        }

        // ========================================
        // TC03: Logout Test
        // ========================================

        [Test]
        [Category("Login")]
        [Category("Smoke")]
        public async Task TC03_Logout_WhenLoggedIn_ShouldLogoutSuccessfully()
        {
            // Arrange - Login first
            Console.WriteLine("Test: Logout after successful login");

            await HomePage.ClickSignupLogin();
            await Wait(1000);

            await LoginPage.PerformLogin(_testEmail, _testPassword);
            await Wait(2000);

            // Verify user is logged in
            bool isLoggedIn = await HomePage.IsUserLoggedIn();
            Assert.That(isLoggedIn, Is.True, "User should be logged in before logout");

            string username = await HomePage.GetLoggedInUsername();
            Console.WriteLine($"Logged in as: {username}");

            // Act - Perform logout
            await HomePage.ClickLogout();
            await Wait(2000);

            // Assert - Redirected to login page
            bool isLoginFormVisible = await LoginPage.IsLoginFormVisible();
            Assert.That(isLoginFormVisible, Is.True, "Should be redirected to login page");

            // Assert - User is no longer logged in
            await HomePage.NavigateToHomePage();
            await Wait(1000);

            isLoggedIn = await HomePage.IsUserLoggedIn();
            Assert.That(isLoggedIn, Is.False, "User should be logged out");

            Console.WriteLine("Logout successful - user returned to login page");
        }
    }
}