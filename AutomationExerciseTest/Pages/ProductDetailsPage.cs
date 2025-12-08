using Microsoft.Playwright;

namespace AutomationExerciseTest.Pages
{
    public class ProductDetailsPage : BasePage
    {
        // ========================================
        // PRODUCT INFORMATION LOCATORS
        // ========================================
        private readonly string _productName = ".product-information h2";
        private readonly string _productCategory = ".product-information p:has-text('Category:')";
        private readonly string _productPrice = ".product-information span span";
        private readonly string _productAvailability = ".product-information p:has-text('Availability:')";
        private readonly string _productCondition = ".product-information p:has-text('Condition:')";
        private readonly string _productBrand = ".product-information p:has-text('Brand:')";

        // ========================================
        // PRODUCT IMAGES
        // ========================================
        private readonly string _productMainImage = ".view-product img";
        private readonly string _productThumbnails = ".product-image-wrapper img";

        // ========================================
        // QUANTITY AND ADD TO CART
        // ========================================
        private readonly string _quantityInput = "input#quantity";
        private readonly string _addToCartButton = "button.btn.btn-default.cart";
        private readonly string _continueShoppingButton = "button.btn.btn-success";
        private readonly string _viewCartLink = "text=View Cart";

        // ========================================
        // REVIEW SECTION
        // ========================================
        private readonly string _writeReviewHeading = "a[href='#reviews']";
        private readonly string _reviewNameInput = "input#name";
        private readonly string _reviewEmailInput = "input#email";
        private readonly string _reviewTextarea = "textarea#review";
        private readonly string _submitReviewButton = "button#button-review";
        private readonly string _reviewSuccessMessage = ".alert-success.alert";

        // Constructor
        public ProductDetailsPage(IPage page) : base(page) { }

        // ========================================
        // VERIFICATION METHODS
        // ========================================

        public async Task<bool> IsProductDetailsVisible()
        {
            return await IsElementVisible(_productName);
        }

        public async Task<bool> AreAllProductDetailsVisible()
        {
            bool nameVisible = await IsElementVisible(_productName);
            bool categoryVisible = await IsElementVisible(_productCategory);
            bool priceVisible = await IsElementVisible(_productPrice);
            bool availabilityVisible = await IsElementVisible(_productAvailability);
            bool conditionVisible = await IsElementVisible(_productCondition);
            bool brandVisible = await IsElementVisible(_productBrand);

            return nameVisible && categoryVisible && priceVisible &&
                   availabilityVisible && conditionVisible && brandVisible;
        }

        // ========================================
        // GET PRODUCT INFORMATION
        // ========================================

        public async Task<string> GetProductName()
        {
            return await Page.Locator(_productName).TextContentAsync() ?? string.Empty;
        }

        public async Task<string> GetProductCategory()
        {
            var fullText = await Page.Locator(_productCategory).TextContentAsync();
            return fullText?.Trim() ?? string.Empty;
        }

        public async Task<string> GetProductPrice()
        {
            return await Page.Locator(_productPrice).TextContentAsync() ?? string.Empty;
        }

        public async Task<string> GetProductAvailability()
        {
            var fullText = await Page.Locator(_productAvailability).TextContentAsync();
            return fullText?.Replace("Availability:", "").Trim() ?? string.Empty;
        }

        public async Task<string> GetProductCondition()
        {
            var fullText = await Page.Locator(_productCondition).TextContentAsync();
            return fullText?.Replace("Condition:", "").Trim() ?? string.Empty;
        }

        public async Task<string> GetProductBrand()
        {
            var fullText = await Page.Locator(_productBrand).TextContentAsync();
            return fullText?.Replace("Brand:", "").Trim() ?? string.Empty;
        }

        public async Task<Dictionary<string, string>> GetAllProductDetails()
        {
            return new Dictionary<string, string>
            {
                { "Name", await GetProductName() },
                { "Category", await GetProductCategory() },
                { "Price", await GetProductPrice() },
                { "Availability", await GetProductAvailability() },
                { "Condition", await GetProductCondition() },
                { "Brand", await GetProductBrand() }
            };
        }

        // ========================================
        // QUANTITY AND CART ACTIONS
        // ========================================

        public async Task<string> GetCurrentQuantity()
        {
            return await Page.Locator(_quantityInput).InputValueAsync();
        }

        public async Task SetQuantity(string quantity)
        {
            await Page.Locator(_quantityInput).ClearAsync();
            await Page.FillAsync(_quantityInput, quantity);
        }

        public async Task IncreaseQuantity(int amount)
        {
            string currentQty = await GetCurrentQuantity();
            int newQty = int.Parse(currentQty) + amount;
            await SetQuantity(newQty.ToString());
        }

        public async Task ClickAddToCart()
        {
            await ClickElement(_addToCartButton);
            await Task.Delay(1000);
        }

        public async Task AddProductWithQuantity(string quantity)
        {
            await SetQuantity(quantity);
            await ClickAddToCart();
        }

        public async Task ClickContinueShopping()
        {
            if (await IsElementVisible(_continueShoppingButton, timeout: 3000))
            {
                await ClickElement(_continueShoppingButton);
            }
        }

        public async Task ClickViewCart()
        {
            await Page.ClickAsync(_viewCartLink);
            await Task.Delay(1000);
        }

        // ========================================
        // REVIEW FUNCTIONALITY
        // ========================================

        public async Task ClickWriteReview()
        {
            await Page.ClickAsync(_writeReviewHeading);
            await Task.Delay(500);
        }

        public async Task EnterReviewName(string name)
        {
            await FillInput(_reviewNameInput, name);
        }

        public async Task EnterReviewEmail(string email)
        {
            await FillInput(_reviewEmailInput, email);
        }

        public async Task EnterReviewText(string review)
        {
            await FillInput(_reviewTextarea, review);
        }

        public async Task ClickSubmitReview()
        {
            await ClickElement(_submitReviewButton);
            await Task.Delay(1000);
        }

        public async Task WriteReview(string name, string email, string review)
        {
            await ClickWriteReview();
            await EnterReviewName(name);
            await EnterReviewEmail(email);
            await EnterReviewText(review);
            await ClickSubmitReview();
        }

        public async Task<bool> IsReviewSuccessMessageVisible()
        {
            return await IsElementVisible(_reviewSuccessMessage);
        }

        public async Task<string> GetReviewSuccessMessage()
        {
            if (await IsReviewSuccessMessageVisible())
            {
                return await Page.Locator(_reviewSuccessMessage).TextContentAsync() ?? string.Empty;
            }
            return string.Empty;
        }

        // ========================================
        // IMAGE VERIFICATION
        // ========================================

        public async Task<bool> IsProductImageVisible()
        {
            return await IsElementVisible(_productMainImage);
        }

        public async Task<int> GetProductImagesCount()
        {
            return await Page.Locator(_productThumbnails).CountAsync();
        }

        public async Task<bool> HasProductImages()
        {
            try
            {
                // Check if main product image exists
                bool mainImageExists = await IsElementVisible(_productMainImage, timeout: 5000);
                if (mainImageExists) return true;

                // Fallback: check any image in product details
                int anyImageCount = await Page.Locator("img").CountAsync();
                return anyImageCount > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
