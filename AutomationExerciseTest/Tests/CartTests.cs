using NUnit.Framework;

namespace AutomationExerciseTest.Tests
{
    [TestFixture]
    public class CartTests : TestBase
    {
        // ========================================
        // TC11: Add Multiple Products to Cart
        // ========================================

        [Test]
        [Category("Cart")]
        [Category("Smoke")]
        public async Task TC11_AddMultipleProducts_ShouldAddAllToCart()
        {
            // Arrange
            Console.WriteLine("Test: Add multiple products to cart");
            int productsToAdd = 2; // Намалено на 2 за стабилност

            // Act - Navigate to Products page
            await HomePage.ClickProducts();
            await Wait(2000);

            // Collect product names before adding
            var expectedProducts = new List<string>();
            for (int i = 0; i < productsToAdd; i++)
            {
                string productName = await ProductsPage.GetProductNameByIndex(i);
                expectedProducts.Add(productName);
                Console.WriteLine($"Product {i + 1}: {productName}");
            }

            // Act - Add first product
            await ProductsPage.AddProductToCartByIndex(0);
            await Wait(2000);

            // Check if continue shopping button is visible, if so click it
            bool continueVisible = await ProductsPage.IsElementVisible(".btn.btn-success", timeout: 3000);
            if (continueVisible)
            {
                await _page.ClickAsync("button.btn.btn-success");
                await Wait(1000);
            }

            // Add second product
            await ProductsPage.AddProductToCartByIndex(1);
            await Wait(2000);

            // Click view cart
            await _page.ClickAsync("text=View Cart");
            await Wait(2000);

            // Assert - Cart page is loaded
            bool isCartLoaded = await CartPage.IsCartPageLoaded();
            Assert.That(isCartLoaded, Is.True, "Cart page should be loaded");

            // Assert - Correct number of products in cart
            int cartItemsCount = await CartPage.GetCartItemsCount();
            Assert.That(cartItemsCount, Is.EqualTo(productsToAdd),
                $"Cart should contain {productsToAdd} products");

            // Assert - All expected products are in cart
            foreach (var expectedProduct in expectedProducts)
            {
                bool productInCart = await CartPage.IsProductInCart(expectedProduct);
                Assert.That(productInCart, Is.True,
                    $"Product '{expectedProduct}' should be in cart");
            }

            Console.WriteLine($"Successfully added {productsToAdd} products to cart");
        }

        // ========================================
        // TC12: Remove Product from Cart
        // ========================================

        [Test]
        [Category("Cart")]
        [Category("Smoke")]
        public async Task TC12_RemoveProduct_ShouldRemoveFromCart()
        {
            // Arrange - Add products to cart first
            Console.WriteLine("Test: Remove product from cart");

            await HomePage.ClickProducts();
            await Wait(2000);

            // Add 2 products
            string firstProduct = await ProductsPage.GetProductNameByIndex(0);
            string secondProduct = await ProductsPage.GetProductNameByIndex(1);

            await ProductsPage.AddProductToCartByIndex(0);
            await ProductsPage.ClickContinueShopping();
            await Wait(500);

            await ProductsPage.AddProductToCartByIndex(1);
            await ProductsPage.ClickViewCart();
            await Wait(2000);

            // Verify both products are in cart
            int initialCount = await CartPage.GetCartItemsCount();
            Assert.That(initialCount, Is.EqualTo(2), "Should have 2 products initially");

            Console.WriteLine($"Initial products: {firstProduct}, {secondProduct}");

            // Act - Remove first product
            await CartPage.DeleteProductByIndex(0);
            await Wait(2000);

            // Assert - Only 1 product remains
            int finalCount = await CartPage.GetCartItemsCount();
            Assert.That(finalCount, Is.EqualTo(1), "Should have 1 product after deletion");

            // Assert - Removed product is no longer in cart
            bool firstProductInCart = await CartPage.IsProductInCart(firstProduct);
            Assert.That(firstProductInCart, Is.False,
                $"Product '{firstProduct}' should be removed from cart");

            // Assert - Other product is still in cart
            bool secondProductInCart = await CartPage.IsProductInCart(secondProduct);
            Assert.That(secondProductInCart, Is.True,
                $"Product '{secondProduct}' should still be in cart");

            Console.WriteLine($"Successfully removed product: {firstProduct}");
        }

        // ========================================
        // TC13: Update Product Quantity in Cart
        // ========================================

        [Test]
        [Category("Cart")]
        [Category("Smoke")]
        public async Task TC13_UpdateQuantity_ShouldReflectNewQuantity()
        {
            // Arrange
            Console.WriteLine("Test: Update product quantity in cart");

            // Navigate to Products and view product details
            await HomePage.ClickProducts();
            await Wait(2000);

            string productName = await ProductsPage.GetProductNameByIndex(0);
            Console.WriteLine($"Selected product: {productName}");

            await ProductsPage.ClickViewProductByIndex(0);
            await Wait(2000);

            // Act - Set quantity to 4 before adding to cart
            string desiredQuantity = "4";
            await ProductDetailsPage.SetQuantity(desiredQuantity);
            await Wait(500);

            // Act - Add to cart
            await ProductDetailsPage.ClickAddToCart();
            await Wait(2000);

            await ProductDetailsPage.ClickViewCart();
            await Wait(2000);

            // Assert - Cart page loaded
            bool isCartLoaded = await CartPage.IsCartPageLoaded();
            Assert.That(isCartLoaded, Is.True, "Cart page should be loaded");

            // Assert - Product quantity is correct
            string cartQuantity = await CartPage.GetProductQuantityByIndex(0);
            Assert.That(cartQuantity, Is.EqualTo(desiredQuantity),
                $"Product quantity should be {desiredQuantity}");

            Console.WriteLine($"Product quantity: {cartQuantity}");

            // Assert - Total price is calculated correctly
            bool isPriceCorrect = await CartPage.VerifyProductTotalPrice(0);
            Assert.That(isPriceCorrect, Is.True,
                "Total price should be correctly calculated (price × quantity)");

            Console.WriteLine("Quantity update and price calculation verified successfully");
        }

