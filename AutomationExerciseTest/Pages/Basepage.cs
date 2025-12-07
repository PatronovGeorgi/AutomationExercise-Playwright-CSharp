using Microsoft.Playwright;

namespace AutomationExerciseTest.Pages
{
    public class BasePage
    {
        protected IPage Page;
        protected readonly string BaseUrl = "https://automationexercise.com";

        public BasePage(IPage page)
        {
            Page = page;
        }

        // Common actions
        public async Task NavigateToUrl(string url)
        {
            await Page.GotoAsync(url);
            await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        }

        public async Task ClickElement(string selector)
        {
            await Page.ClickAsync(selector);
        }

        public async Task FillInput(string selector, string text)
        {
            await Page.FillAsync(selector, text);
        }

        public async Task<bool> IsElementVisible(string selector, int timeout = 5000)
        {
            try
            {
                await Page.WaitForSelectorAsync(selector, new() { Timeout = timeout });
                return await Page.IsVisibleAsync(selector);
            }
            catch
            {
                return false;
            }
        }

        public async Task HandleCookieConsent()
        {
            try
            {
                var acceptButton = Page.Locator("button:has-text('Einwilligen')");
                if (await acceptButton.IsVisibleAsync(new() { Timeout = 3000 }))
                {
                    await acceptButton.ClickAsync();
                    await Task.Delay(1000);
                }
            }
            catch
            {
                // Cookie dialog not present
            }
        }

        public async Task<string> GetPageTitle()
        {
            return await Page.TitleAsync();
        }

        public async Task TakeScreenshot(string filename)
        {
            await Page.ScreenshotAsync(new() { Path = $"screenshots/{filename}.png", FullPage = true });
        }
    }
}
