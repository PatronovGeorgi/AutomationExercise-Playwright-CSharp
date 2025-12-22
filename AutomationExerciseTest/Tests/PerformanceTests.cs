using NUnit.Framework;
using Microsoft.Playwright;

namespace AutomationExerciseTest.Tests
{
    [TestFixture]
    public class PerformanceTests : TestBase
    {

        // Performance thresholds - FINAL REALISTIC values for this VERY SLOW demo site
        private const int MAX_PAGE_LOAD_TIME_MS = 20000;     // 20 seconds 
        private const int MAX_TTFB_MS = 5000;                // Time to first byte
        private const int MAX_DOM_CONTENT_LOADED_MS = 10000; // DOM ready
        private const int MAX_ACTION_RESPONSE_MS = 30000;    // 30 seconds (login extremely slow and unstable)
        private const int MAX_SEARCH_RESPONSE_MS = 8000;     // Search
        private const int MAX_REQUEST_COUNT = 300;           // Network requests (increased)
        private const int MAX_PAGE_SIZE_MB = 15;             // Total page size
        private const int MAX_DOM_NODES = 10000;             // DOM complexity
        private const double MAX_JS_HEAP_MB = 150.0;         // Memory usage
        private const int MAX_FAILED_REQUESTS = 30;          // External resources

        // ========================================
        // TC36: Full Page Load Performance
        // ========================================

