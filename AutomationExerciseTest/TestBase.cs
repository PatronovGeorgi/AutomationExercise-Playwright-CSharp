using Microsoft.Playwright;
using NUnit.Framework;
using AutomationExerciseTest.Pages;

namespace AutomationExerciseTest
{
    /// <summary>
    /// Base class за всички тестове - съдържа setup, teardown и инициализация на Page Objects
    /// </summary>
    public class TestBase
    {
        // ========================================
        // PLAYWRIGHT OBJECTS
        // ========================================
        protected IPlaywright _playwright;
        protected IBrowser _browser;
        protected IBrowserContext _context;
        protected IPage _page;

        // ========================================
        // PAGE OBJECTS
        // ========================================
        protected HomePage HomePage;
        protected LoginPage LoginPage;
        protected SignupPage SignupPage;
        protected ProductsPage ProductsPage;
        protected ProductDetailsPage ProductDetailsPage;
        protected CartPage CartPage;

        // ========================================
        // TEST CONFIGURATION
        // ========================================
        protected readonly string BaseUrl = "https://automationexercise.com";
        protected readonly int DefaultTimeout = 30000; // 30 seconds
        protected readonly bool Headless = false; // Set to true for CI/CD

        // ========================================
        // SETUP - Изпълнява се преди всеки тест
        // ========================================
        [SetUp]
        public async Task SetupTest()
        {
            // Initialize Playwright
            _playwright = await Playwright.CreateAsync();

            // Launch Browser
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = Headless,
                Args = new[] { "--start-maximized" },
                SlowMo = 0 // Set to 500-1000 for debugging (slows down actions)
            });

            // Create Browser Context
            _context = await _browser.NewContextAsync(new BrowserNewContextOptions
            {
                ViewportSize = ViewportSize.NoViewport,

                // Video recording (optional - uncomment if needed)
                // RecordVideoDir = "videos/",
                // RecordVideoSize = new RecordVideoSize { Width = 1920, Height = 1080 },

                // Accept all downloads
                AcceptDownloads = true,

                // Set default timeout
                // Timeout is now set via DefaultTimeout property in Playwright 1.40+
            });

            // Set default navigation timeout
            _context.SetDefaultNavigationTimeout(DefaultTimeout);
            _context.SetDefaultTimeout(DefaultTimeout);

            // Create Page
            _page = await _context.NewPageAsync();

            // Initialize Page Objects
            InitializePageObjects();

            // Navigate to home page and handle cookie consent
            await HomePage.NavigateToHomePage();

            // Log test start
            Console.WriteLine($"========================================");
            Console.WriteLine($"Test Started: {TestContext.CurrentContext.Test.Name}");
            Console.WriteLine($"========================================");
        }

        // ========================================
        // TEARDOWN - Изпълнява се след всеки тест
        // ========================================
        [TearDown]
        public async Task TeardownTest()
        {
            var testName = TestContext.CurrentContext.Test.Name;
            var testStatus = TestContext.CurrentContext.Result.Outcome.Status;

            Console.WriteLine($"========================================");
            Console.WriteLine($"Test Finished: {testName}");
            Console.WriteLine($"Status: {testStatus}");
            Console.WriteLine($"========================================");

            // Take screenshot on failure
            if (testStatus == NUnit.Framework.Interfaces.TestStatus.Failed)
            {
                await TakeScreenshotOnFailure(testName);
            }

            // Close browser and cleanup
            await CleanupBrowser();
        }

        // ========================================
        // INITIALIZATION
        // ========================================

        /// <summary>
        /// Инициализира всички Page Objects с текущия page instance
        /// </summary>
        private void InitializePageObjects()
        {
            HomePage = new HomePage(_page);
            LoginPage = new LoginPage(_page);
            SignupPage = new SignupPage(_page);
            ProductsPage = new ProductsPage(_page);
            ProductDetailsPage = new ProductDetailsPage(_page);
            CartPage = new CartPage(_page);
        }

        // ========================================
        // HELPER METHODS
        // ========================================

        /// <summary>
        /// Прави screenshot при failed тест
        /// </summary>
        private async Task TakeScreenshotOnFailure(string testName)
        {
            try
            {
                // Create screenshots directory if it doesn't exist
                var screenshotsDir = Path.Combine(Directory.GetCurrentDirectory(), "screenshots");
                Directory.CreateDirectory(screenshotsDir);

                // Generate filename with timestamp
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var filename = $"FAILED_{testName}_{timestamp}.png";
                var filepath = Path.Combine(screenshotsDir, filename);

                // Take screenshot
                await _page.ScreenshotAsync(new PageScreenshotOptions
                {
                    Path = filepath,
                    FullPage = true
                });

                Console.WriteLine($"Screenshot saved: {filepath}");

                // Attach screenshot to test report (NUnit)
                TestContext.AddTestAttachment(filepath, "Failure Screenshot");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to take screenshot: {ex.Message}");
            }
        }

        /// <summary>
        /// Затваря браузър и изчиства ресурси
        /// </summary>
        private async Task CleanupBrowser()
        {
            try
            {
                if (_page != null)
                {
                    await _page.CloseAsync();
                }

                if (_context != null)
                {
                    await _context.CloseAsync();
                }

                if (_browser != null)
                {
                    await _browser.CloseAsync();
                }

                _playwright?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during cleanup: {ex.Message}");
            }
        }

        // ========================================
        // UTILITY METHODS FOR TESTS
        // ========================================

        /// <summary>
        /// Генерира random email за тестове
        /// </summary>
        protected string GenerateRandomEmail()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return $"testuser_{timestamp}_{random}@example.com";
        }

        /// <summary>
        /// Генерира random име
        /// </summary>
        protected string GenerateRandomName()
        {
            var timestamp = DateTime.Now.ToString("HHmmss");
            return $"TestUser_{timestamp}";
        }

        /// <summary>
        /// Генерира random mobile number
        /// </summary>
        protected string GenerateRandomMobile()
        {
            var random = new Random();
            return $"9{random.Next(100000000, 999999999)}";
        }

        /// <summary>
        /// Wait helper - изчаква определено време
        /// </summary>
        protected async Task Wait(int milliseconds)
        {
            await Task.Delay(milliseconds);
        }

        /// <summary>
        /// Прави screenshot по време на теста (за debugging)
        /// </summary>
        protected async Task TakeScreenshot(string name)
        {
            var screenshotsDir = Path.Combine(Directory.GetCurrentDirectory(), "screenshots");
            Directory.CreateDirectory(screenshotsDir);

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = $"{name}_{timestamp}.png";
            var filepath = Path.Combine(screenshotsDir, filename);

            await _page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = filepath,
                FullPage = true
            });

            Console.WriteLine($"Screenshot saved: {filepath}");
        }

        /// <summary>
        /// Reload page
        /// </summary>
        protected async Task ReloadPage()
        {
            await _page.ReloadAsync();
            await Wait(1000);
        }

        /// <summary>
        /// Go back in browser history
        /// </summary>
        protected async Task GoBack()
        {
            await _page.GoBackAsync();
            await Wait(1000);
        }

        /// <summary>
        /// Get current URL
        /// </summary>
        protected string GetCurrentUrl()
        {
            return _page.Url;
        }

        /// <summary>
        /// Verify URL contains expected text
        /// </summary>
        protected bool UrlContains(string expectedText)
        {
            return GetCurrentUrl().Contains(expectedText, StringComparison.OrdinalIgnoreCase);
        }
    }
}

