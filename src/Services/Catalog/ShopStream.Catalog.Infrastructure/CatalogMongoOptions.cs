namespace ShopStream.Catalog.Infrastructure;

public sealed class CatalogMongoOptions
{
    public const string SectionName = "Mongo";

    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string Database { get; set; } = "catalog";

    /// <summary>Seed demo data on startup when the products collection is empty.</summary>
    public bool SeedOnStartup { get; set; }
    public int SeedProductCount { get; set; } = 10_000;
}
