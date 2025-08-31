using GoogleShoppingScraperApi.Models;

namespace GoogleShoppingScraperApi.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var products = new List<Product>
        {
            new Product(1, "iPhone 15", 1200.00m),
            new Product(2, "Samsung Galaxy S24", 999.99m),
            new Product(3, "Google Pixel 9", 899.99m)
        };

        // GET all products
        app.MapGet("/products", () => products)
           .WithName("GetProducts")
           .WithTags("Products");

        // GET product by ID
        app.MapGet("/products/{id}", (int id) =>
            products.FirstOrDefault(p => p.Id == id) is Product product
                ? Results.Ok(product)
                : Results.NotFound(new { Message = $"Product with id {id} not found" })
        )
        .WithName("GetProductById")
        .WithTags("Products");
    }
}