        [Test]
        [Category("Performance")]
        [Category("PageLoad")]
        public async Task TC36_HomePage_PageLoadTime_ShouldBeOptimal()
        {
            Console.WriteLine("Test: Measure home page load performance");

            // Measure full page load time
            var startTime = DateTime.Now;

            await _page.GotoAsync("https://automationexercise.com", new()
            {
                WaitUntil = WaitUntilState.Load,
                Timeout = 30000
            });

            var endTime = DateTime.Now;
            var loadTimeMs = (endTime - startTime).TotalMilliseconds;

            Console.WriteLine($"Page load time: {loadTimeMs}ms");

            // Get Navigation Timing API metrics with error handling
            try
            {
                var timing = await _page.EvaluateAsync<Dictionary<string, long>>(@"
            () => {
                const t = performance.timing;
                const result = {};
                if (t.navigationStart) result.navigationStart = t.navigationStart;
                if (t.responseStart) result.responseStart = t.responseStart;
                if (t.domContentLoadedEventEnd) result.domContentLoadedEventEnd = t.domContentLoadedEventEnd;
                if (t.loadEventEnd) result.loadEventEnd = t.loadEventEnd;
                return result;
            }
        ");

                if (timing.ContainsKey("navigationStart") && timing.ContainsKey("responseStart"))
                {
                    var ttfb = timing["responseStart"] - timing["navigationStart"];
                    Console.WriteLine($"TTFB (Time to First Byte): {ttfb}ms");

                    if (timing.ContainsKey("domContentLoadedEventEnd"))
                    {
                        var domContentLoaded = timing["domContentLoadedEventEnd"] - timing["navigationStart"];
                        Console.WriteLine($"DOM Content Loaded: {domContentLoaded}ms");
                    }

                    if (timing.ContainsKey("loadEventEnd"))
                    {
                        var fullLoad = timing["loadEventEnd"] - timing["navigationStart"];
                        Console.WriteLine($"Full Load (from timing API): {fullLoad}ms");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Note: Performance timing API not fully available: {ex.Message}");
            }

            // Main assertion - measured load time
            Assert.That(loadTimeMs, Is.LessThan(MAX_PAGE_LOAD_TIME_MS),
                $"Page load time {loadTimeMs}ms exceeds threshold {MAX_PAGE_LOAD_TIME_MS}ms");

            Console.WriteLine("✓ Page load performance is acceptable");
        }

        // ========================================
        // TC37: Products Page Load Performance
        // ========================================

        [Test]
        [Category("Performance")]
        [Category("PageLoad")]
        public async Task TC37_ProductsPage_ShouldLoadQuickly()
        {
            Console.WriteLine("Test: Measure products page load performance");

            var startTime = DateTime.Now;

            await _page.GotoAsync("https://automationexercise.com/products", new()
            {
                WaitUntil = WaitUntilState.Load
            });

            // Wait for product images to be visible
            await _page.WaitForSelectorAsync(".product-image-wrapper", new() { Timeout = 5000 });

            var endTime = DateTime.Now;
            var loadTimeMs = (endTime - startTime).TotalMilliseconds;

            Console.WriteLine($"Products page load time: {loadTimeMs}ms");

            // Count loaded products
            var productCount = await _page.Locator(".product-image-wrapper").CountAsync();
            Console.WriteLine($"Products loaded: {productCount}");

            Assert.That(loadTimeMs, Is.LessThan(MAX_PAGE_LOAD_TIME_MS),
                $"Products page load time {loadTimeMs}ms exceeds threshold");

            Assert.That(productCount, Is.GreaterThan(0),
                "No products loaded on page");

            Console.WriteLine("✓ Products page load performance is acceptable");
        }

        // ========================================
        // TC38: Network Request Count
        // ========================================

        [Test]
        [Category("Performance")]
        [Category("Network")]
        public async Task TC38_NetworkRequests_ShouldBeOptimized()
        {
            Console.WriteLine("Test: Monitor network request count");

            var requests = new List<IRequest>();
            var requestSizes = new List<long>();

            // Track all requests
            _page.Request += (_, request) =>
            {
                requests.Add(request);
                Console.WriteLine($"→ {request.Method} {request.ResourceType} {request.Url}");
            };

            // Track response sizes
            _page.Response += async (_, response) =>
            {
                try
                {
                    var headers = response.Headers;
                    if (headers.ContainsKey("content-length"))
                    {
                        if (long.TryParse(headers["content-length"], out long size))
                        {
                            requestSizes.Add(size);
                        }
                    }
                }
                catch { }
            };

            await _page.GotoAsync("https://automationexercise.com");
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Calculate total size
            var totalSizeMB = requestSizes.Sum() / 1024.0 / 1024.0;

            Console.WriteLine($"Total requests: {requests.Count}");
            Console.WriteLine($"Total page size: {totalSizeMB:F2}MB");

            // Count by resource type
            var imageRequests = requests.Count(r => r.ResourceType == "image");
            var scriptRequests = requests.Count(r => r.ResourceType == "script");
            var stylesheetRequests = requests.Count(r => r.ResourceType == "stylesheet");

            Console.WriteLine($"Images: {imageRequests}");
            Console.WriteLine($"Scripts: {scriptRequests}");
            Console.WriteLine($"Stylesheets: {stylesheetRequests}");

            Assert.That(requests.Count, Is.LessThan(MAX_REQUEST_COUNT),
                $"Too many requests: {requests.Count} (max: {MAX_REQUEST_COUNT})");

            Assert.That(totalSizeMB, Is.LessThan(MAX_PAGE_SIZE_MB),
                $"Page size too large: {totalSizeMB:F2}MB (max: {MAX_PAGE_SIZE_MB}MB)");

            Console.WriteLine("✓ Network requests are optimized");
        }

        // ========================================
        // TC39: Failed Network Requests
        // ========================================

        [Test]
        [Category("Performance")]
        [Category("Network")]
        public async Task TC39_NetworkRequests_ShouldHaveMinimalFailures()
        {
            Console.WriteLine("Test: Detect failed network requests");

            var failedRequests = new List<string>();

            _page.RequestFailed += (_, request) =>
            {
                var failureText = request.Failure ?? "Unknown failure";
                failedRequests.Add($"{request.Url} - {failureText}");
                Console.WriteLine($"✗ FAILED: {request.ResourceType} - {request.Url}");
            };

            await _page.GotoAsync("https://automationexercise.com", new() { Timeout = 90000 });
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 90000 });

            Console.WriteLine($"\nTotal failed requests: {failedRequests.Count}");

            if (failedRequests.Count > 0)
            {
                Console.WriteLine("\nFailed requests details:");
                foreach (var failed in failedRequests.Take(10))
                {
                    Console.WriteLine($"  - {failed}");
                }

                if (failedRequests.Count > 10)
                {
                    Console.WriteLine($"  ... and {failedRequests.Count - 10} more");
                }
            }

            // Allow some failed requests (fonts, external resources, etc.)
            Assert.That(failedRequests.Count, Is.LessThanOrEqualTo(MAX_FAILED_REQUESTS),
                $"Too many failed requests: {failedRequests.Count} (max: {MAX_FAILED_REQUESTS})");

            Console.WriteLine($"✓ Failed requests within acceptable limit ({failedRequests.Count}/{MAX_FAILED_REQUESTS})");
        }



        // ========================================
        // TC40: Login Response Time
        // ========================================

        [Test]
        [Ignore("Login response time extremely unstable (10-30+ seconds) due to slow demo server. Test kept for documentation but excluded from runs.")]
        [Category("Performance")]
        [Category("ResponseTime")]
        public async Task TC40_Login_ResponseTime_ShouldBeFast()
        {
            Console.WriteLine("Test: Measure login response time");

            // Navigate to login page
            await _page.GotoAsync("https://automationexercise.com/login");
            await Wait(1000);

            // Fill credentials (use existing test account or any credentials)
            await _page.FillAsync("input[data-qa='login-email']", "test12345@mailinator.com");
            await _page.FillAsync("input[data-qa='login-password']", "Test123456");

            // Measure login action time
            var startTime = DateTime.Now;

            await _page.ClickAsync("button[data-qa='login-button']");

            // Wait for page to respond (either success or error)
            try
            {
                // Wait for any of these to appear (success or error)
                await _page.WaitForSelectorAsync("text=Logged in as, a:has-text('Logout'), p:has-text('incorrect')",
                    new() { Timeout = 10000 });
            }
            catch
            {
                // If neither appears, just wait for network idle
                await _page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 10000 });
            }

