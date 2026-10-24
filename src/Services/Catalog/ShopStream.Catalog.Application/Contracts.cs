using ShopStream.Catalog.Domain;

namespace ShopStream.Catalog.Application;

public sealed record ProductDto(
    string Id, string Sku, string Name, string Description, string CategoryId,
    decimal Price, string Currency, IReadOnlyDictionary<string, string> Attributes,
    bool IsActive, DateTime CreatedAtUtc, DateTime UpdatedAtUtc);

public sealed record CreateProductRequest(
    string Sku, string Name, string? Description, string CategoryId,
    decimal Price, string Currency, Dictionary<string, string>? Attributes);

public sealed record UpdateProductRequest(
    string Name, string? Description, string CategoryId, decimal Price, string Currency,
    Dictionary<string, string>? Attributes, bool IsActive);

public sealed record CategoryDto(string Id, string Name, string Slug);

public sealed record CreateCategoryRequest(string Name, string Slug);

public static class Mapping
{
    public static ProductDto ToDto(this Product p) => new(
        p.Id, p.Sku, p.Name, p.Description, p.CategoryId, p.Price, p.Currency,
        p.Attributes, p.IsActive, p.CreatedAtUtc, p.UpdatedAtUtc);

    public static CategoryDto ToDto(this Category c) => new(c.Id, c.Name, c.Slug);
}
