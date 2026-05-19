using System;
using System.Collections.Generic;
using System.Text;

using Catalog.Domain.Entities;

namespace Catalog.Domain.Interfaces;
public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(Guid id);
    Task AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(Guid id);
}
