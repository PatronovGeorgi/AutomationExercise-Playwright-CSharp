using NUnit.Framework;

namespace AutomationExerciseTest.Tests
{
    [TestFixture]
    public class ProductTests : TestBase
    {
        // ========================================
        // TC07: View All Products
        // ========================================

        [Test]
        [Category("Products")]
        [Category("Smoke")]
        public async Task TC07_ViewAllProducts_ShouldDisplayProductsList()
        {
            // Arrange
            Console.WriteLine("Test: View all products page");

            // Act - Navigate to Products page
            await HomePage.ClickProducts();
            await Wait(2000);

            // Assert - Products page is visible
            bool isProductsPageVisible = await ProductsPage.IsAllProductsPageVisible();
            Assert.That(isProductsPageVisible, Is.True, "Products page should be visible");

            // Assert - Page title is correct
            string pageTitle = await ProductsPage.GetPageTitle();
            Assert.That(pageTitle, Does.Contain("ALL PRODUCTS").IgnoreCase,
                "Page title should contain 'All Products'");
            Console.WriteLine($"Page Title: {pageTitle}");

            // Assert - Products list is visible
            int productsCount = await ProductsPage.GetProductsCount();
            Assert.That(productsCount, Is.GreaterThan(0), "Products list should not be empty");
            Console.WriteLine($"Total products displayed: {productsCount}");

            // Assert - All products have "View Product" button
            bool allHaveViewButton = await ProductsPage.DoAllProductsHaveViewButton();
            Assert.That(allHaveViewButton, Is.True, "All products should have View Product button");

            // Assert - All products have price
            bool allHavePrice = await ProductsPage.DoAllProductsHavePrice();
            Assert.That(allHavePrice, Is.True, "All products should display price");

            Console.WriteLine("All products page validations passed");
        }

        // ========================================
        // TC08: View Product Details
        // ========================================

        [Test]
        [Category("Products")]
        [Category("Smoke")]
        public async Task TC08_ViewProductDetails_ShouldDisplayAllDetails()
        {
            // Arrange
            Console.WriteLine("Test: View product details page");

            // Act - Navigate to Products page
            await HomePage.ClickProducts();
            await Wait(2000);

            // Act - Get first product name and click View Product
            string productNameFromList = await ProductsPage.GetProductNameByIndex(0);
            Console.WriteLine($"Viewing details for: {productNameFromList}");

            await ProductsPage.ClickViewProductByIndex(0);
            await Wait(2000);

            // Assert - Product details page is visible
            bool isDetailsVisible = await ProductDetailsPage.IsProductDetailsVisible();
            Assert.That(isDetailsVisible, Is.True, "Product details should be visible");

            // Assert - All product information is displayed
            bool allDetailsVisible = await ProductDetailsPage.AreAllProductDetailsVisible();
            Assert.That(allDetailsVisible, Is.True,
                "All product details (name, category, price, availability, condition, brand) should be visible");

            // Get and verify all product details
            var productDetails = await ProductDetailsPage.GetAllProductDetails();

            Console.WriteLine("Product Details:");
            Console.WriteLine($"Name: {productDetails["Name"]}");
            Console.WriteLine($"Category: {productDetails["Category"]}");
            Console.WriteLine($"Price: {productDetails["Price"]}");
            Console.WriteLine($"Availability: {productDetails["Availability"]}");
            Console.WriteLine($"Condition: {productDetails["Condition"]}");
            Console.WriteLine($"Brand: {productDetails["Brand"]}");

            // Assert - Each field has content
            Assert.That(productDetails["Name"], Is.Not.Empty, "Product name should not be empty");
            Assert.That(productDetails["Price"], Is.Not.Empty, "Product price should not be empty");
            Assert.That(productDetails["Availability"], Is.Not.Empty, "Availability should not be empty");
            Assert.That(productDetails["Condition"], Is.Not.Empty, "Condition should not be empty");
            Assert.That(productDetails["Brand"], Is.Not.Empty, "Brand should not be empty");

            // Assert - Product has images
            bool hasImages = await ProductDetailsPage.HasProductImages();
            Assert.That(hasImages, Is.True, "Product should have at least one image");

            Console.WriteLine("All product details validations passed");
        }

        // ========================================
        // TC09: Search Product
        // ========================================

