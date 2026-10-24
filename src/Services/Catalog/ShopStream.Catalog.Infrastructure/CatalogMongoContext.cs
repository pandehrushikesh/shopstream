using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ShopStream.Catalog.Domain;

namespace ShopStream.Catalog.Infrastructure;

public sealed class CatalogMongoContext
{
    public CatalogMongoContext(IMongoClient client, IOptions<CatalogMongoOptions> options)
    {
        Database = client.GetDatabase(options.Value.Database);
        Products = Database.GetCollection<Product>("products");
        Categories = Database.GetCollection<Category>("categories");
    }

    public IMongoDatabase Database { get; }
    public IMongoCollection<Product> Products { get; }
    public IMongoCollection<Category> Categories { get; }

    public async Task EnsureIndexesAsync(CancellationToken ct)
    {
        await Products.Indexes.CreateManyAsync(new[]
        {
            // Atomic uniqueness for SKU: the source of truth for "SKU already exists".
            new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Ascending(p => p.Sku),
                new CreateIndexOptions { Unique = true, Name = "ux_products_sku" }),
            // Serves "products in category X, paged by _id" without a sort step.
            new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Ascending(p => p.CategoryId).Ascending("_id"),
                new CreateIndexOptions { Name = "ix_products_category_id" })
        }, ct);

        await Categories.Indexes.CreateOneAsync(
            new CreateIndexModel<Category>(
                Builders<Category>.IndexKeys.Ascending(c => c.Slug),
                new CreateIndexOptions { Unique = true, Name = "ux_categories_slug" }),
            cancellationToken: ct);
    }
}
