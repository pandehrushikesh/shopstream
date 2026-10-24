using MongoDB.Bson;
using MongoDB.Driver;
using ShopStream.Catalog.Application;
using ShopStream.Catalog.Domain;

namespace ShopStream.Catalog.Infrastructure;

internal sealed class ProductRepository(CatalogMongoContext context) : IProductRepository
{
    public bool IsValidId(string id) => ObjectId.TryParse(id, out _);

    public async Task<Product?> GetByIdAsync(string id, CancellationToken ct) =>
        await context.Products.Find(p => p.Id == id).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<Product>> ListAfterAsync(
        string? categoryId, string? afterId, int take, CancellationToken ct)
    {
        var filters = Builders<Product>.Filter;
        var filter = FilterDefinition<Product>.Empty;
        if (categoryId is not null) filter &= filters.Eq(p => p.CategoryId, categoryId);
        if (afterId is not null) filter &= filters.Gt(p => p.Id, afterId);

        return await context.Products.Find(filter).SortBy(p => p.Id).Limit(take).ToListAsync(ct);
    }

    public async Task<bool> AddAsync(Product product, CancellationToken ct)
    {
        try
        {
            await context.Products.InsertOneAsync(product, cancellationToken: ct);
            return true;
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            return false;
        }
    }

    public async Task<bool> ReplaceAsync(Product product, CancellationToken ct)
    {
        var result = await context.Products.ReplaceOneAsync(p => p.Id == product.Id, product, cancellationToken: ct);
        return result.MatchedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct)
    {
        var result = await context.Products.DeleteOneAsync(p => p.Id == id, ct);
        return result.DeletedCount > 0;
    }
}
