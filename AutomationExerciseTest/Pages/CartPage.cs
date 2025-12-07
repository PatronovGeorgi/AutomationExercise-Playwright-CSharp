using Microsoft.Playwright;

namespace AutomationExerciseTest.Pages
{
    public class CartPage : BasePage
    {
        // ========================================
        // PAGE VERIFICATION LOCATORS
        // ========================================
        private readonly string _cartInfoTable = "#cart_info_table";
        private readonly string _breadcrumb = ".breadcrumb";
        private readonly string _emptyCartMessage = "text=Cart is empty!";

        // ========================================
        // CART ITEM LOCATORS
        // ========================================
        private readonly string _cartItems = "tbody tr";
        private readonly string _productRow = "#product-";
        private readonly string _productImage = ".cart_product img";
        private readonly string _productName = ".cart_description h4 a";
        private readonly string _productPrice = ".cart_price p";
        private readonly string _productQuantity = ".cart_quantity button";
        private readonly string _productTotalPrice = ".cart_total_price";
        private readonly string _deleteButton = ".cart_quantity_delete";

        // ========================================
        // CHECKOUT LOCATORS
        // ========================================
        private readonly string _proceedToCheckoutButton = "text=Proceed To Checkout";
        private readonly string _registerLoginLink = "text=Register / Login";

        // ========================================
        // RECOMMENDED ITEMS
        // ========================================
        private readonly string _recommendedItemsSection = "#recommended-item-carousel";
        private readonly string _addToCartFromRecommended = ".recommendeditem_add_to_cart";

        // Constructor
        public CartPage(IPage page) : base(page) { }

        // ========================================
        // PAGE VERIFICATION
        // ========================================

        public async Task<bool> IsCartPageLoaded()
        {
            return await IsElementVisible(_cartInfoTable);
        }

        public async Task<bool> IsCartEmpty()
        {
            return await IsElementVisible(_emptyCartMessage, timeout: 3000);
        }

        public async Task<string> GetBreadcrumbText()
        {
            return await Page.Locator(_breadcrumb).TextContentAsync() ?? string.Empty;
        }

        // ========================================
        // CART ITEMS COUNT AND RETRIEVAL
        // ========================================

        /// <summary>
        /// Връща броя на продуктите в кошницата
        /// </summary>
        public async Task<int> GetCartItemsCount()
        {
            if (await IsCartEmpty())
                return 0;

            return await Page.Locator(_cartItems).CountAsync();
        }

        /// <summary>
        /// Проверява дали кошницата съдържа продукти
        /// </summary>
        public async Task<bool> HasProducts()
        {
            int count = await GetCartItemsCount();
            return count > 0;
        }

        // ========================================
        // PRODUCT INFORMATION RETRIEVAL
        // ========================================

