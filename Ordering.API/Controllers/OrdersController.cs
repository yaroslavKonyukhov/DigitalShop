using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ordering.Application.DTOs;
using Ordering.Application.Interfaces;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

[ApiController]
[Route("api/ordering/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto createOrderDto)
    {
        var userIdString = GetUserId();

        if (string.IsNullOrEmpty(userIdString))
            return Unauthorized("User ID not found in token");

        try
        {
            var userId = Guid.Parse(userIdString);
            var result = await _orderService.CreateOrderAsync(userId, createOrderDto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetUserOrders()
    {
        var userIdString = GetUserId(); // Используем тот же надежный метод

        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var userId = Guid.Parse(userIdString);
        var orders = await _orderService.GetUserOrdersAsync(userId);
        return Ok(orders);
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        try
        {
            var result = await _orderService.CancelOrderAsync(id);
            return result ? Ok(new { Message = "Заказ успешно отменен, товары возвращены на склад" })
                          : NotFound("Заказ не найден");
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    // Вынес логику поиска ID в отдельный приватный метод для чистоты
    private string? GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("sub")
               ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
    }
}