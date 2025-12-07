using Microsoft.Playwright;
using NUnit.Framework;

namespace AutomationExerciseTest
{
    [TestFixture]
    public class LoginTests
    {
        private IPlaywright _playwright;
        private IBrowser _browser;
        private IPage _page;

        [SetUp]
        public async Task Setup()
        {
            // Стартирай Playwright
            _playwright = await Playwright.CreateAsync();

            // Стартирай browser с аргументи за максимизация
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false,
                Args = new[] { "--start-maximized" }
            });

            // Създай context без viewport ограничения
            var context = await _browser.NewContextAsync(new BrowserNewContextOptions
            {
                ViewportSize = ViewportSize.NoViewport
            });

            // Създай page
            _page = await context.NewPageAsync();
        }

        [TearDown]
        public async Task Teardown()
        {
            await _browser.CloseAsync();
            _playwright.Dispose();
        }

        [Test]
        public async Task ValidLogin_NavigateToLoginPage_ShouldDisplayLoginForm()
        {
            // Navigate to homepage
            await _page.GotoAsync("https://automationexercise.com");

            await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            await Task.Delay(2000);

            // Handle cookie consent
            try
            {
                var acceptButton = _page.Locator("button:has-text('Einwilligen')");
                await acceptButton.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
                await acceptButton.ClickAsync();
                Console.WriteLine("Cookie consent accepted");
                await Task.Delay(1000);
            }
            catch
            {
                Console.WriteLine("No cookie dialog");
            }

            // Click on Signup/Login
            await _page.ClickAsync("a[href='/login']");

            // Verify login form
            var loginEmail = _page.Locator("input[data-qa='login-email']");
            await loginEmail.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });

            Console.WriteLine("Login page loaded successfully!");

            await Task.Delay(3000);
        }
    }
}

