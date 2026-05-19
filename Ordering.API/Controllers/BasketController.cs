using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ordering.Domain.Entities;
using Ordering.Application.Interfaces;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Ordering.API.Controllers;

[ApiController]
[Route("api/ordering/basket")]
[Authorize]
public class BasketController : ControllerBase
{
    private readonly IBasketService _service;

    public BasketController(IBasketService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<CustomerBasket>> GetBasket()
    {
        // Используем универсальный поиск клейма
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            return Unauthorized("User ID (sub) not found in token");

        if (!Guid.TryParse(userIdClaim.Value, out var userId))
            return BadRequest("Invalid User ID format in token");

        return Ok(await _service.GetBasketAsync(userId));
    }

    [HttpPost]
    public async Task<ActionResult<CustomerBasket>> UpdateBasket(CustomerBasket basket)
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        basket.UserId = Guid.Parse(userIdClaim.Value);

        var result = await _service.UpdateBasketAsync(basket);
        return result != null ? Ok(result) : BadRequest("Could not update basket");
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteBasket()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        await _service.DeleteBasketAsync(Guid.Parse(userIdClaim.Value));
        return Ok();
    }
}