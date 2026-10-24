using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.MongoDb;

namespace ShopStream.Catalog.IntegrationTests;

/// <summary>Starts a real MongoDB in Docker and a real in-process Catalog API pointed at it.</summary>
public sealed class ApiFixture : IAsyncLifetime
{
    private readonly MongoDbContainer _mongo = new MongoDbBuilder().WithImage("mongo:7.0").Build();
    private CatalogApiFactory? _factory;

    public HttpClient Client { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        await _mongo.StartAsync();
        _factory = new CatalogApiFactory(_mongo.GetConnectionString());
        Client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        if (_factory is not null) await _factory.DisposeAsync();
        await _mongo.DisposeAsync();
    }
}

public sealed class CatalogApiFactory(string mongoConnectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // UseSetting is applied early enough for minimal-hosting Program.cs to see it.
        builder.UseSetting("Mongo:ConnectionString", mongoConnectionString);
        builder.UseSetting("Mongo:Database", "catalog_tests");
        builder.UseSetting("Mongo:SeedOnStartup", "false");
        builder.UseSetting("Seq:Url", "");
    }
}