        [Test]
        [Category("Products")]
        [Category("Smoke")]
        public async Task TC09_SearchProduct_WithValidName_ShouldDisplayResults()
        {
            // Arrange
            Console.WriteLine("Test: Search for product");
            string searchTerm = "Top"; // Common product name on the site

            // Act - Navigate to Products page
            await HomePage.ClickProducts();
            await Wait(2000);

            // Act - Search for product
            await ProductsPage.SearchProduct(searchTerm);
            await Wait(2000);

            // Assert - Search results are visible
            bool resultsVisible = await ProductsPage.IsSearchResultsVisible();
            Assert.That(resultsVisible, Is.True, "Search results should be visible");

            // Assert - Page title shows "SEARCHED PRODUCTS"
            string headerText = await ProductsPage.GetSearchedProductsHeaderText();
            Assert.That(headerText, Does.Contain("SEARCHED PRODUCTS").IgnoreCase,
                "Header should indicate searched products");
            Console.WriteLine($"Search Header: {headerText}");

            // Assert - Results contain searched product
            bool productFound = await ProductsPage.IsProductInSearchResults(searchTerm);
            Assert.That(productFound, Is.True,
                $"Search results should contain products with '{searchTerm}'");

            // Get all product names from search results
            var productNames = await ProductsPage.GetProductNames();
            Console.WriteLine($"Found {productNames.Count} products matching '{searchTerm}':");

            foreach (var name in productNames.Take(5)) // Show first 5
            {
                Console.WriteLine($"  - {name}");
            }

            // Assert - At least one product found
            Assert.That(productNames.Count, Is.GreaterThan(0),
                "Search should return at least one product");

            Console.WriteLine("Search product test passed");
        }

        // ========================================
        // TC10: Add Product to Cart
        // ========================================

        [Test]
        [Category("Products")]
        [Category("Cart")]
        [Category("Smoke")]
        public async Task TC10_AddProductToCart_ShouldAddSuccessfully()
        {
            // Arrange
            Console.WriteLine("Test: Add product to cart");

            // Act - Navigate to Products page
            await HomePage.ClickProducts();
            await Wait(2000);

            // Act - Get first product details
            string productName = await ProductsPage.GetProductNameByIndex(0);
            string productPrice = await ProductsPage.GetProductPriceByIndex(0);

            Console.WriteLine($"Adding to cart: {productName}");
            Console.WriteLine($"Price: {productPrice}");

            // Act - Add product to cart
            await ProductsPage.AddProductToCartByIndex(0);
            await Wait(2000);

            // Act - Click View Cart
            await ProductsPage.ClickViewCart();
            await Wait(2000);

            // Assert - Cart page is loaded
            bool isCartLoaded = await CartPage.IsCartPageLoaded();
            Assert.That(isCartLoaded, Is.True, "Cart page should be loaded");

            // Assert - Product is in cart
            bool productInCart = await CartPage.IsProductInCart(productName);
            Assert.That(productInCart, Is.True,
                $"Product '{productName}' should be in cart");

            // Assert - Cart has exactly 1 product
            int cartItemsCount = await CartPage.GetCartItemsCount();
            Assert.That(cartItemsCount, Is.EqualTo(1),
                "Cart should contain exactly 1 product");

            // Verify product details in cart
            string cartProductName = await CartPage.GetProductNameByIndex(0);
            string cartProductPrice = await CartPage.GetProductPriceByIndex(0);
            string cartQuantity = await CartPage.GetProductQuantityByIndex(0);

            Console.WriteLine($"Cart Product: {cartProductName}");
            Console.WriteLine($"Cart Price: {cartProductPrice}");
            Console.WriteLine($"Cart Quantity: {cartQuantity}");

            // Assert - Product name matches
            Assert.That(cartProductName, Does.Contain(productName).IgnoreCase,
                "Product name in cart should match");

            // Assert - Default quantity is 1
            Assert.That(cartQuantity, Is.EqualTo("1"),
                "Default quantity should be 1");

            // Assert - Total price is correct
            bool isPriceCorrect = await CartPage.VerifyProductTotalPrice(0);
            Assert.That(isPriceCorrect, Is.True,
                "Total price should be correctly calculated");

            Console.WriteLine("Add product to cart test passed");
        }
    }
}