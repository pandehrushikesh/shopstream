using System.Text.RegularExpressions;
using ShopStream.BuildingBlocks;

namespace ShopStream.Catalog.Domain;

public sealed partial class Category
{
    private Category() { }

    public string Id { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;

    public static Result<Category> Create(string? name, string? slug)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 100)
            return Error.Validation("category.invalid_name", "Name is required and must be at most 100 characters.");
        if (string.IsNullOrWhiteSpace(slug) || slug.Length > 100 || !SlugPattern().IsMatch(slug))
            return Error.Validation("category.invalid_slug", "Slug must be lowercase letters, digits and single hyphens.");

        return new Category { Name = name.Trim(), Slug = slug };
    }

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugPattern();
}
