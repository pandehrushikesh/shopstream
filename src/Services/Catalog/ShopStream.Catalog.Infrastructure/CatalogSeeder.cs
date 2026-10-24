using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ShopStream.Catalog.Domain;

namespace ShopStream.Catalog.Infrastructure;

/// <summary>Deterministic demo data (fixed random seed) so load tests are repeatable.</summary>
public sealed partial class CatalogSeeder(CatalogMongoContext context, TimeProvider clock, ILogger<CatalogSeeder> logger)
{
    private static readonly string[] CategoryNames =
        ["Electronics", "Books", "Home & Kitchen", "Sports", "Toys", "Fashion", "Beauty", "Grocery", "Automotive", "Garden"];
    private static readonly string[] Brands = ["Acme", "Nova", "Zenith", "Orbit", "Pinnacle", "Lumen", "Vertex", "Aster"];
    private static readonly string[] Nouns = ["Lamp", "Backpack", "Speaker", "Bottle", "Chair", "Notebook", "Headset", "Mug", "Jacket", "Watch"];
    private static readonly string[] Colors = ["black", "white", "red", "blue", "green", "grey"];

    public async Task SeedIfEmptyAsync(int productCount, CancellationToken ct)
    {
        var any = await context.Products.Find(FilterDefinition<Product>.Empty).Limit(1).AnyAsync(ct);
        if (any) return;

        var rng = new Random(42);
        var now = clock.GetUtcNow().UtcDateTime;

        var categories = CategoryNames.Select(n => Category.Create(n, Slugify(n)).Value).ToList();
        await context.Categories.InsertManyAsync(categories, cancellationToken: ct); // assigns Ids

        var batch = new List<Product>(1000);
        for (var i = 1; i <= productCount; i++)
        {
            var category = categories[rng.Next(categories.Count)];
            var brand = Brands[rng.Next(Brands.Length)];
            var noun = Nouns[rng.Next(Nouns.Length)];
            var price = Math.Round((decimal)(rng.NextDouble() * 1995 + 5), 2);

            var product = Product.Create(
                $"SKU-{i:D6}", $"{brand} {noun} {i}", $"Seeded demo product #{i}.", category.Id, price, "USD",
                new Dictionary<string, string> { ["brand"] = brand, ["color"] = Colors[rng.Next(Colors.Length)] },
                now).Value;
            batch.Add(product);

            if (batch.Count == 1000)
            {
                await context.Products.InsertManyAsync(batch, cancellationToken: ct);
                batch.Clear();
            }
        }
        if (batch.Count > 0) await context.Products.InsertManyAsync(batch, cancellationToken: ct);

        logger.LogInformation("Seeded {Categories} categories and {Products} products", categories.Count, productCount);
    }

    private static string Slugify(string name) =>
        NonAlphanumeric().Replace(name.ToLowerInvariant().Replace("&", "and"), "-").Trim('-');

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumeric();
}
