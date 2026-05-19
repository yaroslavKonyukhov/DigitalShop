FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем проекты (пути от корня DigitalShop)
COPY ["CatalogService/Catalog.API/Catalog.API.csproj", "CatalogService/Catalog.API/"]
COPY ["CatalogService/Catalog.Application/Catalog.Application.csproj", "CatalogService/Catalog.Application/"]
COPY ["CatalogService/Catalog.Infrastructure/Catalog.Infrastructure.csproj", "CatalogService/Catalog.Infrastructure/"]
COPY ["EventBus.Messages/EventBus.Messages.csproj", "EventBus.Messages/"]

RUN dotnet restore "CatalogService/Catalog.API/Catalog.API.csproj"

# Копируем всё содержимое корня
COPY . .

WORKDIR "/src/CatalogService/Catalog.API"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Catalog.API.dll"]