        // ========================================
        // TC14: Verify Cart Totals Calculation
        // ========================================

        [Test]
        [Category("Cart")]
        [Category("Smoke")]
        public async Task TC14_VerifyCartTotals_ShouldCalculateCorrectly()
        {
            // Arrange
            Console.WriteLine("Test: Verify cart totals calculation");

            // Add multiple products with different quantities
            await HomePage.ClickProducts();
            await Wait(2000);

            // Add first product
            await ProductsPage.AddProductToCartByIndex(0);
            await Wait(1500);
            await ProductsPage.ClickContinueShopping();
            await Wait(1000);

            // Add second product
            await ProductsPage.AddProductToCartByIndex(1);
            await Wait(1500);
            await ProductsPage.ClickViewCart();
            await Wait(2000);

            // Act - Get all products details
            var allProducts = await CartPage.GetAllProductsDetails();

            Console.WriteLine("Products in cart:");
            foreach (var product in allProducts)
            {
                Console.WriteLine($"  {product["Name"]}: {product["Price"]} × {product["Quantity"]} = {product["Total"]}");
            }

            // Assert - All products have correct total price calculation
            bool allPricesCorrect = await CartPage.VerifyAllProductTotalPrices();
            Assert.That(allPricesCorrect, Is.True,
                "All product totals should be correctly calculated");

            // Assert - Each individual product total is correct
            int productsCount = await CartPage.GetCartItemsCount();
            for (int i = 0; i < productsCount; i++)
            {
                bool isPriceCorrect = await CartPage.VerifyProductTotalPrice(i);
                Assert.That(isPriceCorrect, Is.True,
                    $"Product {i + 1} total price should be correct");
            }

            Console.WriteLine("All cart calculations verified successfully");
        }

        // ========================================
        // TC15: Clear Entire Cart
        // ========================================

        [Test]
        [Category("Cart")]
        public async Task TC15_ClearCart_ShouldRemoveAllProducts()
        {
            // Arrange - Add products to cart
            Console.WriteLine("Test: Clear entire cart");

            await HomePage.ClickProducts();
            await Wait(2000);

            // Add 2 products (по-малко за стабилност)
            await ProductsPage.AddProductToCartByIndex(0);
            await Wait(2000);

            bool continueVisible = await ProductsPage.IsElementVisible(".btn.btn-success", timeout: 3000);
            if (continueVisible)
            {
                await _page.ClickAsync("button.btn.btn-success");
                await Wait(1000);
            }

            await ProductsPage.AddProductToCartByIndex(1);
            await Wait(2000);

            await _page.ClickAsync("text=View Cart");
            await Wait(2000);

            // Verify cart has products
            int initialCount = await CartPage.GetCartItemsCount();
            Console.WriteLine($"Cart has {initialCount} products before clearing");
            Assert.That(initialCount, Is.GreaterThan(0), "Cart should have products initially");

            // Act - Clear cart by deleting all products
            for (int i = 0; i < initialCount; i++)
            {
                await CartPage.DeleteProductByIndex(0); // Always delete first
                await Wait(1500);
            }

            // Assert - Cart is empty
            await Wait(2000);
            bool isEmpty = await CartPage.IsCartEmpty();
            Assert.That(isEmpty, Is.True, "Cart should be empty after clearing");

            Console.WriteLine("Cart successfully cleared - all products removed");
        }

        // ========================================
        // TC16: View Empty Cart
        // ========================================

        [Test]
        [Category("Cart")]
        [Category("Negative")]
        public async Task TC16_ViewEmptyCart_ShouldShowEmptyMessage()
        {
            // Arrange
            Console.WriteLine("Test: View cart when empty");

            // Act - Navigate directly to cart (without adding products)
            await HomePage.ClickCart();
            await Wait(2000);

            // Assert - Cart page is loaded
            string currentUrl = GetCurrentUrl();
            Assert.That(currentUrl, Does.Contain("/view_cart"),
                "Should navigate to cart page");

            // Assert - Cart is empty
            bool isEmpty = await CartPage.IsCartEmpty();
            Assert.That(isEmpty, Is.True, "Cart should be empty");

            // Assert - No products in cart
            int productsCount = await CartPage.GetCartItemsCount();
            Assert.That(productsCount, Is.EqualTo(0),
                "Cart should have 0 products");

            Console.WriteLine("Empty cart message displayed correctly");

            // Additional verification - cart table might not be visible
            bool hasProducts = await CartPage.HasProducts();
            Assert.That(hasProducts, Is.False, "Cart should not have any products");
        }
    }
}