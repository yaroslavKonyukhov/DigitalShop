using System;
using System.Collections.Generic;
using System.Text;

using System;

namespace Ordering.Domain.Entities;

public class BasketItem
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
