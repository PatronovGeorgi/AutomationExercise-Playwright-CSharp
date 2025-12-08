using Microsoft.Playwright;

namespace AutomationExerciseTest.Pages
{
    public class ProductsPage : BasePage
    {
        // ========================================
        // PAGE ELEMENTS LOCATORS
        // ========================================
        private readonly string _allProductsHeader = "h2.title.text-center";
        private readonly string _productsList = ".features_items";
        private readonly string _productItem = ".product-image-wrapper";
        private readonly string _productName = ".productinfo p";
        private readonly string _productPrice = ".productinfo h2";

        // ========================================
        // SEARCH LOCATORS
        // ========================================
        private readonly string _searchInput = "input#search_product";
        private readonly string _searchButton = "button#submit_search";
        private readonly string _searchedProductsHeader = "h2.title.text-center";

        // ========================================
        // PRODUCT INTERACTION LOCATORS
        // ========================================
        private readonly string _viewProductButton = "a[href*='/product_details/']";
        private readonly string _addToCartButton = ".btn.btn-default.add-to-cart";
        private readonly string _continueShoppingButton = "button.btn.btn-success";
        private readonly string _viewCartLink = "text=View Cart";

        // Constructor
        public ProductsPage(IPage page) : base(page) { }

        // ========================================
        // NAVIGATION & PAGE VERIFICATION
        // ========================================

        public async Task<bool> IsAllProductsPageVisible()
        {
            return await IsElementVisible(_allProductsHeader);
        }

        public async Task<string> GetPageTitle()
        {
            var titleElement = Page.Locator(_allProductsHeader).First;
            return await titleElement.TextContentAsync() ?? string.Empty;
        }

        public async Task<int> GetProductsCount()
        {
            return await Page.Locator(_productItem).CountAsync();
        }

        // ========================================
        // SEARCH FUNCTIONALITY
        // ========================================

        public async Task SearchProduct(string productName)
        {
            await FillInput(_searchInput, productName);
            await ClickElement(_searchButton);
            await Task.Delay(1000);
        }

        public async Task<bool> IsSearchResultsVisible()
        {
            return await IsElementVisible(_productsList);
        }

        public async Task<string> GetSearchedProductsHeaderText()
        {
            return await Page.Locator(_searchedProductsHeader).TextContentAsync() ?? string.Empty;
        }

        public async Task<List<string>> GetProductNames()
        {
            var products = await Page.Locator(_productName).AllTextContentsAsync();
            return products.Select(p => p.Trim()).ToList();
        }

        public async Task<bool> IsProductInSearchResults(string productName)
        {
            var productNames = await GetProductNames();
            return productNames.Any(p => p.Contains(productName, StringComparison.OrdinalIgnoreCase));
        }

        // ========================================
        // PRODUCT INTERACTION
        // ========================================

        public async Task ClickViewProductByIndex(int index = 0)
        {
            await Page.Locator(_viewProductButton).Nth(index).ClickAsync();
            await Task.Delay(1000);
        }

        public async Task ClickViewProductByName(string productName)
        {
            var productWrapper = Page.Locator(_productItem)
                .Filter(new() { HasText = productName });

            await productWrapper.Locator(_viewProductButton).First.ClickAsync();
            await Task.Delay(1000);
        }

        public async Task AddProductToCartByIndex(int index = 0)
        {
            var product = Page.Locator(_productItem).Nth(index);
            await product.HoverAsync();
            await Task.Delay(500);

            // Use First to avoid multiple elements error
            await product.Locator(_addToCartButton).First.ClickAsync();
            await Task.Delay(1000);
        }

        public async Task AddProductToCartByName(string productName)
        {
            var productWrapper = Page.Locator(_productItem)
                .Filter(new() { HasText = productName });

            await productWrapper.HoverAsync();
            await Task.Delay(500);

            await productWrapper.Locator(_addToCartButton).ClickAsync();
            await Task.Delay(1000);
        }

        public async Task AddMultipleProductsToCart(int count)
        {
            for (int i = 0; i < count; i++)
            {
                await AddProductToCartByIndex(i);
                await ClickContinueShopping();
                await Task.Delay(500);
            }
        }

        // ========================================
        // MODAL ACTIONS
        // ========================================

        public async Task ClickContinueShopping()
        {
            if (await IsElementVisible(_continueShoppingButton, timeout: 3000))
            {
                await ClickElement(_continueShoppingButton);
                await Task.Delay(500);
            }
        }

        public async Task ClickViewCart()
        {
            await Page.ClickAsync(_viewCartLink);
            await Task.Delay(1000);
        }

        // ========================================
        // PRODUCT DETAILS RETRIEVAL
        // ========================================

        public async Task<string> GetProductPriceByIndex(int index)
        {
            var price = await Page.Locator(_productPrice).Nth(index).TextContentAsync();
            return price?.Trim() ?? string.Empty;
        }

        public async Task<string> GetProductNameByIndex(int index)
        {
            var name = await Page.Locator(_productName).Nth(index).TextContentAsync();
            return name?.Trim() ?? string.Empty;
        }

        public async Task<bool> DoAllProductsHavePrice()
        {
            int productsCount = await GetProductsCount();
            int pricesCount = await Page.Locator(_productPrice).CountAsync();

            return productsCount == pricesCount && productsCount > 0;
        }

        public async Task<bool> DoAllProductsHaveViewButton()
        {
            int productsCount = await GetProductsCount();
            int buttonsCount = await Page.Locator(_viewProductButton).CountAsync();

            return productsCount == buttonsCount && productsCount > 0;
        }

        // ========================================
        // UTILITY METHODS
        // ========================================

        public async Task ScrollToProduct(int index)
        {
            await Page.Locator(_productItem).Nth(index).ScrollIntoViewIfNeededAsync();
            await Task.Delay(300);
        }

        public async Task ClearSearchInput()
        {
            await Page.Locator(_searchInput).ClearAsync();
        }
    }
}