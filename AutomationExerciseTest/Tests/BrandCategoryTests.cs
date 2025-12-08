using NUnit.Framework;

namespace AutomationExerciseTest.Tests
{
    [TestFixture]
    public class BrandCategoryTests : TestBase
    {
        // ========================================
        // TC22: Verify All Brands Are Visible
        // ========================================

        [Test]
        [Category("Brand")]
        [Category("Smoke")]
        public async Task TC22_ViewAllBrands_ShouldDisplayBrandsList()
        {
            // Arrange
            Console.WriteLine("Test: View all brands");

            // Act - Navigate to Products page
            await HomePage.ClickProducts();
            await Wait(2000);

            // Act - Scroll to brands section (left sidebar)
            await _page.EvaluateAsync("window.scrollTo(0, 400)");
            await Wait(1000);

            // Verify - Brands section is visible
            var brandsHeader = await _page.Locator("h2:has-text('Brands')").IsVisibleAsync();
            Assert.That(brandsHeader, Is.True, "Brands section should be visible");

            // Get all brands
            var brandLinks = await _page.Locator(".brands-name .nav.nav-pills.nav-stacked li a").AllTextContentsAsync();

            Console.WriteLine($"Total brands available: {brandLinks.Count}");
            foreach (var brand in brandLinks)
            {
                Console.WriteLine($"  - {brand.Trim()}");
            }

            // Assert - At least one brand exists
            Assert.That(brandLinks.Count, Is.GreaterThan(0),
                "Should have at least one brand");

            Console.WriteLine("All brands displayed successfully");
        }

        // ========================================
        // TC23: Filter Products by Brand
        // ========================================

        [Test]
        [Category("Brand")]
        [Category("Smoke")]
        public async Task TC23_FilterByBrand_ShouldShowBrandProducts()
        {
            // Arrange
            Console.WriteLine("Test: Filter products by brand");

            // Navigate to Products page
            await HomePage.ClickProducts();
            await Wait(2000);

            // Scroll to brands
            await _page.EvaluateAsync("window.scrollTo(0, 400)");
            await Wait(1000);

            // Act - Click on first brand
            var firstBrand = _page.Locator(".brands-name .nav.nav-pills.nav-stacked li a").First;
            string brandName = await firstBrand.TextContentAsync() ?? "Unknown";
            brandName = brandName.Trim();

            Console.WriteLine($"Clicking on brand: {brandName}");
            await firstBrand.ClickAsync();
            await Wait(2000);

            // Assert - Brand products page is displayed
            var pageTitle = await _page.Locator("h2.title.text-center").TextContentAsync();
            Assert.That(pageTitle, Does.Contain("BRAND").IgnoreCase,
                "Should display brand products page");

            Console.WriteLine($"Page title: {pageTitle}");

            // Assert - Products are displayed
            int productsCount = await _page.Locator(".product-image-wrapper").CountAsync();
            Assert.That(productsCount, Is.GreaterThan(0),
                "Should display brand products");

            Console.WriteLine($"Found {productsCount} products for brand: {brandName}");
        }

        // ========================================
        // TC24: View Category Products
        // ========================================

        [Test]
        [Category("Category")]
        [Category("Smoke")]
        public async Task TC24_ViewCategoryProducts_ShouldDisplayCategory()
        {
            // Arrange
            Console.WriteLine("Test: View products by category");

            // Already on home page

            // Verify - Category section is visible in left sidebar
            var categoryHeader = await _page.Locator("h2:has-text('Category')").IsVisibleAsync();
            Assert.That(categoryHeader, Is.True, "Category section should be visible");

            // Act - Expand Women category
            await _page.ClickAsync("a[href='#Women']");
            await Wait(1000);

            // Act - Click on a subcategory (e.g., Dress)
            await _page.ClickAsync("text=Dress");
            await Wait(2000);

            // Assert - Category page is displayed
            var pageTitle = await _page.Locator("h2.title.text-center").TextContentAsync();
            Assert.That(pageTitle, Does.Contain("WOMEN").IgnoreCase,
                "Should display Women category page");
            Assert.That(pageTitle, Does.Contain("DRESS").IgnoreCase,
                "Should display Dress subcategory");

            Console.WriteLine($"Page title: {pageTitle}");

            // Assert - Products are displayed
            int productsCount = await _page.Locator(".product-image-wrapper").CountAsync();
            Assert.That(productsCount, Is.GreaterThan(0),
                "Should display category products");

            Console.WriteLine($"Found {productsCount} products in Women > Dress category");
        }

        // ========================================
        // TC25: Navigate Between Categories
        // ========================================

        [Test]
        [Category("Category")]
        [Category("Smoke")]
        public async Task TC25_NavigateBetweenCategories_ShouldUpdateProducts()
        {
            // Arrange
            Console.WriteLine("Test: Navigate between different categories");

            // Act - Navigate to first category (Women > Dress)
            await _page.ClickAsync("a[href='#Women']");
            await Wait(500);
            await _page.ClickAsync("text=Dress");
            await Wait(2000);

            // Capture first category title
            var firstCategoryTitle = await _page.Locator("h2.title.text-center").TextContentAsync();
            Console.WriteLine($"First category: {firstCategoryTitle}");

            // Act - Navigate to second category (Men > Tshirts)
            await _page.ClickAsync("a[href='#Men']");
            await Wait(500);
            await _page.ClickAsync("text=Tshirts");
            await Wait(2000);

            // Assert - Second category page is displayed
            var secondCategoryTitle = await _page.Locator("h2.title.text-center").TextContentAsync();
            Console.WriteLine($"Second category: {secondCategoryTitle}");

            Assert.That(secondCategoryTitle, Does.Contain("MEN").IgnoreCase,
                "Should display Men category");
            Assert.That(secondCategoryTitle, Does.Contain("TSHIRTS").IgnoreCase,
                "Should display Tshirts subcategory");

            // Assert - Title changed (different from first category)
            Assert.That(secondCategoryTitle, Is.Not.EqualTo(firstCategoryTitle),
                "Category title should change when navigating");

            // Assert - Products are displayed
            int productsCount = await _page.Locator(".product-image-wrapper").CountAsync();
            Assert.That(productsCount, Is.GreaterThan(0),
                "Should display products in new category");

            Console.WriteLine($"Successfully navigated between categories");
        }
    }
}