using Microsoft.Playwright;

namespace GoogleShoppingScraperApi.Services;

public class ProductDto
{
    public string? Title { get; set; }
    public string? Price { get; set; }
    public string? Merchant { get; set; }
    public string? Image { get; set; }
    public string? Link { get; set; }
    public string? Rating { get; set; }
    public string? Reviews { get; set; }
}


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

    public async Task<List<ProductDto>> ScrapProduct(string productName)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false // set to true in production
        });

        var page = await browser.NewPageAsync();
        var query = System.Web.HttpUtility.UrlEncode(productName);
        await page.GotoAsync($"https://www.google.com/search?tbm=shop&q={query}", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });

        await page.Locator("[data-cid]").First.WaitForAsync();

        // 🔽 Keep scrolling until enough results are loaded (or until no more new ones appear)
        int prevCount = 0;
        for (int scrolls = 0; scrolls < 10; scrolls++) // adjust max scrolls
        {
            await page.EvaluateAsync("window.scrollBy(0, document.body.scrollHeight)");
            await page.WaitForTimeoutAsync(2000); // wait for new items to load

            int currentCount = await page.Locator("[data-cid]").CountAsync();
            if (currentCount == prevCount)
                break; // no new results loaded -> stop scrolling

            prevCount = currentCount;
        }

        var products = new List<ProductDto>();
        var productCards = page.Locator("[data-cid]");
        int count = await productCards.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var card = productCards.Nth(i);

            string? SafeText(ILocator locator)
            {
                try
                {
                    return locator.First.TextContentAsync(new LocatorTextContentOptions { Timeout = 500 })
                                  .GetAwaiter().GetResult();
                }
                catch
                {
                    return null;
                }
            }

            async Task<string?> SafeAttrAsync(ILocator locator, string attribute)
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

            var title = SafeText(card.Locator("div.gkQHve"));
            var price = SafeText(card.Locator("span[aria-label^='Current price']"));
            var merchant = SafeText(card.Locator("span.WJMUdc"));
            var image = await SafeAttrAsync(card.Locator("div.R1iPve img"), "src");
            var link = await SafeAttrAsync(card.Locator("a"), "href");
            var rating = SafeText(card.Locator("div.LFROUd span.yi40Hd"));
            var reviews = SafeText(card.Locator("div.LFROUd span.RDApEe"));

            products.Add(new ProductDto
            {
                Title = title?.Trim(),
                Price = price?.Trim(),
                Merchant = merchant?.Trim(),
                Image = image,
                Rating = rating?.Trim(),
                Reviews = reviews?.Trim()
            });
        }

        return products;
    }


}
