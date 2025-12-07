using Microsoft.Playwright;

namespace AutomationExerciseTest.Pages
{
    public class SignupPage : BasePage
    {
        // ========================================
        // ACCOUNT INFORMATION LOCATORS
        // ========================================
        private readonly string _titleMr = "input#id_gender1";
        private readonly string _titleMrs = "input#id_gender2";
        private readonly string _passwordInput = "input[data-qa='password']";
        private readonly string _dayDropdown = "select[data-qa='days']";
        private readonly string _monthDropdown = "select[data-qa='months']";
        private readonly string _yearDropdown = "select[data-qa='years']";
        private readonly string _newsletterCheckbox = "input#newsletter";
        private readonly string _offersCheckbox = "input#optin";
        private readonly string _accountInfoHeader = "text=Enter Account Information";

        // ========================================
        // ADDRESS INFORMATION LOCATORS
        // ========================================
        private readonly string _firstNameInput = "input[data-qa='first_name']";
        private readonly string _lastNameInput = "input[data-qa='last_name']";
        private readonly string _companyInput = "input[data-qa='company']";
        private readonly string _addressInput = "input[data-qa='address']";
        private readonly string _address2Input = "input[data-qa='address2']";
        private readonly string _countryDropdown = "select[data-qa='country']";
        private readonly string _stateInput = "input[data-qa='state']";
        private readonly string _cityInput = "input[data-qa='city']";
        private readonly string _zipcodeInput = "input[data-qa='zipcode']";
        private readonly string _mobileNumberInput = "input[data-qa='mobile_number']";

        // ========================================
        // BUTTONS AND MESSAGES
        // ========================================
        private readonly string _createAccountButton = "button[data-qa='create-account']";
        private readonly string _accountCreatedMessage = "h2[data-qa='account-created']";
        private readonly string _continueButton = "a[data-qa='continue-button']";

        // Constructor
        public SignupPage(IPage page) : base(page) { }

        // ========================================
        // ACCOUNT INFORMATION ACTIONS
        // ========================================

        /// <summary>
        /// Избира title (Mr. или Mrs.)
        /// </summary>
        /// <param name="title">Въведи "Mr" или "Mrs"</param>
        public async Task SelectTitle(string title)
        {
            if (title.ToLower() == "mr" || title.ToLower() == "male")
                await Page.CheckAsync(_titleMr);
            else if (title.ToLower() == "mrs" || title.ToLower() == "female")
                await Page.CheckAsync(_titleMrs);
        }

        public async Task EnterPassword(string password)
        {
            await FillInput(_passwordInput, password);
        }

        /// <summary>
        /// Избира дата на раждане от dropdown менютата
        /// </summary>
        /// <param name="day">Ден (1-31)</param>
        /// <param name="month">Месец (January, February, и т.н.)</param>
        /// <param name="year">Година (напр. 1990)</param>
        public async Task SelectDateOfBirth(string day, string month, string year)
        {
            await Page.SelectOptionAsync(_dayDropdown, day);
            await Page.SelectOptionAsync(_monthDropdown, month);
            await Page.SelectOptionAsync(_yearDropdown, year);
        }

        public async Task CheckNewsletter()
        {
            await Page.CheckAsync(_newsletterCheckbox);
        }

        public async Task UncheckNewsletter()
        {
            await Page.UncheckAsync(_newsletterCheckbox);
        }

        public async Task CheckSpecialOffers()
        {
            await Page.CheckAsync(_offersCheckbox);
        }

        public async Task UncheckSpecialOffers()
        {
            await Page.UncheckAsync(_offersCheckbox);
        }

        // ========================================
        // ADDRESS INFORMATION ACTIONS
        // ========================================

        public async Task EnterFirstName(string firstName)
        {
            await FillInput(_firstNameInput, firstName);
        }

        public async Task EnterLastName(string lastName)
        {
            await FillInput(_lastNameInput, lastName);
        }

        public async Task EnterCompany(string company)
        {
            await FillInput(_companyInput, company);
        }

        public async Task EnterAddress(string address)
        {
            await FillInput(_addressInput, address);
        }

        public async Task EnterAddress2(string address2)
        {
            await FillInput(_address2Input, address2);
        }

        /// <summary>
        /// Избира държава от dropdown
        /// </summary>
        /// <param name="country">Име на държавата (напр. "India", "United States")</param>
        public async Task SelectCountry(string country)
        {
            await Page.SelectOptionAsync(_countryDropdown, new SelectOptionValue { Label = country });
        }

        public async Task EnterState(string state)
        {
            await FillInput(_stateInput, state);
        }

        public async Task EnterCity(string city)
        {
            await FillInput(_cityInput, city);
        }

        public async Task EnterZipcode(string zipcode)
        {
            await FillInput(_zipcodeInput, zipcode);
        }

