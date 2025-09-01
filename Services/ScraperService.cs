using Microsoft.Playwright;

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
}
