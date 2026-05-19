using System.Text.Json.Serialization;

namespace Ordering.Application.DTOs;

public record CatalogProductDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("price")] decimal Price,
    [property: JsonPropertyName("stockQuantity")] int StockQuantity
);