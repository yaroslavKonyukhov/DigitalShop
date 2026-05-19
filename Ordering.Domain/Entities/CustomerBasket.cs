using System;
using System.Collections.Generic;
using System.Text;

using System;
using System.Collections.Generic;

namespace Ordering.Domain.Entities;

public class CustomerBasket
{
    public Guid UserId { get; set; }
    public List<BasketItem> Items { get; set; } = new();

    public CustomerBasket() { }

    public CustomerBasket(Guid userId)
    {
        UserId = userId;
    }
}
