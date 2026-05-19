FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["OrderingService/Ordering.API/Ordering.API.csproj", "OrderingService/Ordering.API/"]
COPY ["OrderingService/Ordering.Application/Ordering.Application.csproj", "OrderingService/Ordering.Application/"]
COPY ["OrderingService/Ordering.Infrastructure/Ordering.Infrastructure.csproj", "OrderingService/Ordering.Infrastructure/"]
COPY ["EventBus.Messages/EventBus.Messages.csproj", "EventBus.Messages/"]

RUN dotnet restore "OrderingService/Ordering.API/Ordering.API.csproj"

COPY . .

WORKDIR "/src/OrderingService/Ordering.API"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Ordering.API.dll"]