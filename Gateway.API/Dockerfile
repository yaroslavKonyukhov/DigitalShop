FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Gateway.API/Gateway.API.csproj", "Gateway.API/"]

RUN dotnet restore "Gateway.API/Gateway.API.csproj"

COPY . .

WORKDIR "/src/Gateway.API"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Gateway.API.dll"]
