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

        // ✅ Product scraping endpoint
        app.MapGet("/scrape/product", async (string query, int? maxScroll, int? totalProducts) =>
        {
            var scraper = new ScraperService();

            // provide sensible defaults if not passed
            int scrolls = maxScroll ?? 10;
            int limit = totalProducts ?? 100;

            var products = await scraper.ScrapProduct(query, scrolls, limit);

            return Results.Ok(products);
        })
 .WithName("ScrapeProduct")
 .WithTags("Scraper");

    }
}
