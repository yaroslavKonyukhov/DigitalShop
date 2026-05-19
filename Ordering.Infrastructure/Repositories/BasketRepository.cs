using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Ordering.Domain.Entities;
using Ordering.Domain.Interfaces;
using StackExchange.Redis;

namespace Ordering.Infrastructure.Repositories;

public class BasketRepository : IBasketRepository
{
    private readonly IDatabase _database;

    public BasketRepository(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task<CustomerBasket?> GetBasketAsync(Guid userId)
    {
        var data = await _database.StringGetAsync($"basket:{userId}");

        if (data.IsNullOrEmpty) return null;

        return JsonSerializer.Deserialize<CustomerBasket>(data.ToString());
    }

    public async Task<CustomerBasket?> UpdateBasketAsync(CustomerBasket basket)
    {
        var jsonData = JsonSerializer.Serialize(basket);

        var created = await _database.StringSetAsync(
            $"basket:{basket.UserId}",
            jsonData,
            TimeSpan.FromDays(30));

        if (!created) return null;

        return await GetBasketAsync(basket.UserId);
    }

    public async Task<bool> DeleteBasketAsync(Guid userId)
    {
        return await _database.KeyDeleteAsync($"basket:{userId}");
    }
}
