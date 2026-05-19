using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Application.DTOs;
public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    Guid CategoryId,
    string CategoryName
);

public record CreateProductDto(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    Guid CategoryId
);
