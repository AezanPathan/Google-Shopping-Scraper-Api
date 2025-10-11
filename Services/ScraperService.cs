using Microsoft.Playwright;
using GoogleShoppingScraperApi.Models;

namespace GoogleShoppingScraperApi.Services;

public class ScraperService
{
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

    await page.Locator("[data-cid]").First.WaitForAsync();

        int prevCount = 0;
        for (int scrolls = 0; scrolls < maxScroll; scrolls++) // adjust max scrolls
        {
            await page.EvaluateAsync("window.scrollBy(0, document.body.scrollHeight)");
            await page.WaitForTimeoutAsync(2000); // wait for new items to load

            int currentCount = await page.Locator("[data-cid]").CountAsync();
            if (currentCount == prevCount)
                break; 

            prevCount = currentCount;
        }

        var products = new List<Product>();
        var productCards = page.Locator("[data-cid]");
        int count = await productCards.CountAsync();

        for (int i = 0; i < count && products.Count < totalProducts; i++)
        {
            var card = productCards.Nth(i);

            var title = (await SafeTextAsync(card.Locator("div.gkQHve")))?.Trim();
            var price = (await SafeTextAsync(card.Locator("span[aria-label^='Current price']")))?.Trim();
            var merchant = (await SafeTextAsync(card.Locator("span.WJMUdc")))?.Trim();
            var image = await SafeGetAttributeAsync(card.Locator("div.R1iPve img"), "src");
            var link = await SafeGetAttributeAsync(card.Locator("a"), "href");
            var rating = (await SafeTextAsync(card.Locator("div.LFROUd span.yi40Hd")))?.Trim();
            var reviews = (await SafeTextAsync(card.Locator("div.LFROUd span.RDApEe")))?.Trim();

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
