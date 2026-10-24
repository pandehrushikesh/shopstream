using ShopStream.BuildingBlocks;

namespace ShopStream.Catalog.UnitTests;

public class CursorTests
{
    [Theory]
    [InlineData("507f1f77bcf86cd799439011")]
    [InlineData("a")]
    [InlineData("ab")]
    [InlineData("abc?>~")]
    public void Encode_then_decode_round_trips(string key)
    {
        var cursor = Cursor.Encode(key);

        cursor.Should().NotContainAny("+", "/", "=");
        Cursor.TryDecode(cursor, out var decoded).Should().BeTrue();
        decoded.Should().Be(key);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("%%%not-base64%%%")]
    public void TryDecode_rejects_garbage(string? cursor)
    {
        Cursor.TryDecode(cursor, out _).Should().BeFalse();
    }
}
