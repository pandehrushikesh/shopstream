using Asp.Versioning;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Serilog;
using ShopStream.BuildingBlocks.Web;
using ShopStream.Catalog.Api;
using ShopStream.Catalog.Application;
using ShopStream.Catalog.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) =>
{
    lc.ReadFrom.Configuration(ctx.Configuration).Enrich.FromLogContext().WriteTo.Console();
    var seq = ctx.Configuration["Seq:Url"];
    if (!string.IsNullOrWhiteSpace(seq)) lc.WriteTo.Seq(seq);
});

builder.Services.AddBuildingBlocksWeb();
builder.Services.AddCatalogApplication();
builder.Services.AddCatalogInfrastructure(builder.Configuration);

builder.Services
    .AddApiVersioning(o =>
    {
        o.DefaultApiVersion = new ApiVersion(1, 0);
        o.ReportApiVersions = true; // api-supported-versions response header
        o.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddApiExplorer(o =>
    {
        o.GroupNameFormat = "'v'VVV";
        o.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
    o.SwaggerDoc("v1", new OpenApiInfo { Title = "ShopStream Catalog API", Version = "v1" }));

var app = builder.Build();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();

if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog API v1"));
}

// Liveness: "is the process up?" (no dependency checks, or a DB blip would restart healthy pods).
// Readiness: "can it serve traffic?" (includes MongoDB).
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = r => r.Tags.Contains("ready") });

// NOTE: no authentication yet. Phase 2 adds JWT validation and policy-based authorization
// (reads public, writes require the "catalog:write" scope).
var catalog = app.NewVersionedApi("Catalog");
var v1 = catalog.MapGroup("/api/v{version:apiVersion}").HasApiVersion(new ApiVersion(1, 0));
v1.MapGroup("/products").WithTags("Products").MapProductEndpoints();
v1.MapGroup("/categories").WithTags("Categories").MapCategoryEndpoints();

app.Run();

// Makes the entry point visible to WebApplicationFactory<Program> in integration tests.
public partial class Program;
