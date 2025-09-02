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

        // wait until at least one product card is visible
        await page.Locator("[data-cid]").First.WaitForAsync();

        var products = new List<ProductDto>();
        var productCards = page.Locator("[data-cid]");
        int count = await productCards.CountAsync();

        for (int i = 0; i < count; i++)
        {
            //if (i >= 7)
            //    return products;

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


            // var title = await card.Locator("div.gkQHve").First.TextContentAsync();
            // var price = await card.Locator("div.zxVpA span.lmQWe").First.TextContentAsync();
            // var merchant = await card.Locator("div.n7emVc span.WJMUdc").First.TextContentAsync();
            // var image = await card.Locator("div.R1iPve img").First.GetAttributeAsync("src");
            // var link = await card.Locator("a").First.GetAttributeAsync("href");
            //var rating = await card.Locator("div.LFROUd span.yi40Hd").First.TextContentAsync().CatchAsync(_ => null);
            //var reviews = await card.Locator("div.LFROUd span.RDApEe").First.TextContentAsync().CatchAsync(_ => null);

            //if (!string.IsNullOrEmpty(link) && link.StartsWith("/"))
            //    link = "https://www.google.com" + link;

            products.Add(new ProductDto
            {
                Title = title?.Trim(),
                Price = price?.Trim(),
                Merchant = merchant?.Trim(),
                Image = image,
               // Link = link,
                Rating = rating?.Trim(),
                Reviews = reviews?.Trim()
            });
        }

        return products;
    }

    //public async Task<string> ScrapProduct(string productName)
    //{
    //    using var playwright = await Playwright.CreateAsync();
    //    await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
    //    {
    //        Headless = false // keep false for testing, captcha happens less
    //    });

    //    var context = await browser.NewContextAsync();
    //    var page = await context.NewPageAsync();

    //    // Encode product name
    //    var query = System.Web.HttpUtility.UrlEncode(productName);

    //    // Wait specifically for the network response we care about
    //    var response = await page.RunAndWaitForResponseAsync(
    //        async () =>
    //        {
    //            await page.GotoAsync($"https://www.google.com/search?tbm=shop&q={query}");
    //        },
    //        r => r.Url.Contains("/async/shoppingsearch") // condition: Google Shopping API request
    //    );

    //    // Get the response body
    //    var apiResponse = await response.TextAsync();

    //    return string.IsNullOrEmpty(apiResponse) ? "No network data found" : apiResponse;
    //}
    /* public async Task<string> ScrapProduct(string productName)
     {
         using var playwright = await Playwright.CreateAsync();
         await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
         {
             Headless = false
         });

         var context = await browser.NewContextAsync();
         var page = await context.NewPageAsync();

         string? apiResponse = null;

         // Just listen, don’t wait-for-event (avoids timeout)
         page.Response += async (_, response) =>
         {
             try
             {
                 if (response.Url.Contains("GetAsyncData"))
                 {
                     Console.WriteLine("✅ Found GetAsyncData response: " + response.Url);
                     var body = await response.TextAsync();
                     apiResponse = body;
                 }
             }
             catch (Exception ex)
             {
                 Console.WriteLine("❌ Response read error: " + ex.Message);
             }
         };

         var query = System.Web.HttpUtility.UrlEncode(productName);
         await page.GotoAsync($"https://www.google.com/search?tbm=shop&q={query}");

         // Instead of hard waiting, poll until we get something or timeout manually
         int retries = 0;
         while (apiResponse == null && retries < 20)
         {
             await Task.Delay(500);
             retries++;
         }

         return apiResponse ?? "No network data found";
     }*/


}
