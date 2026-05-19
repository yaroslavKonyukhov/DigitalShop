using System;
using System.Collections.Generic;
using System.Text;

namespace Ordering.Domain.Entities;

public enum OrderStatus { Created, Paid, Shipped, Cancelled }

public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalPrice { get; set; }
    public OrderStatus Status { get; set; }

    public List<OrderItem> OrderItems { get; set; } = new();
}
