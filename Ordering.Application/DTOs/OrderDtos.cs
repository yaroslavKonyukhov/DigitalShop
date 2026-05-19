using System;
using System.Collections.Generic;

namespace Ordering.Application.DTOs;

public record OrderItemDto(Guid ProductId, int Quantity, decimal UnitPrice);

public record OrderDto(
    Guid Id,
    DateTime OrderDate,
    decimal TotalPrice,
    string Status,
    List<OrderItemDto> Items);

public record CreateOrderDto(List<CreateOrderItemDto>? Items = null);

public record CreateOrderItemDto(Guid ProductId, int Quantity);