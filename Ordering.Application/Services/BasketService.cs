using Ordering.Application.DTOs;
using Ordering.Application.ExternalServices;
using Ordering.Application.Interfaces;
using Ordering.Domain.Entities;
using Ordering.Domain.Interfaces;

namespace Ordering.Application.Services;

public class BasketService : IBasketService
{
    private readonly IBasketRepository _repository;
    private readonly ICatalogClient _catalogClient;

    public BasketService(IBasketRepository repository, ICatalogClient catalogClient)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _catalogClient = catalogClient ?? throw new ArgumentNullException(nameof(catalogClient));
    }

    public async Task<CustomerBasket> GetBasketAsync(Guid userId)
    {
        var basket = await _repository.GetBasketAsync(userId);

        if (basket == null)
        {
            return new CustomerBasket(userId);
        }

        await EnrichBasketItems(basket);

        return basket;
    }

    public async Task<CustomerBasket?> UpdateBasketAsync(CustomerBasket basket)
    {
        await EnrichBasketItems(basket);

        return await _repository.UpdateBasketAsync(basket);
    }

    public async Task DeleteBasketAsync(Guid userId)
    {
        await _repository.DeleteBasketAsync(userId);
    }

    private async Task EnrichBasketItems(CustomerBasket basket)
    {
        foreach (var item in basket.Items)
        {
            try
            {
                var product = await _catalogClient.GetProductAsync(item.ProductId);
                if (product != null)
                {
                    item.ProductName = product.Name;
                    item.Price = product.Price;
                }
                else
                {
                    item.ProductName = "Товар не найден";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BasketService Error]: Ошибка связи с каталогом для товара {item.ProductId}: {ex.Message}");
                item.ProductName = "Ошибка загрузки данных";
            }
        }
    }
}