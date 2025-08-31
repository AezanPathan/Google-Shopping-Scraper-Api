using Microsoft.Playwright;

namespace GoogleShoppingScraperApi.Endpoints;

public static class ScraperEndpoints
{
    public static void MapScraperEndpoints(this WebApplication app)
    {
        // Test scrape endpoint
        app.MapGet("/scrape", async () =>
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true // set false if you want to see the browser window
            });

            var page = await browser.NewPageAsync();
            await page.GotoAsync("https://www.google.com/shopping");

            var title = await page.TitleAsync();

            return Results.Ok(new { Title = title });
        })
        .WithName("TestScraper")
        .WithTags("Scraper");
    }
}
