FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["IdentityService/Identity.API/Identity.API.csproj", "IdentityService/Identity.API/"]
COPY ["IdentityService/Identity.Application/Identity.Application.csproj", "IdentityService/Identity.Application/"]
COPY ["IdentityService/Identity.Infrastructure/Identity.Infrastructure.csproj", "IdentityService/Identity.Infrastructure/"]

RUN dotnet restore "IdentityService/Identity.API/Identity.API.csproj"

COPY . .

WORKDIR "/src/IdentityService/Identity.API"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Identity.API.dll"]