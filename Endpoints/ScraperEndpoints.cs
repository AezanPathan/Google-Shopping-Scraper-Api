using GoogleShoppingScraperApi.Services;

namespace GoogleShoppingScraperApi.Endpoints;

public static class ScraperEndpoints
{
    public static void MapScraperEndpoints(this WebApplication app)
    {
        // Test scrape endpoint
        app.MapGet("/scrape", async () =>
        {
            var scraper = new ScraperService();
            var title = await scraper.ScrapeGoogleShoppingTitleAsync();

            return Results.Ok(new { Title = title });
        })
        .WithName("TestScraper")
        .WithTags("Scraper");
    }
}
