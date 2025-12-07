using Microsoft.Playwright;

namespace AutomationExerciseTest.Pages
{
    public class LoginPage : BasePage
    {
        // Login Section Locators
        private readonly string _loginEmailInput = "input[data-qa='login-email']";
        private readonly string _loginPasswordInput = "input[data-qa='login-password']";
        private readonly string _loginButton = "button[data-qa='login-button']";
        private readonly string _loginSectionHeader = "text=Login to your account";

        // Signup Section Locators
        private readonly string _signupNameInput = "input[data-qa='signup-name']";
        private readonly string _signupEmailInput = "input[data-qa='signup-email']";
        private readonly string _signupButton = "button[data-qa='signup-button']";
        private readonly string _signupSectionHeader = "text=New User Signup!";

        // Error Messages
        private readonly string _loginErrorMessage = "p[style*='color: red']";
        private readonly string _emailExistsError = "text=Email Address already exist!";

        // Success Messages
        private readonly string _loggedInAsText = "text=Logged in as";
        private readonly string _accountDeletedMessage = "h2[data-qa='account-deleted']";

        // Constructor
        public LoginPage(IPage page) : base(page) { }

        // ========================================
        // LOGIN ACTIONS
        // ========================================

        public async Task EnterLoginEmail(string email)
        {
            await FillInput(_loginEmailInput, email);
        }

        public async Task EnterLoginPassword(string password)
        {
            await FillInput(_loginPasswordInput, password);
        }

        public async Task ClickLoginButton()
        {
            await ClickElement(_loginButton);
        }

        /// <summary>
        /// Извършва пълен login flow с email и password
        /// </summary>
        public async Task PerformLogin(string email, string password)
        {
            await EnterLoginEmail(email);
            await EnterLoginPassword(password);
            await ClickLoginButton();
            await Task.Delay(1000); // Wait for navigation
        }

        // ========================================
        // SIGNUP ACTIONS
        // ========================================

        public async Task EnterSignupName(string name)
        {
            await FillInput(_signupNameInput, name);
        }

        public async Task EnterSignupEmail(string email)
        {
            await FillInput(_signupEmailInput, email);
        }

        public async Task ClickSignupButton()
        {
            await ClickElement(_signupButton);
        }

        /// <summary>
        /// Извършва signup с име и email (първа стъпка на регистрацията)
        /// </summary>
        public async Task PerformSignup(string name, string email)
        {
            await EnterSignupName(name);
            await EnterSignupEmail(email);
            await ClickSignupButton();
            await Task.Delay(1000); // Wait for navigation
        }

        // ========================================
        // VERIFICATION METHODS
        // ========================================

        public async Task<bool> IsLoginFormVisible()
        {
            return await IsElementVisible(_loginEmailInput);
        }

        public async Task<bool> IsSignupFormVisible()
        {
            return await IsElementVisible(_signupNameInput);
        }

        public async Task<bool> IsLoginSectionHeaderVisible()
        {
            return await IsElementVisible(_loginSectionHeader);
        }

        public async Task<bool> IsSignupSectionHeaderVisible()
        {
            return await IsElementVisible(_signupSectionHeader);
        }

        public async Task<bool> IsLoginSuccessful()
        {
            return await IsElementVisible(_loggedInAsText);
        }

        public async Task<bool> IsLoginErrorDisplayed()
        {
            return await IsElementVisible(_loginErrorMessage);
        }

        public async Task<string> GetLoginErrorMessage()
        {
            if (await IsLoginErrorDisplayed())
            {
                var errorText = await Page.Locator(_loginErrorMessage).TextContentAsync();
                return errorText?.Trim() ?? string.Empty;
            }
            return string.Empty;
        }

        public async Task<bool> IsEmailExistsErrorDisplayed()
        {
            return await IsElementVisible(_emailExistsError);
        }

        public async Task<bool> IsAccountDeletedMessageVisible()
        {
            return await IsElementVisible(_accountDeletedMessage);
        }

        public async Task<string> GetAccountDeletedMessage()
        {
            if (await IsAccountDeletedMessageVisible())
            {
                return await Page.Locator(_accountDeletedMessage).TextContentAsync() ?? string.Empty;
            }
            return string.Empty;
        }

        // ========================================
        // UTILITY METHODS
        // ========================================

        /// <summary>
        /// Изчиства login email полето
        /// </summary>
        public async Task ClearLoginEmail()
        {
            await Page.Locator(_loginEmailInput).ClearAsync();
        }

        /// <summary>
        /// Изчиства login password полето
        /// </summary>
        public async Task ClearLoginPassword()
        {
            await Page.Locator(_loginPasswordInput).ClearAsync();
        }

        /// <summary>
        /// Проверява дали login бутонът е enabled
        /// </summary>
        public async Task<bool> IsLoginButtonEnabled()
        {
            return await Page.Locator(_loginButton).IsEnabledAsync();
        }

        /// <summary>
        /// Проверява дали signup бутонът е enabled
        /// </summary>
        public async Task<bool> IsSignupButtonEnabled()
        {
            return await Page.Locator(_signupButton).IsEnabledAsync();
        }
    }
}