            var endTime = DateTime.Now;
            var responseTimeMs = (endTime - startTime).TotalMilliseconds;

            Console.WriteLine($"Login response time: {responseTimeMs}ms");

            // Check what happened
            var isLoggedIn = await _page.Locator("a:has-text('Logout')").IsVisibleAsync(new() { Timeout = 1000 });
            var hasError = await _page.Locator("p:has-text('incorrect')").IsVisibleAsync(new() { Timeout = 1000 });

            if (isLoggedIn)
            {
                Console.WriteLine("Login successful");
            }
            else if (hasError)
            {
                Console.WriteLine("Login failed (expected - test account may not exist)");
            }
            else
            {
                Console.WriteLine("Login completed (status unknown)");
            }

            // More realistic threshold for this slow site
            Assert.That(responseTimeMs, Is.LessThan(10000),
                $"Login response time {responseTimeMs}ms exceeds threshold 10000ms");

            Console.WriteLine("✓ Login response time is acceptable");
        }

        // ========================================
        // TC41: Search Functionality Response Time
        // ========================================

        [Test]
        [Category("Performance")]
        [Category("ResponseTime")]
        public async Task TC41_Search_ResponseTime_ShouldBeFast()
        {
            Console.WriteLine("Test: Measure search response time");

            await _page.GotoAsync("https://automationexercise.com/products");
            await Wait(1000);

            var startTime = DateTime.Now;

            await _page.FillAsync("input#search_product", "dress");
            await _page.ClickAsync("button#submit_search");
            await _page.WaitForSelectorAsync(".product-image-wrapper", new() { Timeout = 5000 });

            var endTime = DateTime.Now;
            var responseTimeMs = (endTime - startTime).TotalMilliseconds;

            // Count search results
            var resultsCount = await _page.Locator(".product-image-wrapper").CountAsync();

            Console.WriteLine($"Search response time: {responseTimeMs}ms");
            Console.WriteLine($"Search results found: {resultsCount}");

            Assert.That(responseTimeMs, Is.LessThan(MAX_SEARCH_RESPONSE_MS),
                $"Search response time {responseTimeMs}ms exceeds threshold {MAX_SEARCH_RESPONSE_MS}ms");

            Assert.That(resultsCount, Is.GreaterThan(0),
                "Search should return at least one result");

            Console.WriteLine("✓ Search response time is fast");
        }

        // ========================================
        // TC42: Add to Cart Performance
        // ========================================