        /// <summary>
        /// Връща името на продукт по index (0-based)
        /// </summary>
        public async Task<string> GetProductNameByIndex(int index)
        {
            var name = await Page.Locator(_productName).Nth(index).TextContentAsync();
            return name?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Връща цената на продукт по index
        /// </summary>
        public async Task<string> GetProductPriceByIndex(int index)
        {
            var price = await Page.Locator(_productPrice).Nth(index).TextContentAsync();
            return price?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Връща количеството на продукт по index
        /// </summary>
        public async Task<string> GetProductQuantityByIndex(int index)
        {
            var quantity = await Page.Locator(_productQuantity).Nth(index).TextContentAsync();
            return quantity?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Връща total цената на продукт (price × quantity) по index
        /// </summary>
        public async Task<string> GetProductTotalPriceByIndex(int index)
        {
            var total = await Page.Locator(_productTotalPrice).Nth(index).TextContentAsync();
            return total?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Връща всички имена на продуктите в кошницата
        /// </summary>
        public async Task<List<string>> GetAllProductNames()
        {
            var names = await Page.Locator(_productName).AllTextContentsAsync();
            return names.Select(n => n.Trim()).ToList();
        }

        /// <summary>
        /// Проверява дали конкретен продукт е в кошницата
        /// </summary>
        public async Task<bool> IsProductInCart(string productName)
        {
            var productNames = await GetAllProductNames();
            return productNames.Any(p => p.Contains(productName, StringComparison.OrdinalIgnoreCase));
        }

        // ========================================
        // PRODUCT MANIPULATION
        // ========================================

        /// <summary>
        /// Изтрива продукт от кошницата по index
        /// </summary>
        public async Task DeleteProductByIndex(int index)
        {
            await Page.Locator(_deleteButton).Nth(index).ClickAsync();
            await Task.Delay(1000); // Wait for deletion animation
        }

        /// <summary>
        /// Изтрива продукт от кошницата по име
        /// </summary>
        public async Task DeleteProductByName(string productName)
        {
            var productNames = await GetAllProductNames();
            int index = productNames.FindIndex(p =>
                p.Contains(productName, StringComparison.OrdinalIgnoreCase));

            if (index >= 0)
            {
                await DeleteProductByIndex(index);
            }
        }

        /// <summary>
        /// Изтрива всички продукти от кошницата
        /// </summary>
        public async Task ClearCart()
        {
            int count = await GetCartItemsCount();

            for (int i = 0; i < count; i++)
            {
                // Always delete first item since indices shift after deletion
                await DeleteProductByIndex(0);
                await Task.Delay(500);
            }
        }

        // ========================================
        // PRICE VERIFICATION
        // ========================================

        /// <summary>
        /// Извлича числовата стойност от price string (напр. "Rs. 500" -> 500)
        /// </summary>
        private int ExtractPrice(string priceText)
        {
            // Remove currency symbols and extract number
            var cleanPrice = priceText.Replace("Rs.", "").Replace(",", "").Trim();

            if (int.TryParse(cleanPrice, out int price))
                return price;

            return 0;
        }

        /// <summary>
        /// Проверява дали total price е правилно изчислен (price × quantity)
        /// </summary>
        public async Task<bool> VerifyProductTotalPrice(int index)
        {
            string priceText = await GetProductPriceByIndex(index);
            string quantityText = await GetProductQuantityByIndex(index);
            string totalText = await GetProductTotalPriceByIndex(index);

            int price = ExtractPrice(priceText);
            int quantity = int.Parse(quantityText);
            int expectedTotal = price * quantity;
            int actualTotal = ExtractPrice(totalText);

            return expectedTotal == actualTotal;
        }

        /// <summary>
        /// Проверява дали всички total prices са правилно изчислени
        /// </summary>
        public async Task<bool> VerifyAllProductTotalPrices()
        {
            int count = await GetCartItemsCount();

            for (int i = 0; i < count; i++)
            {
                if (!await VerifyProductTotalPrice(i))
                    return false;
            }

            return true;
        }

        // ========================================
        // CHECKOUT ACTIONS
        // ========================================

        public async Task ClickProceedToCheckout()
        {
            await ClickElement(_proceedToCheckoutButton);
            await Task.Delay(1000);
        }

        /// <summary>
        /// Проверява дали "Proceed to Checkout" бутонът е visible
        /// </summary>
        public async Task<bool> IsProceedToCheckoutVisible()
        {
            return await IsElementVisible(_proceedToCheckoutButton);
        }

        /// <summary>
        /// Проверява дали се показва "Register / Login" линк (когато user не е logged in)
        /// </summary>
        public async Task<bool> IsRegisterLoginLinkVisible()
        {
            return await IsElementVisible(_registerLoginLink, timeout: 3000);
        }

        public async Task ClickRegisterLogin()
        {
            await ClickElement(_registerLoginLink);
        }

        // ========================================
        // RECOMMENDED ITEMS
        // ========================================

        public async Task<bool> AreRecommendedItemsVisible()
        {
            return await IsElementVisible(_recommendedItemsSection);
        }

        /// <summary>
        /// Добавя recommended item в кошницата по index
        /// </summary>
        public async Task AddRecommendedItemToCart(int index = 0)
        {
            var recommendedItem = Page.Locator(_addToCartFromRecommended).Nth(index);
            await recommendedItem.ScrollIntoViewIfNeededAsync();
            await recommendedItem.ClickAsync();
            await Task.Delay(1000);
        }

        // ========================================
        // UTILITY METHODS
        // ========================================

        /// <summary>
        /// Връща детайлна информация за конкретен продукт като Dictionary
        /// </summary>
        public async Task<Dictionary<string, string>> GetProductDetails(int index)
        {
            return new Dictionary<string, string>
            {
                { "Name", await GetProductNameByIndex(index) },
                { "Price", await GetProductPriceByIndex(index) },
                { "Quantity", await GetProductQuantityByIndex(index) },
                { "Total", await GetProductTotalPriceByIndex(index) }
            };
        }

        /// <summary>
        /// Връща детайлна информация за всички продукти в кошницата
        /// </summary>
        public async Task<List<Dictionary<string, string>>> GetAllProductsDetails()
        {
            var productsList = new List<Dictionary<string, string>>();
            int count = await GetCartItemsCount();

            for (int i = 0; i < count; i++)
            {
                productsList.Add(await GetProductDetails(i));
            }

            return productsList;
        }

        /// <summary>
        /// Scroll до конкретен продукт в кошницата
        /// </summary>
        public async Task ScrollToProduct(int index)
        {
            await Page.Locator(_cartItems).Nth(index).ScrollIntoViewIfNeededAsync();
            await Task.Delay(300);
        }

        /// <summary>
        /// Проверява дали product image е visible за конкретен index
        /// </summary>
        public async Task<bool> IsProductImageVisible(int index)
        {
            try
            {
                var image = Page.Locator(_productImage).Nth(index);
                return await image.IsVisibleAsync();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Взима URL на product image
        /// </summary>
        public async Task<string> GetProductImageUrl(int index)
        {
            var src = await Page.Locator(_productImage).Nth(index).GetAttributeAsync("src");
            return src ?? string.Empty;
        }
    }
}
