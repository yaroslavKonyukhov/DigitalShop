using System.Net.Http.Json;
using System.Text.Json;
using Ordering.Application.DTOs;
using Ordering.Application.Interfaces;
using System.Net.Http.Json;

namespace Ordering.Application.ExternalServices;

public class CatalogClient : ICatalogClient
{
    private readonly HttpClient _httpClient;

    public CatalogClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CatalogProductDto?> GetProductAsync(Guid productId)
    {
        try
        {
            Console.WriteLine($"[DEBUG] Запрос к каталогу: {_httpClient.BaseAddress}api/catalog/products/{productId}");

            var response = await _httpClient.GetAsync($"api/catalog/products/{productId}");

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var product = await response.Content.ReadFromJsonAsync<CatalogProductDto>(options);

                if (product != null)
                {
                    Console.WriteLine($"[DEBUG] Товар получен: {product.Name}, Цена: {product.Price}");
                }

                return product;
            }

            Console.WriteLine($"[WARNING] Каталог ответил статусом: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CatalogClient Error]: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> UpdateStockAsync(Guid productId, int quantityChange)
    {
        try
        {
            var response = await _httpClient.PatchAsync(
                $"api/catalog/products/{productId}/adjust-stock",
                JsonContent.Create(quantityChange)
            );

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CatalogClient Error]: {ex.Message}");
            return false;
        }
    }
}