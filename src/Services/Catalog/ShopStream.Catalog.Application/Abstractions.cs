using ShopStream.Catalog.Domain;

namespace ShopStream.Catalog.Application;

// Ports implemented by Infrastructure. The Application layer never sees MongoDB types.

public interface IProductRepository
{
    /// <summary>True when the string is a well-formed identifier for this store.</summary>
    bool IsValidId(string id);

    Task<Product?> GetByIdAsync(string id, CancellationToken ct);

    /// <summary>Items with Id greater than afterId (or from the start), ordered by Id ascending.</summary>
    Task<IReadOnlyList<Product>> ListAfterAsync(string? categoryId, string? afterId, int take, CancellationToken ct);

    /// <summary>Returns false if the SKU already exists (enforced atomically by a unique index).</summary>
    Task<bool> AddAsync(Product product, CancellationToken ct);

    Task<bool> ReplaceAsync(Product product, CancellationToken ct);
    Task<bool> DeleteAsync(string id, CancellationToken ct);
}

public interface ICategoryRepository
{
    bool IsValidId(string id);
    Task<Category?> GetByIdAsync(string id, CancellationToken ct);
    Task<IReadOnlyList<Category>> ListAsync(CancellationToken ct);

    /// <summary>Returns false if the slug already exists.</summary>
    Task<bool> AddAsync(Category category, CancellationToken ct);
}
