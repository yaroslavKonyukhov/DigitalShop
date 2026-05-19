using EventBus.Messages.Events;
using MassTransit;
using Catalog.Domain.Interfaces;

namespace Catalog.Application.Consumers;

public class OrderCancelledConsumer : IConsumer<IOrderCancelledEvent>
{
    private readonly IProductRepository _productRepository;

    public OrderCancelledConsumer(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task Consume(ConsumeContext<IOrderCancelledEvent> context)
    {
        var message = context.Message;
        foreach (var item in message.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product != null)
            {
                product.StockQuantity += item.Quantity;
                await _productRepository.UpdateAsync(product);
            }
        }
    }
}