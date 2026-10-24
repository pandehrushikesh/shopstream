using System.Reflection;
using ShopStream.BuildingBlocks;
using ShopStream.Catalog.Application;
using ShopStream.Catalog.Domain;

namespace ShopStream.Catalog.UnitTests;

public class ProductServiceTests
{
    private readonly FakeProducts _products = new();
    private readonly FakeCategories _categories = new();
    private readonly ProductService _sut;

    public ProductServiceTests() =>
        _sut = new ProductService(_products, _categories, TimeProvider.System);

    private async Task<string> SeedCategoryAsync()
    {
        var category = Category.Create("Books", "books").Value;
        await _categories.AddAsync(category, default);
        return category.Id;
    }

    private static CreateProductRequest Request(string categoryId, string sku) =>
        new(sku, "Item " + sku, null, categoryId, 10m, "USD", null);

    [Fact]
    public async Task Create_unknown_category_is_not_found()
    {
        var result = await _sut.CreateAsync(Request("000000000000000000000999", "S1"), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Create_duplicate_sku_is_conflict()
    {
        var cat = await SeedCategoryAsync();
        (await _sut.CreateAsync(Request(cat, "S1"), default)).IsSuccess.Should().BeTrue();

        var second = await _sut.CreateAsync(Request(cat, "S1"), default);

        second.IsFailure.Should().BeTrue();
        second.Error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task List_walks_all_pages_without_duplicates()
    {
        var cat = await SeedCategoryAsync();
        for (var i = 1; i <= 5; i++) await _sut.CreateAsync(Request(cat, $"S{i}"), default);

        var seen = new List<string>();
        string? cursor = null;
        var pages = 0;
        do
        {
            var page = (await _sut.ListAsync(null, cursor, 2, default)).Value;
            seen.AddRange(page.Items.Select(i => i.Id));
            cursor = page.NextCursor;
            pages++;
        } while (cursor is not null);

        pages.Should().Be(3); // 2 + 2 + 1
        seen.Should().HaveCount(5).And.OnlyHaveUniqueItems().And.BeInAscendingOrder();
    }

    [Fact]
    public async Task List_exact_multiple_of_page_size_has_no_trailing_empty_page()
    {
        var cat = await SeedCategoryAsync();
        for (var i = 1; i <= 4; i++) await _sut.CreateAsync(Request(cat, $"S{i}"), default);

        var first = (await _sut.ListAsync(null, null, 2, default)).Value;
        var second = (await _sut.ListAsync(null, first.NextCursor, 2, default)).Value;

        second.Items.Should().HaveCount(2);
        second.NextCursor.Should().BeNull();
    }

    [Fact]
    public async Task List_invalid_cursor_is_validation_error()
    {
        var result = await _sut.ListAsync(null, "%%%", 10, default);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(1000, ProductService.MaxLimit)]
    public async Task List_clamps_limit(int requested, int expectedTake)
    {
        var cat = await SeedCategoryAsync();
        for (var i = 1; i <= 150; i++) await _sut.CreateAsync(Request(cat, $"S{i}"), default);

        var page = (await _sut.ListAsync(null, null, requested, default)).Value;

        page.Items.Should().HaveCount(expectedTake);
    }

    // ---- in-memory fakes ----
    private static string NextId(int n) => n.ToString("D24"); // 24 digits: valid-looking, sortable

    private sealed class FakeProducts : IProductRepository
    {
        private readonly List<Product> _items = [];
        public bool IsValidId(string id) => id.Length == 24 && id.All(char.IsAsciiHexDigit);
        public Task<Product?> GetByIdAsync(string id, CancellationToken ct) =>
            Task.FromResult(_items.FirstOrDefault(p => p.Id == id));

        public Task<IReadOnlyList<Product>> ListAfterAsync(string? categoryId, string? afterId, int take, CancellationToken ct)
        {
            IEnumerable<Product> q = _items;
            if (categoryId is not null) q = q.Where(p => p.CategoryId == categoryId);
            if (afterId is not null) q = q.Where(p => string.CompareOrdinal(p.Id, afterId) > 0);
            return Task.FromResult<IReadOnlyList<Product>>(q.OrderBy(p => p.Id, StringComparer.Ordinal).Take(take).ToList());
        }

        public Task<bool> AddAsync(Product product, CancellationToken ct)
        {
            if (_items.Any(p => p.Sku == product.Sku)) return Task.FromResult(false);
            typeof(Product).GetProperty(nameof(Product.Id), BindingFlags.Public | BindingFlags.Instance)!
                .SetValue(product, NextId(_items.Count + 1));
            _items.Add(product);
            return Task.FromResult(true);
        }

        public Task<bool> ReplaceAsync(Product product, CancellationToken ct) => Task.FromResult(true);
        public Task<bool> DeleteAsync(string id, CancellationToken ct) =>
            Task.FromResult(_items.RemoveAll(p => p.Id == id) > 0);
    }

    private sealed class FakeCategories : ICategoryRepository
    {
        private readonly List<Category> _items = [];
        public bool IsValidId(string id) => id.Length == 24 && id.All(char.IsAsciiHexDigit);
        public Task<Category?> GetByIdAsync(string id, CancellationToken ct) =>
            Task.FromResult(_items.FirstOrDefault(c => c.Id == id));
        public Task<IReadOnlyList<Category>> ListAsync(CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<Category>>(_items.ToList());

        public Task<bool> AddAsync(Category category, CancellationToken ct)
        {
            if (_items.Any(c => c.Slug == category.Slug)) return Task.FromResult(false);
            typeof(Category).GetProperty(nameof(Category.Id), BindingFlags.Public | BindingFlags.Instance)!
                .SetValue(category, NextId(_items.Count + 1));
            _items.Add(category);
            return Task.FromResult(true);
        }
    }
}
