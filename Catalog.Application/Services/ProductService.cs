using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Catalog.Application.DTOs;
using Catalog.Application.Interfaces;
using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;
using StackExchange.Redis;

namespace Catalog.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IDatabase _cache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);

    private const string AllProductsKeyPrefix = "products_all_";
    private const string ProductKeyPrefix = "product_";

    public ProductService(IProductRepository repository, IConnectionMultiplexer redis)
    {
        _repository = repository;
        _cache = redis.GetDatabase();
    }

    public async Task<IEnumerable<ProductDto>> GetProductsAsync(bool isAdmin)
    {
        string cacheKey = $"{AllProductsKeyPrefix}{isAdmin}";

        var cachedData = await _cache.StringGetAsync(cacheKey);
        if (!cachedData.IsNullOrEmpty)
        {
            return JsonSerializer.Deserialize<List<ProductDto>>(cachedData.ToString())!;
        }

        var products = await _repository.GetAllAsync(includeOutOfStock: isAdmin);
        var dtos = products.Select(p => MapToDto(p)).ToList();

        await _cache.StringSetAsync(cacheKey, JsonSerializer.Serialize(dtos), _cacheDuration);

        return dtos;
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        string cacheKey = $"{ProductKeyPrefix}{id}";

        var cachedData = await _cache.StringGetAsync(cacheKey);
        if (!cachedData.IsNullOrEmpty)
        {
            return JsonSerializer.Deserialize<ProductDto>(cachedData.ToString())!;
        }

        var p = await _repository.GetByIdAsync(id);
        if (p == null) return null;

        var dto = MapToDto(p);

        await _cache.StringSetAsync(cacheKey, JsonSerializer.Serialize(dto), _cacheDuration);

        return dto;
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            CategoryId = dto.CategoryId
        };

        await _repository.AddAsync(product);

        await ClearGeneralCache();

        return MapToDto(product);
    }

    public async Task UpdateStockAsync(Guid id, int newQuantity)
    {
        await _repository.UpdateStockAsync(id, newQuantity);
        await InvalidateProductCache(id);
    }

    public async Task UpdateProductAsync(Guid id, CreateProductDto dto)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null) throw new KeyNotFoundException();

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.StockQuantity = dto.StockQuantity;
        product.CategoryId = dto.CategoryId;

        await _repository.UpdateAsync(product);

        await InvalidateProductCache(id);
    }

    public async Task DeleteProductAsync(Guid id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null) throw new KeyNotFoundException($"Product with ID {id} not found.");

        await _repository.DeleteAsync(id);

        await InvalidateProductCache(id);
    }

    public async Task<IEnumerable<ProductDto>> GetByCategoryIdAsync(Guid categoryId, bool isAdmin)
    {
        var products = await _repository.GetByCategoryIdAsync(categoryId, includeOutOfStock: isAdmin);
        return products.Select(p => MapToDto(p));
    }

    private async Task InvalidateProductCache(Guid id)
    {
        await _cache.KeyDeleteAsync($"{ProductKeyPrefix}{id}");
        await ClearGeneralCache();
    }

    private async Task ClearGeneralCache()
    {
        await _cache.KeyDeleteAsync($"{AllProductsKeyPrefix}true");
        await _cache.KeyDeleteAsync($"{AllProductsKeyPrefix}false");
    }

    private static ProductDto MapToDto(Product p) => new ProductDto(
        p.Id,
        p.Name,
        p.Description,
        p.Price,
        p.StockQuantity,
        p.CategoryId,
        p.Category?.Name ?? "N/A"
    );
}