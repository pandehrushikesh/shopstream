using ShopStream.BuildingBlocks;
using ShopStream.BuildingBlocks.Web;
using ShopStream.Catalog.Application;

namespace ShopStream.Catalog.Api;

public static class Endpoints
{
    public static RouteGroupBuilder MapProductEndpoints(this RouteGroupBuilder g)
    {
        g.MapGet("/", async (ProductService svc, string? categoryId, string? cursor, int? limit, CancellationToken ct) =>
            Ok(await svc.ListAsync(categoryId, cursor, limit, ct)))
            .WithSummary("List products (keyset pagination). Pass nextCursor back as ?cursor= for the next page.");

        g.MapGet("/{id}", async (string id, ProductService svc, CancellationToken ct) =>
            Ok(await svc.GetAsync(id, ct)));

        g.MapPost("/", async (CreateProductRequest body, ProductService svc, HttpContext http, CancellationToken ct) =>
        {
            var result = await svc.CreateAsync(body, ct);
            if (result.IsFailure) return result.Error.ToProblem();
            return Results.Created($"{http.Request.Path.Value!.TrimEnd('/')}/{result.Value.Id}", result.Value);
        });

        g.MapPut("/{id}", async (string id, UpdateProductRequest body, ProductService svc, CancellationToken ct) =>
            Ok(await svc.UpdateAsync(id, body, ct)));

        g.MapDelete("/{id}", async (string id, ProductService svc, CancellationToken ct) =>
        {
            var result = await svc.DeleteAsync(id, ct);
            return result.IsSuccess ? Results.NoContent() : result.Error.ToProblem();
        });

        return g;
    }

    public static RouteGroupBuilder MapCategoryEndpoints(this RouteGroupBuilder g)
    {
        g.MapGet("/", async (CategoryService svc, CancellationToken ct) => Results.Ok(await svc.ListAsync(ct)));

        g.MapGet("/{id}", async (string id, CategoryService svc, CancellationToken ct) =>
            Ok(await svc.GetAsync(id, ct)));

        g.MapPost("/", async (CreateCategoryRequest body, CategoryService svc, HttpContext http, CancellationToken ct) =>
        {
            var result = await svc.CreateAsync(body, ct);
            if (result.IsFailure) return result.Error.ToProblem();
            return Results.Created($"{http.Request.Path.Value!.TrimEnd('/')}/{result.Value.Id}", result.Value);
        });

        return g;
    }

    private static IResult Ok<T>(Result<T> result) =>
        result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
}
