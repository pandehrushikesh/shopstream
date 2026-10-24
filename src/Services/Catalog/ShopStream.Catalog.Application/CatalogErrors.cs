using ShopStream.BuildingBlocks;

namespace ShopStream.Catalog.Application;

public static class CatalogErrors
{
    public static Error ProductNotFound(string id) =>
        Error.NotFound("catalog.product_not_found", $"Product '{id}' was not found.");

    public static Error CategoryNotFound(string id) =>
        Error.NotFound("catalog.category_not_found", $"Category '{id}' was not found.");

    public static Error SkuTaken(string sku) =>
        Error.Conflict("catalog.sku_taken", $"A product with SKU '{sku}' already exists.");

    public static Error SlugTaken(string slug) =>
        Error.Conflict("catalog.slug_taken", $"A category with slug '{slug}' already exists.");

    public static readonly Error InvalidCursor =
        Error.Validation("catalog.invalid_cursor", "The cursor is not valid.");

    public static Error InvalidCategoryFilter(string id) =>
        Error.Validation("catalog.invalid_category_id", $"'{id}' is not a valid category id.");
}
