using System;
using System.Collections.Generic;
using System.Text;

using Catalog.Application.DTOs;

namespace Catalog.Application.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    Task<CategoryDto?> GetCategoryByIdAsync(Guid id);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto);
    Task UpdateCategoryAsync(Guid id, CreateCategoryDto dto);
    Task DeleteCategoryAsync(Guid id);
}
