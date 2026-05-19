using Catalog.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Application.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetProductsAsync(bool isAdmin);
    Task<IEnumerable<ProductDto>> GetByCategoryIdAsync(Guid categoryId, bool isAdmin);
    Task<ProductDto?> GetProductByIdAsync(Guid id);
    Task<ProductDto> CreateProductAsync(CreateProductDto dto);
    Task UpdateStockAsync(Guid id, int newQuantity);
    Task UpdateProductAsync(Guid id, CreateProductDto dto);
    Task DeleteProductAsync(Guid id);
}
