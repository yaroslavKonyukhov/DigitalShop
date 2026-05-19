using System;
using System.Collections.Generic;
using System.Text;

using System;
using System.Threading.Tasks;
using Ordering.Application.DTOs;

namespace Ordering.Application.ExternalServices;

public interface ICatalogClient
{
    Task<CatalogProductDto?> GetProductAsync(Guid productId);
    Task<bool> UpdateStockAsync(Guid productId, int quantityChange);
}