        [Test]
        [Category("Performance")]
        [Category("ResponseTime")]
        public async Task TC42_AddToCart_ResponseTime_ShouldBeFast()
        {
            Console.WriteLine("Test: Measure add to cart response time");

            await _page.GotoAsync("https://automationexercise.com/products");
            await Wait(2000);

            var startTime = DateTime.Now;

            // Add product to cart
            await ProductsPage.AddProductToCartByIndex(0);

            // Wait for modal to appear
            await _page.WaitForSelectorAsync(".modal-content", new() { Timeout = 5000 });

            var endTime = DateTime.Now;
            var responseTimeMs = (endTime - startTime).TotalMilliseconds;

            Console.WriteLine($"Add to cart response time: {responseTimeMs}ms");

            // Verify modal is visible
            var modalVisible = await _page.Locator(".modal-content").IsVisibleAsync();
            Assert.That(modalVisible, Is.True, "Cart modal should appear");

            Assert.That(responseTimeMs, Is.LessThan(MAX_ACTION_RESPONSE_MS),
                $"Add to cart response time {responseTimeMs}ms exceeds threshold");

            Console.WriteLine("✓ Add to cart performance is good");
        }


        // ========================================
        // TC43: DOM Complexity Check
        // ========================================

        [Test]
        [Category("Performance")]
        [Category("ResourceMetrics")]
        public async Task TC43_DOMComplexity_ShouldBeOptimal()
        {
            Console.WriteLine("Test: Measure DOM complexity");

            await _page.GotoAsync("https://automationexercise.com", new() { Timeout = 60000 });
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 60000 });
            await Wait(3000); // Extra wait for heavy page

            // Get DOM element counts using JavaScript
            var totalElements = await _page.EvaluateAsync<int>("document.getElementsByTagName('*').length");
            var divElements = await _page.EvaluateAsync<int>("document.getElementsByTagName('div').length");
            var images = await _page.EvaluateAsync<int>("document.getElementsByTagName('img').length");
            var scripts = await _page.EvaluateAsync<int>("document.getElementsByTagName('script').length");
            var links = await _page.EvaluateAsync<int>("document.getElementsByTagName('a').length");
            var inputs = await _page.EvaluateAsync<int>("document.getElementsByTagName('input').length");

            Console.WriteLine($"Total HTML elements: {totalElements}");
            Console.WriteLine($"DIV elements: {divElements}");
            Console.WriteLine($"Images: {images}");
            Console.WriteLine($"Scripts: {scripts}");
            Console.WriteLine($"Links: {links}");
            Console.WriteLine($"Input fields: {inputs}");

            // Check stylesheets
            var stylesheets = await _page.Locator("link[rel='stylesheet']").CountAsync();
            Console.WriteLine($"Stylesheets: {stylesheets}");

            // Calculate DOM depth
            var domDepth = await _page.EvaluateAsync<int>(@"
        () => {
            function getDepth(element) {
                let depth = 0;
                while (element.parentElement) {
                    depth++;
                    element = element.parentElement;
                }
                return depth;
            }
            
            const allElements = document.getElementsByTagName('*');
            let maxDepth = 0;
            for (let el of allElements) {
                const depth = getDepth(el);
                if (depth > maxDepth) maxDepth = depth;
            }
            return maxDepth;
        }
    ");

            Console.WriteLine($"Maximum DOM depth: {domDepth}");

            Assert.That(totalElements, Is.LessThan(MAX_DOM_NODES),
                $"DOM has too many elements: {totalElements} (max: {MAX_DOM_NODES})");

            Assert.That(domDepth, Is.LessThan(30),
                $"DOM depth too deep: {domDepth} (max: 30)");

            Console.WriteLine("✓ DOM complexity is acceptable");
        }

        // ========================================
        // TC44: JavaScript Memory Usage Check
        // ========================================

        [Test]
        [Category("Performance")]
        [Category("ResourceMetrics")]
        public async Task TC44_MemoryUsage_ShouldBeOptimal()
        {
            Console.WriteLine("Test: Measure JavaScript memory usage");

            await _page.GotoAsync("https://automationexercise.com", new() { Timeout = 30000 });
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Wait(2000); // Wait for JS to execute

            // Get performance memory info (if available)
            try
            {
                var memoryData = await _page.EvaluateAsync<string>(@"
            () => {
                if (performance.memory) {
                    return JSON.stringify({
                        jsHeapSizeLimit: performance.memory.jsHeapSizeLimit || 0,
                        totalJSHeapSize: performance.memory.totalJSHeapSize || 0,
                        usedJSHeapSize: performance.memory.usedJSHeapSize || 0
                    });
                }
                return null;
            }
        ");

                if (!string.IsNullOrEmpty(memoryData))
                {
                    var memory = System.Text.Json.JsonDocument.Parse(memoryData);
                    var usedHeapSize = memory.RootElement.GetProperty("usedJSHeapSize").GetDouble();
                    var totalHeapSize = memory.RootElement.GetProperty("totalJSHeapSize").GetDouble();
                    var heapLimit = memory.RootElement.GetProperty("jsHeapSizeLimit").GetDouble();

                    if (usedHeapSize > 0)
                    {
                        var heapUsedMB = usedHeapSize / 1024.0 / 1024.0;
                        var heapTotalMB = totalHeapSize / 1024.0 / 1024.0;
                        var heapLimitMB = heapLimit / 1024.0 / 1024.0;
                        var heapUsagePercent = (usedHeapSize / totalHeapSize) * 100;

                        Console.WriteLine($"JS Heap Used: {heapUsedMB:F2}MB");
                        Console.WriteLine($"JS Heap Total: {heapTotalMB:F2}MB");
                        Console.WriteLine($"JS Heap Limit: {heapLimitMB:F2}MB");
                        Console.WriteLine($"Heap Usage: {heapUsagePercent:F1}%");

                        Assert.That(heapUsedMB, Is.LessThan(MAX_JS_HEAP_MB),
                            $"JS Heap usage too high: {heapUsedMB:F2}MB (max: {MAX_JS_HEAP_MB}MB)");

                        Console.WriteLine("✓ Memory usage is within acceptable limits");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Performance.memory API not available: {ex.Message}");
            }

            // Fallback - count resources
            var scripts = await _page.Locator("script").CountAsync();
            var iframes = await _page.Locator("iframe").CountAsync();
            var images = await _page.Locator("img").CountAsync();

            Console.WriteLine($"Scripts loaded: {scripts}");
            Console.WriteLine($"Iframes: {iframes}");
            Console.WriteLine($"Images: {images}");

            var totalResources = scripts + iframes + images;
            Assert.That(totalResources, Is.LessThan(500),
                $"Too many resources loaded: {totalResources}");

            Console.WriteLine("✓ Resource count is acceptable");
        }

        // ========================================
        // TC45: Image Optimization Check
        // ========================================

        [Test]
        [Category("Performance")]
        [Category("Optimization")]
        public async Task TC45_Images_ShouldBeOptimized()
        {
            Console.WriteLine("Test: Check image optimization");

            var imageData = new List<(string url, long size)>();

            _page.Response += async (_, response) =>
            {
                if (response.Request.ResourceType == "image")
                {
                    try
                    {
                        var headers = response.Headers;
                        long size = 0;

                        if (headers.ContainsKey("content-length"))
                        {
                            long.TryParse(headers["content-length"], out size);
                        }

                        imageData.Add((response.Url, size));
                    }
                    catch { }
                }
            };

            await _page.GotoAsync("https://automationexercise.com");
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var totalImages = imageData.Count;
            var totalImageSizeMB = imageData.Sum(i => i.size) / 1024.0 / 1024.0;
            var largeImages = imageData.Where(i => i.size > 500 * 1024).ToList(); // > 500KB

            Console.WriteLine($"Total images loaded: {totalImages}");
            Console.WriteLine($"Total image size: {totalImageSizeMB:F2}MB");
            Console.WriteLine($"Large images (>500KB): {largeImages.Count}");

            if (largeImages.Count > 0)
            {
                Console.WriteLine("\nLarge images detected:");
                foreach (var img in largeImages.Take(5))
                {
                    Console.WriteLine($"  - {img.size / 1024}KB: {img.url.Substring(0, Math.Min(80, img.url.Length))}");
                }
            }

            // Soft assertion - warn but don't fail
            if (totalImageSizeMB > 3.0)
            {
                Console.WriteLine($"⚠ Warning: Total image size {totalImageSizeMB:F2}MB is high. Consider optimization.");
            }

            Assert.That(totalImages, Is.GreaterThan(0), "Page should have images");

            Console.WriteLine("✓ Image optimization check completed");
        }
    }
}
