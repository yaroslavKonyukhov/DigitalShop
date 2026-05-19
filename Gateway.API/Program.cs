using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

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
    options.SwaggerEndpoint("https://localhost:7203/swagger/v1/swagger.json", "Identity API");
    options.SwaggerEndpoint("https://localhost:7291/swagger/v1/swagger.json", "Catalog API");
    options.SwaggerEndpoint("https://localhost:7171/swagger/v1/swagger.json", "Ordering API");

    options.RoutePrefix = "swagger";
    options.DocumentTitle = "DigitalShop Unified Gateway Swagger";
});

app.UseRouting();

app.MapReverseProxy();

app.Run();