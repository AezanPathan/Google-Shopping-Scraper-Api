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
            app.MapGet("/scrape/product", async (string query) =>
            {
                var scraper = new ScraperService();
                var product = await scraper.ScrapProduct(query);

                return Results.Ok(product);
            })
            .WithName("ScrapeProduct")
            .WithTags("Scraper");
    }
}
