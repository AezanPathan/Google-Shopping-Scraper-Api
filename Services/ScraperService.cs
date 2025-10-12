using Microsoft.Playwright;
using GoogleShoppingScraperApi.Models;

namespace GoogleShoppingScraperApi.Services;

public class ScraperService
{
    private const string ProductCardSelector = "[data-cid]";
    private const string TitleSelector = "div.gkQHve";
    private const string PriceSelector = "span[aria-label^='Current price']";
    private const string MerchantSelector = "span.WJMUdc";
    private const string ImageSelector = "div.R1iPve img";
    private const string LinkSelector = "a";
    private const string RatingSelector = "div.LFROUd span.yi40Hd";
    private const string ReviewsSelector = "div.LFROUd span.RDApEe";
    private const int ScrollDelayMs = 2000;

    public async Task<string> ScrapeGoogleShoppingTitleAsync()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });

        var page = await browser.NewPageAsync();
        await page.GotoAsync("https://www.google.com/shopping");

        return await page.TitleAsync();
    }

    /// <summary>
    /// Scrapes Google Shopping search results for the given product name.
    /// Returns a list of <see cref="Product"/> instances found on the page.
    /// </summary>
    /// <param name="productName">Search query to use on Google Shopping.</param>
    /// <param name="maxScroll">Maximum number of scroll iterations to attempt loading more items.</param>
    /// <param name="totalProducts">Maximum number of products to return.</param>
    public async Task<List<Product>> ScrapeProductsAsync(string productName, int maxScroll = 10, int totalProducts = 100)
    {
        if (string.IsNullOrWhiteSpace(productName))
        {
            throw new ArgumentException("Product name must be provided.", nameof(productName));
        }

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true // set to true in production
        });

        var page = await browser.NewPageAsync();
        var query = System.Web.HttpUtility.UrlEncode(productName);
        await page.GotoAsync($"https://www.google.com/search?tbm=shop&q={query}", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });

        await page.Locator(ProductCardSelector).First.WaitForAsync();

        int prevCount = 0;
        for (int scrolls = 0; scrolls < maxScroll; scrolls++) // adjust max scrolls
        {
            await page.EvaluateAsync("window.scrollBy(0, document.body.scrollHeight)");
            await page.WaitForTimeoutAsync(ScrollDelayMs); // wait for new items to load

            int currentCount = await page.Locator(ProductCardSelector).CountAsync();
            if (currentCount == prevCount)
                break; 

            prevCount = currentCount;
        }

        var products = new List<Product>();
        var productCards = page.Locator(ProductCardSelector);
        int count = await productCards.CountAsync();

        for (int i = 0; i < count && products.Count < totalProducts; i++)
        {
            var card = productCards.Nth(i);

            var title = (await SafeTextAsync(card.Locator(TitleSelector)))?.Trim();
            var price = (await SafeTextAsync(card.Locator(PriceSelector)))?.Trim();
            var merchant = (await SafeTextAsync(card.Locator(MerchantSelector)))?.Trim();
            var image = await SafeGetAttributeAsync(card.Locator(ImageSelector), "src");
            var link = await SafeGetAttributeAsync(card.Locator(LinkSelector), "href");
            var rating = (await SafeTextAsync(card.Locator(RatingSelector)))?.Trim();
            var reviews = (await SafeTextAsync(card.Locator(ReviewsSelector)))?.Trim();

            products.Add(new Product
            {
                Title = title,
                Price = price,
                Merchant = merchant,
                Image = image,
                Rating = rating,
                Reviews = reviews,
                Link = link
            });
        }

        return products;
    }

    private static async Task<string?> SafeTextAsync(ILocator locator)
    {
        try
        {
            return await locator.First.TextContentAsync(new LocatorTextContentOptions { Timeout = 500 });
        }
        catch
        {
            return null;
        }
    }

    private static async Task<string?> SafeGetAttributeAsync(ILocator locator, string attribute)
    {
        try
        {
            return await locator.First.GetAttributeAsync(attribute, new LocatorGetAttributeOptions { Timeout = 500 });
        }
        catch
        {
            return null;
        }
    }
}
