using NUnit.Framework;

namespace AutomationExerciseTest.Tests
{
    [TestFixture]
    public class NavigationTests : TestBase
    {
        // ========================================
        // TC32: Verify Header Navigation Links
        // ========================================

        [Test]
        [Category("Navigation")]
        [Category("Smoke")]
        public async Task TC32_VerifyHeaderLinks_ShouldNavigateCorrectly()
        {
            // Arrange
            Console.WriteLine("Test: Verify all header navigation links");

            // Test Home link
            await _page.ClickAsync("a[href='/']");
            await Wait(1000);
            string homeUrl = GetCurrentUrl();
            Assert.That(homeUrl, Does.Contain("/"), "Home link should navigate to homepage");
            Console.WriteLine("✓ Home link works");

            // Test Products link
            await HomePage.ClickProducts();
            await Wait(2000);
            string productsUrl = GetCurrentUrl();
            Assert.That(productsUrl, Does.Contain("/products"), "Products link should work");
            Console.WriteLine("✓ Products link works");

            // Test Cart link
            await HomePage.ClickCart();
            await Wait(2000);
            string cartUrl = GetCurrentUrl();
            Assert.That(cartUrl, Does.Contain("/view_cart"), "Cart link should work");
            Console.WriteLine("✓ Cart link works");

            // Test Signup/Login link
            await HomePage.ClickSignupLogin();
            await Wait(2000);
            string loginUrl = GetCurrentUrl();
            Assert.That(loginUrl, Does.Contain("/login"), "Login link should work");
            Console.WriteLine("✓ Signup/Login link works");

            // Test Contact Us link
            await HomePage.ClickContactUs();
            await Wait(2000);
            string contactUrl = GetCurrentUrl();
            Assert.That(contactUrl, Does.Contain("/contact_us"), "Contact Us link should work");
            Console.WriteLine("✓ Contact Us link works");

            Console.WriteLine("All header navigation links verified successfully");
        }

        // ========================================
        // TC33: Verify Footer Links
        // ========================================

        [Test]
        [Category("Navigation")]
        [Category("Smoke")]
        public async Task TC33_VerifyFooterLinks_ShouldBeVisible()
        {
            // Arrange
            Console.WriteLine("Test: Verify footer section and links");

            // Scroll to footer
            await HomePage.ScrollToFooter();
            await Wait(1000);

            // Verify Subscription section
            var subscriptionHeader = await _page.Locator("h2:has-text('Subscription')").IsVisibleAsync();
            Assert.That(subscriptionHeader, Is.True, "Subscription section should be visible in footer");
            Console.WriteLine("✓ Subscription section visible");

            // Verify subscription email input
            var emailInput = await _page.Locator("input#susbscribe_email").IsVisibleAsync();
            Assert.That(emailInput, Is.True, "Subscription email input should be visible");
            Console.WriteLine("✓ Subscription email input visible");

            // Verify footer text
            var footerText = await _page.Locator("footer").TextContentAsync();
            Assert.That(footerText, Does.Contain("Copyright").IgnoreCase,
                "Footer should contain copyright text");
            Console.WriteLine("✓ Footer copyright text present");

            // Verify footer section exists and has content
            var footerExists = await _page.Locator("footer").IsVisibleAsync();
            Assert.That(footerExists, Is.True, "Footer should be visible");

            var footerContentLength = footerText?.Length ?? 0;
            Assert.That(footerContentLength, Is.GreaterThan(100),
                "Footer should have substantial content");
            Console.WriteLine($"✓ Footer contains {footerContentLength} characters");

            Console.WriteLine("Footer verification completed successfully");
        }

        // ========================================
        // TC34: Verify Test Cases Page
        // ========================================

        [Test]
        [Category("Navigation")]
        [Category("Smoke")]
        public async Task TC34_VerifyTestCasesPage_ShouldDisplayTestCases()
        {
            // Arrange
            Console.WriteLine("Test: Navigate to Test Cases page");

            // Act - Click Test Cases link in header
            await _page.ClickAsync("a[href='/test_cases']");
            await Wait(2000);

            // Assert - Verify navigation
            string currentUrl = GetCurrentUrl();
            Assert.That(currentUrl, Does.Contain("/test_cases"),
                "Should navigate to test cases page");

            // Verify page title/heading
            var pageHeading = await _page.Locator("h2.title.text-center").First.TextContentAsync();
            Assert.That(pageHeading, Does.Contain("TEST CASES").IgnoreCase,
                "Page should have 'Test Cases' heading");

            Console.WriteLine($"Page heading: {pageHeading}");

            // Verify page content
            var pageContent = await _page.Locator("body").TextContentAsync();
            Assert.That(pageContent, Does.Contain("Test Case").IgnoreCase,
                "Page should contain test cases");

            Console.WriteLine("Test Cases page verified successfully");
        }

        // ========================================
        // TC35: Verify Scroll Up and Down Functionality
        // ========================================

        [Test]
        [Category("Navigation")]
        [Category("Smoke")]
        public async Task TC35_VerifyScrollFunctionality_ShouldScrollCorrectly()
        {
            // Arrange
            Console.WriteLine("Test: Verify scroll up and down functionality");

            // Get initial scroll position (should be near top)
            var initialScroll = await _page.EvaluateAsync<int>("window.pageYOffset");
            Console.WriteLine($"Initial scroll position: {initialScroll}px");

            // Act - Scroll down to footer
            await HomePage.ScrollToFooter();
            await Wait(1000);

            // Assert - Verify scrolled down
            var scrolledPosition = await _page.EvaluateAsync<int>("window.pageYOffset");
            Assert.That(scrolledPosition, Is.GreaterThan(initialScroll),
                "Page should scroll down");
            Console.WriteLine($"Scrolled to position: {scrolledPosition}px");

            // Verify footer subscription is visible
            var subscriptionVisible = await _page.Locator("h2:has-text('Subscription')").IsVisibleAsync();
            Assert.That(subscriptionVisible, Is.True,
                "Subscription section should be visible after scroll");
            Console.WriteLine("✓ Footer content visible after scroll down");

            // Act - Scroll back to top
            await HomePage.ScrollToTop();
            await Wait(1000);

            // Assert - Verify scrolled to top
            var finalPosition = await _page.EvaluateAsync<int>("window.pageYOffset");
            Assert.That(finalPosition, Is.LessThan(scrolledPosition),
                "Page should scroll back up");
            Console.WriteLine($"Scrolled back to position: {finalPosition}px");

            // Verify header is visible at top
            var headerVisible = await _page.Locator("header").IsVisibleAsync();
            Assert.That(headerVisible, Is.True, "Header should be visible after scroll up");
            Console.WriteLine("✓ Header visible after scroll up");

            Console.WriteLine("Scroll functionality verified successfully");
        }
    }
}