        public async Task EnterMobileNumber(string mobileNumber)
        {
            await FillInput(_mobileNumberInput, mobileNumber);
        }

        // ========================================
        // COMPLEX ACTIONS
        // ========================================

        /// <summary>
        /// Попълва само Address Information секцията
        /// </summary>
        public async Task FillAddressInformation(string firstName, string lastName,
            string company, string address, string address2, string country,
            string state, string city, string zipcode, string mobileNumber)
        {
            await EnterFirstName(firstName);
            await EnterLastName(lastName);
            await EnterCompany(company);
            await EnterAddress(address);
            await EnterAddress2(address2);
            await SelectCountry(country);
            await EnterState(state);
            await EnterCity(city);
            await EnterZipcode(zipcode);
            await EnterMobileNumber(mobileNumber);
        }

        /// <summary>
        /// Попълва само Account Information секцията
        /// </summary>
        public async Task FillAccountInformation(string title, string password,
            string day, string month, string year, bool newsletter = true, bool offers = true)
        {
            await SelectTitle(title);
            await EnterPassword(password);
            await SelectDateOfBirth(day, month, year);

            if (newsletter)
                await CheckNewsletter();

            if (offers)
                await CheckSpecialOffers();
        }

        /// <summary>
        /// Извършва целия registration flow - попълва цялата форма и създава account
        /// </summary>
        public async Task CompleteRegistration(
            string title, string password, string day, string month, string year,
            string firstName, string lastName, string company, string address,
            string address2, string country, string state, string city,
            string zipcode, string mobileNumber, bool newsletter = true, bool offers = true)
        {
            // Account Information
            await FillAccountInformation(title, password, day, month, year, newsletter, offers);

            // Address Information
            await FillAddressInformation(firstName, lastName, company, address,
                address2, country, state, city, zipcode, mobileNumber);

            // Submit
            await ClickCreateAccount();
        }

        /// <summary>
        /// Бърз registration с default values - използвай за тестове които не тестват конкретни полета
        /// </summary>
        public async Task QuickRegistration(string password, string firstName,
            string lastName, string mobileNumber)
        {
            await CompleteRegistration(
                title: "Mr",
                password: password,
                day: "15",
                month: "5",
                year: "1990",
                firstName: firstName,
                lastName: lastName,
                company: "Test Company",
                address: "123 Test Street",
                address2: "Apt 456",
                country: "India",
                state: "Test State",
                city: "Test City",
                zipcode: "12345",
                mobileNumber: mobileNumber,
                newsletter: true,
                offers: true
            );
        }

        // ========================================
        // BUTTON ACTIONS
        // ========================================

        public async Task ClickCreateAccount()
        {
            await ClickElement(_createAccountButton);
            await Task.Delay(2000); // Wait for account creation
        }

        public async Task ClickContinue()
        {
            await ClickElement(_continueButton);
        }

        // ========================================
        // VERIFICATION METHODS
        // ========================================

        public async Task<bool> IsAccountInfoHeaderVisible()
        {
            return await IsElementVisible(_accountInfoHeader);
        }

        public async Task<bool> IsAccountCreatedMessageVisible()
        {
            return await IsElementVisible(_accountCreatedMessage, timeout: 10000);
        }

        public async Task<string> GetAccountCreatedMessage()
        {
            if (await IsAccountCreatedMessageVisible())
            {
                return await Page.Locator(_accountCreatedMessage).TextContentAsync() ?? string.Empty;
            }
            return string.Empty;
        }

        public async Task<bool> IsCreateAccountButtonEnabled()
        {
            return await Page.Locator(_createAccountButton).IsEnabledAsync();
        }

        /// <summary>
        /// Проверява дали newsletter checkbox е checked
        /// </summary>
        public async Task<bool> IsNewsletterChecked()
        {
            return await Page.Locator(_newsletterCheckbox).IsCheckedAsync();
        }

        /// <summary>
        /// Проверява дали special offers checkbox е checked
        /// </summary>
        public async Task<bool> IsSpecialOffersChecked()
        {
            return await Page.Locator(_offersCheckbox).IsCheckedAsync();
        }

        // ========================================
        // UTILITY METHODS
        // ========================================

        /// <summary>
        /// Взима избраната държава от dropdown
        /// </summary>
        public async Task<string> GetSelectedCountry()
        {
            var selectedValue = await Page.Locator(_countryDropdown).InputValueAsync();
            return selectedValue ?? string.Empty;
        }

        /// <summary>
        /// Взима всички налични държави от dropdown
        /// </summary>
        public async Task<List<string>> GetAvailableCountries()
        {
            var options = await Page.Locator($"{_countryDropdown} option").AllTextContentsAsync();
            return options.ToList();
        }
    }
}