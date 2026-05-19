using Ordering.Application.DTOs;
using Ordering.Application.Interfaces;
using Ordering.Domain.Entities;
using Ordering.Domain.Interfaces;
using Ordering.Application.ExternalServices;
using EventBus.Messages.Events;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ordering.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly IBasketRepository _basketRepository;
    private readonly ICatalogClient _catalogClient;
    private readonly IPublishEndpoint _publishEndpoint;

    public OrderService(
        IOrderRepository repository,
        IBasketRepository basketRepository,
        ICatalogClient catalogClient,
        IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _basketRepository = basketRepository;
        _catalogClient = catalogClient;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<OrderDto> CreateOrderAsync(Guid userId, CreateOrderDto createOrderDto)
    {
        var basket = await _basketRepository.GetBasketAsync(userId);
        if (basket == null || !basket.Items.Any()) throw new Exception("Корзина пуста.");

        var orderItems = new List<OrderItem>();
        foreach (var basketItem in basket.Items)
        {
            var product = await _catalogClient.GetProductAsync(basketItem.ProductId);
            orderItems.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = basketItem.ProductId,
                Quantity = basketItem.Quantity,
                UnitPrice = product?.Price ?? basketItem.Price
            });
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Created,
            OrderItems = orderItems,
            TotalPrice = orderItems.Sum(x => x.UnitPrice * x.Quantity)
        };

        try
        {
            await _repository.AddAsync(order);

            await _publishEndpoint.Publish<IOrderCreatedEvent>(new
            {
                OrderId = order.Id,
                UserId = order.UserId,
                Items = order.OrderItems.Select(i => new OrderItemMessage(i.ProductId, i.Quantity)).ToList()
            });

            await _basketRepository.DeleteBasketAsync(userId);
            return MapToDto(order);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CRITICAL ERROR]: {ex.Message}");
            throw new Exception($"Ошибка при оформлении: {ex.Message}", ex);
        }
    }

    public async Task<bool> CancelOrderAsync(Guid orderId)
    {
        var order = await _repository.GetByIdAsync(orderId);
        if (order == null) return false;

        if (order.Status == OrderStatus.Cancelled) return true;

        order.Status = OrderStatus.Cancelled;
        await _repository.UpdateAsync(order);

        await _publishEndpoint.Publish<IOrderCancelledEvent>(new
        {
            OrderId = order.Id,
            Items = order.OrderItems.Select(i => new OrderItemMessage(i.ProductId, i.Quantity)).ToList()
        });

        return true;
    }

    public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(Guid userId)
    {
        var orders = await _repository.GetByUserIdAsync(userId);
        return orders.Select(MapToDto);
    }

    private static OrderDto MapToDto(Order order) => new OrderDto(
        order.Id,
        order.OrderDate,
        order.TotalPrice,
        order.Status.ToString(),
        order.OrderItems.Select(i => new OrderItemDto(
            i.ProductId,
            i.Quantity,
            i.UnitPrice
        )).ToList()
    );
}