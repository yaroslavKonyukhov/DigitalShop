using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger-definitions/identity/swagger.json", "Identity API");
    options.SwaggerEndpoint("/swagger-definitions/catalog/swagger.json", "Catalog API");
    options.SwaggerEndpoint("/swagger-definitions/ordering/swagger.json", "Ordering API");

    options.RoutePrefix = "swagger";
    options.DocumentTitle = "DigitalShop Unified Gateway Swagger";
});

app.UseRouting();

app.MapGet("/swagger-definitions/{apiName}/swagger.json", async (
    string apiName,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    CancellationToken cancellationToken) =>
{
    var clusterId = apiName.ToLowerInvariant() switch
    {
        "identity" => "identity-cluster",
        "catalog" => "catalog-cluster",
        "ordering" => "ordering-cluster",
        _ => null
    };

    if (clusterId is null)
    {
        return Results.NotFound();
    }

    var destinationAddress = configuration[$"ReverseProxy:Clusters:{clusterId}:Destinations:destination1:Address"];
    if (string.IsNullOrWhiteSpace(destinationAddress))
    {
        return Results.Problem($"Swagger destination is not configured for {apiName}.");
    }

    var swaggerUrl = new Uri(new Uri(destinationAddress), "swagger/v1/swagger.json");
    var client = httpClientFactory.CreateClient();
    var response = await client.GetAsync(swaggerUrl, cancellationToken);
    var content = await response.Content.ReadAsStringAsync(cancellationToken);

    return Results.Content(content, "application/json", statusCode: (int)response.StatusCode);
});

app.MapReverseProxy();

app.Run();
