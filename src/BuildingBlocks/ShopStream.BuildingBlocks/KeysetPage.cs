using System.Text;

namespace ShopStream.BuildingBlocks;

/// <summary>One page of a keyset-paginated list. NextCursor is null on the last page.</summary>
public sealed record KeysetPage<T>(IReadOnlyList<T> Items, string? NextCursor);

/// <summary>Opaque cursor encoding (base64url of the last key seen). Clients must not parse it.</summary>
public static class Cursor
{
    public static string Encode(string key) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(key))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public static bool TryDecode(string? cursor, out string key)
    {
        key = string.Empty;
        if (string.IsNullOrWhiteSpace(cursor)) return false;
        try
        {
            var s = cursor.Replace('-', '+').Replace('_', '/');
            s = s.PadRight(s.Length + (4 - s.Length % 4) % 4, '=');
            key = Encoding.UTF8.GetString(Convert.FromBase64String(s));
            return key.Length > 0;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
