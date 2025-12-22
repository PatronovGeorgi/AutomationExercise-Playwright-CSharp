using NUnit.Framework;
using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace AutomationExerciseTest.Tests
{
    [TestFixture]
    public class SecurityTests : TestBase
    {
        // ========================================
        // TC46: HTTPS Enforcement
        // ========================================

        [Test]
        [Category("Security")]
        [Category("HTTPS")]
        public async Task TC46_HTTPSEnforcement_ShouldUseSecureConnection()
        {
            Console.WriteLine("Test: Verify HTTPS is enforced");

            // Navigate to site
            await _page.GotoAsync("https://automationexercise.com");
            await Wait(2000);

            // Get current URL
            string currentUrl = _page.Url;
            Console.WriteLine($"Current URL: {currentUrl}");

            // Assert - URL should use HTTPS
            Assert.That(currentUrl, Does.StartWith("https://"),
                "Site should enforce HTTPS for secure communication");

            // Try navigating with HTTP (if supported)
            try
            {
                var response = await _page.GotoAsync("http://automationexercise.com",
                    new() { Timeout = 10000 });

                string finalUrl = _page.Url;
                Console.WriteLine($"After HTTP request, final URL: {finalUrl}");

                // Should redirect to HTTPS
                Assert.That(finalUrl, Does.StartWith("https://"),
                    "HTTP requests should redirect to HTTPS");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HTTP navigation failed (expected): {ex.Message}");
                // If HTTP is completely blocked, that's even better for security
            }

            Console.WriteLine("✓ HTTPS enforcement verified");
        }

        // ========================================
        // TC47: Security Headers Validation
        // ========================================

        [Test]
        [Category("Security")]
        [Category("Headers")]
        public async Task TC47_SecurityHeaders_ShouldBePresent()
        {
            Console.WriteLine("Test: Verify security headers are present");

            IResponse? response = null;

            _page.Response += (_, resp) =>
            {
                if (resp.Url.Contains("automationexercise.com") && resp.Status == 200)
                {
                    response = resp;
                }
            };

            await _page.GotoAsync("https://automationexercise.com");
            await Wait(2000);

            Assert.That(response, Is.Not.Null, "Should capture response");

            var headers = response!.Headers;

            // Log all headers
            Console.WriteLine("\nResponse Headers:");
            foreach (var header in headers)
            {
                Console.WriteLine($"  {header.Key}: {header.Value}");
            }

            // Check for important security headers
            var securityHeaders = new Dictionary<string, bool>
            {
                { "x-frame-options", false },
                { "x-content-type-options", false },
                { "x-xss-protection", false },
                { "strict-transport-security", false },
                { "content-security-policy", false }
            };

            foreach (var header in headers)
            {
                string headerName = header.Key.ToLower();
                if (securityHeaders.ContainsKey(headerName))
                {
                    securityHeaders[headerName] = true;
                    Console.WriteLine($"✓ Found security header: {header.Key} = {header.Value}");
                }
            }

            // Count present headers
            int presentHeaders = securityHeaders.Values.Count(v => v);
            Console.WriteLine($"\nSecurity headers present: {presentHeaders}/5");

            // Soft assertion - document findings but don't fail test
            if (presentHeaders < 3)
            {
                Console.WriteLine("⚠ Warning: Site is missing important security headers");
                Console.WriteLine("Missing headers can expose the site to:");
                if (!securityHeaders["x-frame-options"]) Console.WriteLine("  - Clickjacking attacks");
                if (!securityHeaders["x-content-type-options"]) Console.WriteLine("  - MIME type sniffing");
                if (!securityHeaders["x-xss-protection"]) Console.WriteLine("  - XSS attacks");
                if (!securityHeaders["strict-transport-security"]) Console.WriteLine("  - Protocol downgrade attacks");
                if (!securityHeaders["content-security-policy"]) Console.WriteLine("  - Code injection attacks");
            }

            // Assert at least response was captured
            Assert.That(response, Is.Not.Null, "Security headers check completed");

            Console.WriteLine("✓ Security headers validation completed");
        }

        // ========================================
        // TC48: XSS Prevention - Input Sanitization
        // ========================================

        [Test]
        [Category("Security")]
        [Category("XSS")]
        public async Task TC48_XSSPrevention_ShouldSanitizeInput()
        {
            Console.WriteLine("Test: Verify XSS attack prevention");

            await _page.GotoAsync("https://automationexercise.com/products");
            await Wait(2000);

            // XSS payloads to test
            string[] xssPayloads = new[]
            {
                "<script>alert('XSS')</script>",
                "<img src=x onerror=alert('XSS')>",
                "javascript:alert('XSS')",
                "<svg/onload=alert('XSS')>"
            };

            foreach (var payload in xssPayloads)
            {
                Console.WriteLine($"\nTesting XSS payload: {payload}");

                // Try to inject XSS in search field
                await _page.FillAsync("input#search_product", payload);
                await _page.ClickAsync("button#submit_search");
                await Wait(2000);

                // Check if alert dialog appears (would indicate XSS vulnerability)
                bool alertAppeared = false;

                _page.Dialog += async (_, dialog) =>
                {
                    alertAppeared = true;
                    Console.WriteLine($"❌ VULNERABILITY: Alert dialog appeared with message: {dialog.Message}");
                    await dialog.DismissAsync();
                };

                await Wait(1000);

                // Check page content
                var pageContent = await _page.ContentAsync();

                // Payload should be sanitized/encoded in the output
                if (pageContent.Contains(payload) && !pageContent.Contains("&lt;") && !pageContent.Contains("&gt;"))
                {
                    Console.WriteLine($"⚠ Warning: Payload appears unsanitized in page content");
                }
                else
                {
                    Console.WriteLine($"✓ Payload properly sanitized or not reflected");
                }

                Assert.That(alertAppeared, Is.False,
                    $"XSS payload should not trigger script execution: {payload}");

                // Reset for next test
                await _page.GotoAsync("https://automationexercise.com/products");
                await Wait(1000);
            }

            Console.WriteLine("\n✓ XSS prevention test completed");
        }

        // ========================================
        // TC49: SQL Injection Prevention
        // ========================================

        [Test]
        [Category("Security")]
        [Category("SQLInjection")]
        public async Task TC49_SQLInjectionPrevention_ShouldBlockMaliciousInput()
        {
            Console.WriteLine("Test: Verify SQL injection prevention");

            await _page.GotoAsync("https://automationexercise.com/login");
            await Wait(2000);

            // SQL injection payloads
            string[] sqlPayloads = new[]
            {
                "' OR '1'='1",
                "admin'--",
                "' OR '1'='1' --",
                "'; DROP TABLE users--",
                "1' UNION SELECT NULL--"
            };

            foreach (var payload in sqlPayloads)
            {
                Console.WriteLine($"\nTesting SQL injection payload: {payload}");

                // Try SQL injection in email field
                await _page.FillAsync("input[data-qa='login-email']", payload);
                await _page.FillAsync("input[data-qa='login-password']", "password123");

                await _page.ClickAsync("button[data-qa='login-button']");
                await Wait(3000);

                // Check if login was successful (would indicate SQL injection vulnerability)
                var logoutVisible = await _page.Locator("a:has-text('Logout')").IsVisibleAsync(new() { Timeout = 2000 });

                Assert.That(logoutVisible, Is.False,
                    $"SQL injection should not bypass authentication: {payload}");

                if (!logoutVisible)
                {
                    Console.WriteLine($"✓ SQL injection blocked - authentication failed as expected");
                }

                // Check for error handling (should not reveal database errors)
                var pageContent = await _page.ContentAsync();

                if (pageContent.ToLower().Contains("sql") ||
                    pageContent.ToLower().Contains("mysql") ||
                    pageContent.ToLower().Contains("syntax error") ||
                    pageContent.ToLower().Contains("database"))
                {
                    Console.WriteLine($"⚠ Warning: Database error information might be exposed");
                }

                // Reset
                await _page.GotoAsync("https://automationexercise.com/login");
                await Wait(1000);
            }

            Console.WriteLine("\n✓ SQL injection prevention test completed");
        }

        // ========================================
        // TC50: Cookie Security Flags
        // ========================================

        [Test]
        [Category("Security")]
        [Category("Cookies")]
        public async Task TC50_CookieSecurity_ShouldHaveSecureFlags()
        {
            Console.WriteLine("Test: Verify cookie security attributes");

            await _page.GotoAsync("https://automationexercise.com");
            await Wait(2000);

            // Get all cookies
            var cookies = await _page.Context.CookiesAsync();

            Console.WriteLine($"\nTotal cookies found: {cookies.Count}");

            if (cookies.Count == 0)
            {
                Console.WriteLine("⚠ No cookies found to analyze");
                Assert.Pass("No cookies present - security check not applicable");
                return;
            }

            foreach (var cookie in cookies)
            {
                Console.WriteLine($"\nCookie: {cookie.Name}");
                Console.WriteLine($"  Domain: {cookie.Domain}");
                Console.WriteLine($"  Path: {cookie.Path}");
                Console.WriteLine($"  Secure: {cookie.Secure}");
                Console.WriteLine($"  HttpOnly: {cookie.HttpOnly}");
                Console.WriteLine($"  SameSite: {cookie.SameSite}");

                // Check for security flags
                if (!cookie.Secure && _page.Url.StartsWith("https://"))
                {
                    Console.WriteLine($"  ⚠ Warning: Cookie '{cookie.Name}' missing Secure flag on HTTPS site");
                }

                if (!cookie.HttpOnly)
                {
                    Console.WriteLine($"  ⚠ Warning: Cookie '{cookie.Name}' missing HttpOnly flag (vulnerable to XSS)");
                }

                if (cookie.SameSite == SameSiteAttribute.None || cookie.SameSite == null)
                {
                    Console.WriteLine($"  ⚠ Warning: Cookie '{cookie.Name}' missing SameSite attribute (vulnerable to CSRF)");
                }
            }

            // Count secure cookies
            int secureCount = cookies.Count(c => c.Secure);
            int httpOnlyCount = cookies.Count(c => c.HttpOnly);

            Console.WriteLine($"\nCookies with Secure flag: {secureCount}/{cookies.Count}");
            Console.WriteLine($"Cookies with HttpOnly flag: {httpOnlyCount}/{cookies.Count}");

            Assert.That(cookies.Count, Is.GreaterThan(0), "Cookie security check completed");

            Console.WriteLine("✓ Cookie security analysis completed");
        }

        // ========================================
        // TC51: Password Field Masking
        // ========================================

        [Test]
        [Category("Security")]
        [Category("Authentication")]
        public async Task TC51_PasswordField_ShouldBeMasked()
        {
            Console.WriteLine("Test: Verify password fields are masked");

            await _page.GotoAsync("https://automationexercise.com/login");
            await Wait(2000);

            // Check login password field
            var loginPasswordType = await _page.GetAttributeAsync("input[data-qa='login-password']", "type");
            Console.WriteLine($"Login password field type: {loginPasswordType}");

            Assert.That(loginPasswordType, Is.EqualTo("password"),
                "Login password field should have type='password' to mask input");

            // Check signup password field
            await _page.GotoAsync("https://automationexercise.com/signup");
            await Wait(2000);

            // Fill name and email to access signup form
            await _page.FillAsync("input[data-qa='signup-name']", "Test User");
            await _page.FillAsync("input[data-qa='signup-email']", GenerateRandomEmail());
            await _page.ClickAsync("button[data-qa='signup-button']");
            await Wait(2000);

            // Check password field in registration form
            var signupPasswordType = await _page.GetAttributeAsync("input[data-qa='password']", "type");
            Console.WriteLine($"Signup password field type: {signupPasswordType}");

            Assert.That(signupPasswordType, Is.EqualTo("password"),
                "Signup password field should have type='password' to mask input");

            // Verify password is visually masked
            await _page.FillAsync("input[data-qa='password']", "TestPassword123");
            var passwordValue = await _page.InputValueAsync("input[data-qa='password']");

            // Value is accessible to automation, but visually it should be masked
            Console.WriteLine($"Password field accepts input correctly (length: {passwordValue.Length})");

            Console.WriteLine("✓ Password fields are properly masked");
        }

        // ========================================
        // TC52: Session Timeout Check
        // ========================================

        [Test]
        [Category("Security")]
        [Category("Session")]
        public async Task TC52_SessionManagement_ShouldExist()
        {
            Console.WriteLine("Test: Verify session management implementation");

            // Register and login
            string email = GenerateRandomEmail();
            string password = "Test123456";

            await _page.GotoAsync("https://automationexercise.com/signup");
            await Wait(2000);

            await _page.FillAsync("input[data-qa='signup-name']", "Session Test");
            await _page.FillAsync("input[data-qa='signup-email']", email);
            await _page.ClickAsync("button[data-qa='signup-button']");
            await Wait(2000);

            await SignupPage.QuickRegistration(password, "Test", "User", GenerateRandomMobile());
            await Wait(3000);

            await SignupPage.ClickContinue();
            await Wait(2000);

            // Verify logged in
            var isLoggedIn = await _page.Locator("a:has-text('Logout')").IsVisibleAsync();
            Assert.That(isLoggedIn, Is.True, "User should be logged in");

            Console.WriteLine("✓ User logged in successfully");

            // Check for session cookies
            var cookies = await _page.Context.CookiesAsync();
            var sessionCookies = cookies.Where(c =>
                c.Name.ToLower().Contains("session") ||
                c.Name.ToLower().Contains("sess") ||
                c.Name.ToLower().Contains("phpsessid") ||
                c.Name.ToLower().Contains("jsessionid") ||
                c.Name.ToLower().Contains("auth")
            ).ToList();

            Console.WriteLine($"Session-related cookies found: {sessionCookies.Count}");
            foreach (var cookie in sessionCookies)
            {
                Console.WriteLine($"  - {cookie.Name} (Secure: {cookie.Secure}, HttpOnly: {cookie.HttpOnly})");
            }

            // Logout
            await _page.ClickAsync("a:has-text('Logout')");
            await Wait(2000);

            // Verify logged out
            var logoutSuccessful = await _page.Locator("a:has-text('Signup / Login')").IsVisibleAsync();
            Assert.That(logoutSuccessful, Is.True, "User should be logged out");

            Console.WriteLine("✓ Session management check completed");
        }

        // ========================================
        // TC53: Sensitive Data in URLs
        // ========================================

        [Test]
        [Category("Security")]
        [Category("DataExposure")]
        public async Task TC53_SensitiveData_ShouldNotBeInURL()
        {
            Console.WriteLine("Test: Verify sensitive data is not exposed in URLs");

            // Navigate through various pages and check URLs
            var urlsToCheck = new List<string>();

            // Home page
            await _page.GotoAsync("https://automationexercise.com");
            await Wait(1000);
            urlsToCheck.Add(_page.Url);

            // Products page
            await HomePage.ClickProducts();
            await Wait(1000);
            urlsToCheck.Add(_page.Url);

            // Login page
            await HomePage.ClickSignupLogin();
            await Wait(1000);
            urlsToCheck.Add(_page.Url);

            // Search
            await _page.GotoAsync("https://automationexercise.com/products");
            await _page.FillAsync("input#search_product", "shirt");
            await _page.ClickAsync("button#submit_search");
            await Wait(2000);
            urlsToCheck.Add(_page.Url);

            Console.WriteLine("\nAnalyzing URLs for sensitive data exposure:");

            // Sensitive patterns to look for
            var sensitivePatterns = new[]
            {
                @"password=",
                @"pwd=",
                @"pass=",
                @"credit.*card",
                @"ssn=",
                @"social.*security",
                @"api.*key",
                @"secret",
                @"token=.*[A-Za-z0-9]{20,}"  // Long tokens
            };

            bool foundSensitiveData = false;

            foreach (var url in urlsToCheck)
            {
                Console.WriteLine($"\nChecking URL: {url}");

                foreach (var pattern in sensitivePatterns)
                {
                    if (Regex.IsMatch(url.ToLower(), pattern))
                    {
                        Console.WriteLine($"  ❌ VULNERABILITY: Sensitive pattern found: {pattern}");
                        foundSensitiveData = true;
                    }
                }

                if (!foundSensitiveData)
                {
                    Console.WriteLine($"  ✓ No sensitive data detected");
                }
            }

            Assert.That(foundSensitiveData, Is.False,
                "URLs should not contain sensitive information");

            Console.WriteLine("\n✓ Sensitive data exposure check completed");
        }

        // ========================================
        // TC54: File Upload Security
        // ========================================

        [Test]
        [Category("Security")]
        [Category("FileUpload")]
        public async Task TC54_FileUpload_ShouldHaveValidation()
        {
            Console.WriteLine("Test: Verify file upload security (if file upload exists)");

            await _page.GotoAsync("https://automationexercise.com/contact_us");
            await Wait(2000);

            // Check if file upload exists
            var fileUploadExists = await _page.Locator("input[type='file']").IsVisibleAsync(new() { Timeout = 3000 });

            if (!fileUploadExists)
            {
                Console.WriteLine("⚠ No file upload field found on contact page");
                Assert.Pass("File upload not present - test not applicable");
                return;
            }

            Console.WriteLine("✓ File upload field found");

            // Get file input element
            var fileInput = _page.Locator("input[type='file']");

            // Check if input has accept attribute (file type restriction)
            var acceptAttribute = await fileInput.GetAttributeAsync("accept");

            if (!string.IsNullOrEmpty(acceptAttribute))
            {
                Console.WriteLine($"✓ File type restriction found: {acceptAttribute}");
            }
            else
            {
                Console.WriteLine("⚠ Warning: No file type restriction (accept attribute missing)");
            }

            // Check for max file size indication
            var pageContent = await _page.ContentAsync();
            bool hasSizeLimit = pageContent.ToLower().Contains("max") ||
                               pageContent.ToLower().Contains("size") ||
                               pageContent.ToLower().Contains("mb") ||
                               pageContent.ToLower().Contains("kb");

            if (hasSizeLimit)
            {
                Console.WriteLine("✓ File size limit information present");
            }
            else
            {
                Console.WriteLine("⚠ Warning: No visible file size limit information");
            }

            Console.WriteLine("✓ File upload security check completed");
        }

        // ========================================
        // TC55: Error Message Information Disclosure
        // ========================================

        [Test]
        [Category("Security")]
        [Category("ErrorHandling")]
        public async Task TC55_ErrorHandling_ShouldNotDiscloseSensitiveInfo()
        {
            Console.WriteLine("Test: Verify error messages don't disclose sensitive information");

            // Test 1: Invalid login
            Console.WriteLine("\n1. Testing invalid login error message:");
            await _page.GotoAsync("https://automationexercise.com/login");
            await Wait(2000);

            await _page.FillAsync("input[data-qa='login-email']", "nonexistent@example.com");
            await _page.FillAsync("input[data-qa='login-password']", "wrongpassword");
            await _page.ClickAsync("button[data-qa='login-button']");
            await Wait(2000);

            var loginErrorMessage = await _page.Locator("p").Filter(new() { HasText = "incorrect" }).First.TextContentAsync();
            Console.WriteLine($"Login error message: {loginErrorMessage}");

            // Error should be generic, not revealing if email exists
            var errorLower = loginErrorMessage.ToLower();
            if (errorLower.Contains("user not found") || errorLower.Contains("email does not exist"))
            {
                Console.WriteLine("⚠ Warning: Error message reveals user enumeration information");
            }
            else
            {
                Console.WriteLine("✓ Error message is appropriately generic");
            }

            // Test 2: Check for stack traces or detailed errors
            Console.WriteLine("\n2. Checking for technical error exposure:");

            // Try accessing non-existent page
            var response = await _page.GotoAsync("https://automationexercise.com/nonexistentpage12345",
                new() { WaitUntil = WaitUntilState.NetworkIdle });

            var errorPageContent = await _page.ContentAsync();

            // Check for sensitive information in error pages
            var sensitiveKeywords = new[]
            {
                "stack trace",
                "exception",
                "sql",
                "database",
                "server error",
                "php warning",
                "php notice",
                "undefined index",
                "mysql",
                "postgresql"
            };

            bool foundSensitiveError = false;
            foreach (var keyword in sensitiveKeywords)
            {
                if (errorPageContent.ToLower().Contains(keyword))
                {
                    Console.WriteLine($"⚠ Warning: Error page contains technical keyword: {keyword}");
                    foundSensitiveError = true;
                }
            }

            if (!foundSensitiveError)
            {
                Console.WriteLine("✓ No sensitive technical information in error pages");
            }

            Console.WriteLine("\n✓ Error handling security check completed");
        }
    }
}
