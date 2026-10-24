using MongoDB.Bson;
using MongoDB.Driver;
using ShopStream.Catalog.Application;
using ShopStream.Catalog.Domain;

namespace ShopStream.Catalog.Infrastructure;

internal sealed class CategoryRepository(CatalogMongoContext context) : ICategoryRepository
{
    public bool IsValidId(string id) => ObjectId.TryParse(id, out _);

    public async Task<Category?> GetByIdAsync(string id, CancellationToken ct) =>
        await context.Categories.Find(c => c.Id == id).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<Category>> ListAsync(CancellationToken ct) =>
        await context.Categories.Find(FilterDefinition<Category>.Empty).SortBy(c => c.Name).ToListAsync(ct);

    public async Task<bool> AddAsync(Category category, CancellationToken ct)
    {
        try
        {
            await context.Categories.InsertOneAsync(category, cancellationToken: ct);
            return true;
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            return false;
        }
    }
}
