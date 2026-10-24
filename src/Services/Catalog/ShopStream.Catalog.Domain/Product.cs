using ShopStream.BuildingBlocks;

namespace ShopStream.Catalog.Domain;

public sealed class Product
{
    public const int MaxNameLength = 200;
    public const int MaxDescriptionLength = 4000;
    public const int MaxSkuLength = 64;
    public const decimal MaxPrice = 1_000_000m;

    private Product() { } // for the persistence layer

    // Id is assigned by the persistence layer on insert.
    public string Id { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string CategoryId { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = "USD";
    public Dictionary<string, string> Attributes { get; private set; } = new();
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static Result<Product> Create(
        string? sku, string? name, string? description, string? categoryId,
        decimal price, string? currency, IDictionary<string, string>? attributes, DateTime nowUtc)
    {
        sku = sku?.Trim();
        if (string.IsNullOrEmpty(sku) || sku.Length > MaxSkuLength)
            return Error.Validation("product.invalid_sku", $"SKU is required and must be at most {MaxSkuLength} characters.");

        var error = ValidateCommon(name, description, categoryId, price, currency);
        if (error is not null) return error;

        return new Product
        {
            Sku = sku,
            Name = name!.Trim(),
            Description = description?.Trim() ?? string.Empty,
            CategoryId = categoryId!,
            Price = price,
            Currency = currency!.Trim().ToUpperInvariant(),
            Attributes = attributes is null ? new() : new Dictionary<string, string>(attributes),
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc
        };
    }

    /// <summary>SKU is immutable by design: other services reference it.</summary>
    public Result Update(
        string? name, string? description, string? categoryId, decimal price, string? currency,
        IDictionary<string, string>? attributes, bool isActive, DateTime nowUtc)
    {
        var error = ValidateCommon(name, description, categoryId, price, currency);
        if (error is not null) return Result.Failure(error);

        Name = name!.Trim();
        Description = description?.Trim() ?? string.Empty;
        CategoryId = categoryId!;
        Price = price;
        Currency = currency!.Trim().ToUpperInvariant();
        Attributes = attributes is null ? new() : new Dictionary<string, string>(attributes);
        IsActive = isActive;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }

    private static Error? ValidateCommon(string? name, string? description, string? categoryId, decimal price, string? currency)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > MaxNameLength)
            return Error.Validation("product.invalid_name", $"Name is required and must be at most {MaxNameLength} characters.");
        if (description is { Length: > MaxDescriptionLength })
            return Error.Validation("product.invalid_description", $"Description must be at most {MaxDescriptionLength} characters.");
        if (string.IsNullOrWhiteSpace(categoryId))
            return Error.Validation("product.invalid_category", "CategoryId is required.");
        if (price <= 0 || price > MaxPrice || decimal.Round(price, 2) != price)
            return Error.Validation("product.invalid_price", $"Price must be greater than 0, at most {MaxPrice}, with at most 2 decimals.");
        if (currency is null || currency.Trim().Length != 3 || !currency.Trim().All(char.IsLetter))
            return Error.Validation("product.invalid_currency", "Currency must be a 3-letter ISO code.");
        return null;
    }
}
