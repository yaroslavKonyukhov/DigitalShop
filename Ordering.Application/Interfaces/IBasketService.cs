using System;
using System.Collections.Generic;
using System.Text;

using Ordering.Domain.Entities;

namespace Ordering.Application.Interfaces;

public interface IBasketService
{
    Task<CustomerBasket> GetBasketAsync(Guid userId);
    Task<CustomerBasket?> UpdateBasketAsync(CustomerBasket basket);
    Task DeleteBasketAsync(Guid userId);
}
