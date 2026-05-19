using System;
using System.Collections.Generic;
using System.Text;

using Catalog.Domain.Entities;

namespace Catalog.Domain.Interfaces;
public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync(bool includeOutOfStock = false);
    Task<IEnumerable<Product>> GetByCategoryIdAsync(Guid categoryId, bool includeOutOfStock = false);

    Task<Product?> GetByIdAsync(Guid id);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Guid id);

    Task UpdateStockAsync(Guid productId, int quantityChange);
}
