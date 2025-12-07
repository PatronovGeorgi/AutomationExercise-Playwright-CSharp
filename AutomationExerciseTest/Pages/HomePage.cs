using Microsoft.Playwright;

namespace AutomationExerciseTest.Pages
{
    public class HomePage : BasePage
    {
        // Locators - CSS селектори за елементи на страницата
        private readonly string _signupLoginLink = "a[href='/login']";
        private readonly string _productsLink = "a[href='/products']";
        private readonly string _cartLink = "a[href='/view_cart']";
        private readonly string _contactUsLink = "a[href='/contact_us']";
        private readonly string _testCasesLink = "a[href='/test_cases']";
        private readonly string _loggedInAsText = "text=Logged in as";
        private readonly string _logoutLink = "a[href='/logout']";
        private readonly string _deleteAccountLink = "a[href='/delete_account']";
        private readonly string _homeLink = "a[href='/']";

        // Constructor - извиква базовия constructor
        public HomePage(IPage page) : base(page) { }

        // Navigation Methods
        public async Task NavigateToHomePage()
        {
            await NavigateToUrl(BaseUrl);
            await HandleCookieConsent();
        }

        public async Task ClickSignupLogin()
        {
            await ClickElement(_signupLoginLink);
        }

        public async Task ClickProducts()
        {
            await ClickElement(_productsLink);
        }

        public async Task ClickCart()
        {
            await ClickElement(_cartLink);
        }

        public async Task ClickContactUs()
        {
            await ClickElement(_contactUsLink);
        }

        public async Task ClickTestCases()
        {
            await ClickElement(_testCasesLink);
        }

        public async Task ClickLogout()
        {
            await ClickElement(_logoutLink);
        }

        public async Task ClickDeleteAccount()
        {
            await ClickElement(_deleteAccountLink);
        }

        public async Task ClickHome()
        {
            await ClickElement(_homeLink);
        }

        // Verification Methods
        public async Task<bool> IsHomePageLoaded()
        {
            return await IsElementVisible(_signupLoginLink);
        }

        public async Task<bool> IsUserLoggedIn()
        {
            return await IsElementVisible(_loggedInAsText);
        }

        public async Task<string> GetLoggedInUsername()
        {
            if (await IsUserLoggedIn())
            {
                var fullText = await Page.Locator(_loggedInAsText).TextContentAsync();
                // "Logged in as TestUser" -> връща "TestUser"
                return fullText?.Replace("Logged in as ", "").Trim() ?? string.Empty;
            }
            return string.Empty;
        }

        public async Task<bool> IsLogoutVisible()
        {
            return await IsElementVisible(_logoutLink);
        }

        public async Task<bool> IsDeleteAccountVisible()
        {
            return await IsElementVisible(_deleteAccountLink);
        }

        // Utility Methods
        public async Task ScrollToFooter()
        {
            await Page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)");
            await Task.Delay(500);
        }

        public async Task ScrollToTop()
        {
            await Page.EvaluateAsync("window.scrollTo(0, 0)");
            await Task.Delay(500);
        }

        public async Task<string> GetHeaderText()
        {
            var header = Page.Locator("h2.title.text-center").First;
            return await header.TextContentAsync() ?? string.Empty;
        }
    }
}
