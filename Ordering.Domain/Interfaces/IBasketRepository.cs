using System;
using System.Collections.Generic;
using System.Text;

using Ordering.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Ordering.Domain.Interfaces;

public interface IBasketRepository
{
    Task<CustomerBasket?> GetBasketAsync(Guid userId);
    Task<CustomerBasket?> UpdateBasketAsync(CustomerBasket basket);
    Task<bool> DeleteBasketAsync(Guid userId);
}
