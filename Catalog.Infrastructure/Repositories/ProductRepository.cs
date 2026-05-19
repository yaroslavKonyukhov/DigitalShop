using System;
using System.Collections.Generic;
using System.Text;

using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _context;

    public ProductRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllAsync(bool includeOutOfStock = false)
    {
        var query = _context.Products.Include(p => p.Category).AsQueryable();

        if (!includeOutOfStock)
            query = query.Where(p => p.StockQuantity > 0);

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetByCategoryIdAsync(Guid categoryId, bool includeOutOfStock = false)
    {
        var query = _context.Products
            .Where(p => p.CategoryId == categoryId)
            .Include(p => p.Category)
            .AsQueryable();

        if (!includeOutOfStock)
            query = query.Where(p => p.StockQuantity > 0);

        return await query.ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(Guid id) =>
        await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);

    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateStockAsync(Guid productId, int quantityChange)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product != null)
        {
            product.StockQuantity += quantityChange;
            await _context.SaveChangesAsync();
        }
    }
}
