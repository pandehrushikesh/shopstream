# Build context is the repository root (see deploy/docker-compose.yml).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/Services/Catalog/ShopStream.Catalog.Api -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
# .NET 8+ images listen on 8080 and run as the non-root "app" user (UID 1654).
USER $APP_UID
ENTRYPOINT ["dotnet", "ShopStream.Catalog.Api.dll"]
