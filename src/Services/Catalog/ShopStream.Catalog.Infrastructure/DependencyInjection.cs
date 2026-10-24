using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ShopStream.Catalog.Application;

namespace ShopStream.Catalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        MongoMapping.Register();

        services.Configure<CatalogMongoOptions>(configuration.GetSection(CatalogMongoOptions.SectionName));

        // MongoClient is thread-safe and expensive: exactly one per process.
        services.AddSingleton<IMongoClient>(sp =>
            new MongoClient(sp.GetRequiredService<IOptions<CatalogMongoOptions>>().Value.ConnectionString));

        services.AddSingleton<CatalogMongoContext>();
        services.AddSingleton<IProductRepository, ProductRepository>();
        services.AddSingleton<ICategoryRepository, CategoryRepository>();
        services.AddSingleton<CatalogSeeder>();
        services.AddHostedService<CatalogInitializer>();

        services.AddHealthChecks().AddCheck<MongoHealthCheck>("mongo", tags: ["ready"]);
        return services;
    }
}
