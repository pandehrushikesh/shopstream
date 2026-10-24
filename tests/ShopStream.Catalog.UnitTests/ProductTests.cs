using ShopStream.Catalog.Domain;

namespace ShopStream.Catalog.UnitTests;

public class ProductTests
{
    private static readonly DateTime Now = new(2026, 10, 24, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_input_succeeds_and_normalises()
    {
        var result = Product.Create(" SKU-1 ", " Lamp ", null, "cat1", 19.99m, "usd", null, Now);

        result.IsSuccess.Should().BeTrue();
        result.Value.Sku.Should().Be("SKU-1");
        result.Value.Name.Should().Be("Lamp");
        result.Value.Currency.Should().Be("USD");
        result.Value.CreatedAtUtc.Should().Be(Now);
    }

    [Theory]
    [InlineData("", "Lamp", 10, "USD", "product.invalid_sku")]
    [InlineData("S1", " ", 10, "USD", "product.invalid_name")]
    [InlineData("S1", "Lamp", 0, "USD", "product.invalid_price")]
    [InlineData("S1", "Lamp", -5, "USD", "product.invalid_price")]
    [InlineData("S1", "Lamp", 10.123, "USD", "product.invalid_price")]
    [InlineData("S1", "Lamp", 10, "US", "product.invalid_currency")]
    public void Create_with_invalid_input_fails(string sku, string name, double price, string currency, string code)
    {
        var result = Product.Create(sku, name, null, "cat1", (decimal)price, currency, null, Now);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(code);
    }

    [Fact]
    public void Update_changes_fields_but_not_sku()
    {
        var product = Product.Create("S1", "Lamp", null, "cat1", 10m, "USD", null, Now).Value;
        var later = Now.AddHours(1);

        var result = product.Update("Better lamp", "desc", "cat2", 12.5m, "eur", null, false, later);

        result.IsSuccess.Should().BeTrue();
        product.Sku.Should().Be("S1");
        product.Name.Should().Be("Better lamp");
        product.Currency.Should().Be("EUR");
        product.IsActive.Should().BeFalse();
        product.UpdatedAtUtc.Should().Be(later);
        product.CreatedAtUtc.Should().Be(Now);
    }

    [Fact]
    public void Failed_update_leaves_product_unchanged()
    {
        var product = Product.Create("S1", "Lamp", null, "cat1", 10m, "USD", null, Now).Value;

        var result = product.Update("", null, "cat1", 10m, "USD", null, true, Now.AddHours(1));

        result.IsFailure.Should().BeTrue();
        product.Name.Should().Be("Lamp");
        product.UpdatedAtUtc.Should().Be(Now);
    }
}
