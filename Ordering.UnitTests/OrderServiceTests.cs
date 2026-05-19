using Moq;
using Xunit;
using Ordering.Application.Services;
using Ordering.Domain.Interfaces;
using Ordering.Application.ExternalServices;
using Ordering.Application.DTOs;
using Ordering.Domain.Entities;
using MassTransit;
using Ordering.Application.Interfaces;
using EventBus.Messages.Events;

namespace Ordering.UnitTests;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly Mock<IBasketRepository> _basketRepoMock;
    private readonly Mock<ICatalogClient> _catalogClientMock;
    private readonly Mock<IPublishEndpoint> _publishMock;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _orderRepoMock = new Mock<IOrderRepository>();
        _basketRepoMock = new Mock<IBasketRepository>();
        _catalogClientMock = new Mock<ICatalogClient>();
        _publishMock = new Mock<IPublishEndpoint>();

        _orderService = new OrderService(
            _orderRepoMock.Object,
            _basketRepoMock.Object,
            _catalogClientMock.Object,
            _publishMock.Object
        );
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldThrowException_WhenBasketIsEmpty()
    {
        var userId = Guid.NewGuid();
        _basketRepoMock.Setup(x => x.GetBasketAsync(userId)).ReturnsAsync((CustomerBasket)null!);

        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _orderService.CreateOrderAsync(userId, new CreateOrderDto()));

        Assert.Equal("Корзина пуста.", exception.Message);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldCreateOrder_WhenBasketIsValid()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var basket = new CustomerBasket
        {
            Items = new List<BasketItem> {
                new BasketItem { ProductId = productId, Quantity = 2, Price = 100 }
            }
        };

        var catalogProduct = new CatalogProductDto(productId, "Test Product", 100, 10);

        _basketRepoMock.Setup(x => x.GetBasketAsync(userId)).ReturnsAsync(basket);
        _catalogClientMock.Setup(x => x.GetProductAsync(productId)).ReturnsAsync(catalogProduct);

        var result = await _orderService.CreateOrderAsync(userId, new CreateOrderDto());

        Assert.NotNull(result);
        Assert.Equal(200, result.TotalPrice);
        Assert.Equal("Created", result.Status);

        _orderRepoMock.Verify(x => x.AddAsync(It.IsAny<Order>()), Times.Once);
        _publishMock.Verify(x => x.Publish<IOrderCreatedEvent>(It.IsAny<object>(), default), Times.Once);
        _basketRepoMock.Verify(x => x.DeleteBasketAsync(userId), Times.Once);
    }

    [Fact]
    public async Task CancelOrderAsync_ShouldReturnFalse_WhenOrderDoesNotExist()
    {
        var orderId = Guid.NewGuid();
        _orderRepoMock.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync((Order)null!);

        var result = await _orderService.CancelOrderAsync(orderId);

        Assert.False(result);
    }
}