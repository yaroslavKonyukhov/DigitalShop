using System;
using System.Collections.Generic;
using System.Text;

namespace EventBus.Messages.Events;

public interface IOrderCancelledEvent
{
    Guid OrderId { get; }
    List<OrderItemMessage> Items { get; }
}
