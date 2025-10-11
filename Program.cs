using Microsoft.OpenApi.Models;
using GoogleShoppingScraperApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ShoppingScraper API",
        Version = "v1",
        Description = "API for scraping Google Shopping"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ShoppingScraper API v1");
    });
//}

app.UseHttpsRedirection();

// Register endpoints (clean!)
app.MapScraperEndpoints();

app.Run();
