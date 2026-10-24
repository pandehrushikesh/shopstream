using System.Net;
using System.Net.Http.Json;
using ShopStream.BuildingBlocks;
using ShopStream.Catalog.Application;

namespace ShopStream.Catalog.IntegrationTests;

public class CatalogApiTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client = fixture.Client;

    private async Task<CategoryDto> NewCategoryAsync()
    {
        var slug = $"cat-{Guid.NewGuid():N}";
        var response = await _client.PostAsJsonAsync("/api/v1/categories", new CreateCategoryRequest($"Category {slug}", slug));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<CategoryDto>())!;
    }

    private Task<HttpResponseMessage> PostProductAsync(string categoryId, string? sku = null) =>
        _client.PostAsJsonAsync("/api/v1/products",
            new CreateProductRequest(sku ?? $"T-{Guid.NewGuid():N}", "Test product", null, categoryId, 9.99m, "USD", null));

    [Fact]
    public async Task Create_then_get_round_trips_through_mongodb()
    {
        var category = await NewCategoryAsync();

        var created = await PostProductAsync(category.Id);

        created.StatusCode.Should().Be(HttpStatusCode.Created);
        created.Headers.Location.Should().NotBeNull();
        var product = (await created.Content.ReadFromJsonAsync<ProductDto>())!;
        product.Price.Should().Be(9.99m);

        var fetched = await _client.GetFromJsonAsync<ProductDto>($"/api/v1/products/{product.Id}");
        fetched!.Sku.Should().Be(product.Sku);
        fetched.Price.Should().Be(9.99m); // proves Decimal128 round trip
    }

    [Fact]
    public async Task Duplicate_sku_returns_409_problem_details()
    {
        var category = await NewCategoryAsync();
        var sku = $"DUP-{Guid.NewGuid():N}";
        (await PostProductAsync(category.Id, sku)).StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await PostProductAsync(category.Id, sku);

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
        second.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        (await second.Content.ReadAsStringAsync()).Should().Contain("catalog.sku_taken");
    }

    [Fact]
    public async Task Keyset_pagination_returns_every_item_exactly_once()
    {
        var category = await NewCategoryAsync();
        for (var i = 0; i < 5; i++)
            (await PostProductAsync(category.Id)).StatusCode.Should().Be(HttpStatusCode.Created);

        var ids = new List<string>();
        string? cursor = null;
        var pages = 0;
        do
        {
            var url = $"/api/v1/products?categoryId={category.Id}&limit=2" +
                      (cursor is null ? "" : $"&cursor={Uri.EscapeDataString(cursor)}");
            var page = (await _client.GetFromJsonAsync<KeysetPage<ProductDto>>(url))!;
            ids.AddRange(page.Items.Select(p => p.Id));
            cursor = page.NextCursor;
            pages++;
        } while (cursor is not null);

        pages.Should().Be(3);
        ids.Should().HaveCount(5).And.OnlyHaveUniqueItems().And.BeInAscendingOrder();
    }

    [Fact]
    public async Task Invalid_cursor_returns_400()
    {
        var response = await _client.GetAsync("/api/v1/products?cursor=%25%25%25");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Unknown_product_returns_404()
    {
        var response = await _client.GetAsync("/api/v1/products/507f1f77bcf86cd799439011");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Malformed_product_id_returns_404_not_500()
    {
        var response = await _client.GetAsync("/api/v1/products/not-an-object-id");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task Health_endpoints_return_200(string path)
    {
        (await _client.GetAsync(path)).StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
