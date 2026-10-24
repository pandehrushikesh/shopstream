using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ShopStream.Catalog.Infrastructure;

/// <summary>
/// Creates indexes (idempotent) and optionally seeds demo data before the app serves traffic.
/// If MongoDB is unreachable this throws and the host fails fast; the orchestrator restarts it.
/// </summary>
internal sealed class CatalogInitializer(
    CatalogMongoContext context, CatalogSeeder seeder, IOptions<CatalogMongoOptions> options) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        await context.EnsureIndexesAsync(ct);
        if (options.Value.SeedOnStartup)
            await seeder.SeedIfEmptyAsync(options.Value.SeedProductCount, ct);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
