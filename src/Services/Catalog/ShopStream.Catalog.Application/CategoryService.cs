using ShopStream.BuildingBlocks;
using ShopStream.Catalog.Domain;

namespace ShopStream.Catalog.Application;

public sealed class CategoryService(ICategoryRepository categories)
{
    public async Task<Result<CategoryDto>> GetAsync(string id, CancellationToken ct)
    {
        var category = categories.IsValidId(id) ? await categories.GetByIdAsync(id, ct) : null;
        return category is null ? CatalogErrors.CategoryNotFound(id) : category.ToDto();
    }

    public async Task<IReadOnlyList<CategoryDto>> ListAsync(CancellationToken ct) =>
        (await categories.ListAsync(ct)).Select(c => c.ToDto()).ToList();

    public async Task<Result<CategoryDto>> CreateAsync(CreateCategoryRequest request, CancellationToken ct)
    {
        var created = Category.Create(request.Name, request.Slug);
        if (created.IsFailure) return created.Error;

        var category = created.Value;
        if (!await categories.AddAsync(category, ct)) return CatalogErrors.SlugTaken(category.Slug);
        return category.ToDto();
    }
}
