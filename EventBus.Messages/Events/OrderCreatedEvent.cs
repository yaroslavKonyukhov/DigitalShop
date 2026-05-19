using System;
using System.Collections.Generic;
using System.Text;

namespace EventBus.Messages.Events;

public interface IOrderCreatedEvent
{
    Guid OrderId { get; }
    Guid UserId { get; }
    List<OrderItemMessage> Items { get; }
}

public record OrderItemMessage(Guid ProductId, int Quantity);
