using ShopStream.BuildingBlocks;
using ShopStream.Catalog.Domain;

namespace ShopStream.Catalog.Application;

/// <summary>
/// Use-case layer. Deliberately plain classes (no MediatR): fewer moving parts, no licence concerns,
/// and the call path is obvious in a debugger.
/// </summary>
public sealed class ProductService(IProductRepository products, ICategoryRepository categories, TimeProvider clock)
{
    public const int DefaultLimit = 20;
    public const int MaxLimit = 100;

    public async Task<Result<ProductDto>> GetAsync(string id, CancellationToken ct)
    {
        var product = products.IsValidId(id) ? await products.GetByIdAsync(id, ct) : null;
        return product is null ? CatalogErrors.ProductNotFound(id) : product.ToDto();
    }

    public async Task<Result<KeysetPage<ProductDto>>> ListAsync(
        string? categoryId, string? cursor, int? limit, CancellationToken ct)
    {
        var take = Math.Clamp(limit ?? DefaultLimit, 1, MaxLimit);

        string? afterId = null;
        if (cursor is not null)
        {
            if (!Cursor.TryDecode(cursor, out var key) || !products.IsValidId(key))
                return CatalogErrors.InvalidCursor;
            afterId = key;
        }

        if (categoryId is not null && !categories.IsValidId(categoryId))
            return CatalogErrors.InvalidCategoryFilter(categoryId);

        // Fetch one extra row: its existence tells us there is a next page without a COUNT query.
        var rows = await products.ListAfterAsync(categoryId, afterId, take + 1, ct);
        var page = rows.Take(take).Select(p => p.ToDto()).ToList();
        var next = rows.Count > take ? Cursor.Encode(page[^1].Id) : null;

        return new KeysetPage<ProductDto>(page, next);
    }

    public async Task<Result<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken ct)
    {
        if (!await CategoryExistsAsync(request.CategoryId, ct))
            return CatalogErrors.CategoryNotFound(request.CategoryId);

        var created = Product.Create(
            request.Sku, request.Name, request.Description, request.CategoryId,
            request.Price, request.Currency, request.Attributes, clock.GetUtcNow().UtcDateTime);
        if (created.IsFailure) return created.Error;

        var product = created.Value;
        // No "exists?" pre-check: that would be a race. The unique index decides atomically.
        if (!await products.AddAsync(product, ct)) return CatalogErrors.SkuTaken(product.Sku);

        return product.ToDto();
    }

    public async Task<Result<ProductDto>> UpdateAsync(string id, UpdateProductRequest request, CancellationToken ct)
    {
        var product = products.IsValidId(id) ? await products.GetByIdAsync(id, ct) : null;
        if (product is null) return CatalogErrors.ProductNotFound(id);

        if (!await CategoryExistsAsync(request.CategoryId, ct))
            return CatalogErrors.CategoryNotFound(request.CategoryId);

        var updated = product.Update(
            request.Name, request.Description, request.CategoryId, request.Price, request.Currency,
            request.Attributes, request.IsActive, clock.GetUtcNow().UtcDateTime);
        if (updated.IsFailure) return updated.Error;

        if (!await products.ReplaceAsync(product, ct)) return CatalogErrors.ProductNotFound(id);
        return product.ToDto();
    }

    public async Task<Result> DeleteAsync(string id, CancellationToken ct)
    {
        var deleted = products.IsValidId(id) && await products.DeleteAsync(id, ct);
        return deleted ? Result.Success() : Result.Failure(CatalogErrors.ProductNotFound(id));
    }

    private async Task<bool> CategoryExistsAsync(string categoryId, CancellationToken ct) =>
        categories.IsValidId(categoryId) && await categories.GetByIdAsync(categoryId, ct) is not null;
}
