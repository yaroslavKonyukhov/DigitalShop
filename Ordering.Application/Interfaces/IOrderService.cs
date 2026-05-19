using System;
using System.Collections.Generic;
using System.Text;

using Ordering.Application.DTOs;

namespace Ordering.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(Guid userId, CreateOrderDto createOrderDto);
    Task<IEnumerable<OrderDto>> GetUserOrdersAsync(Guid userId);
    Task<bool> CancelOrderAsync(Guid orderId);
}
