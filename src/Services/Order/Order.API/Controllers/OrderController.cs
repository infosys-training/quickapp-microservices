using Microsoft.AspNetCore.Mvc;
using Order.Domain.Entities;
using Order.Domain.Interfaces;
using Shared.Contracts.DTOs;

namespace Order.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderRepository _repository;
    private readonly ILogger<OrderController> _logger;

    public OrderController(IOrderRepository repository, ILogger<OrderController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll()
    {
        var orders = await _repository.GetAllAsync();
        return Ok(orders.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetById(int id)
    {
        var order = await _repository.GetByIdAsync(id);
        if (order == null)
            return NotFound();

        return Ok(MapToDto(order));
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderRequest request)
    {
        var entity = new OrderEntity
        {
            Discount = request.Discount,
            Comments = request.Comments,
            CashierId = request.CashierId,
            CustomerId = request.CustomerId,
            OrderDetails = request.OrderDetails.Select(d => new OrderDetailEntity
            {
                UnitPrice = d.UnitPrice,
                Quantity = d.Quantity,
                Discount = d.Discount,
                ProductId = d.ProductId
            }).ToList()
        };

        var created = await _repository.CreateAsync(entity);
        _logger.LogInformation("Order {OrderId} created for customer {CustomerId}", created.Id, created.CustomerId);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<OrderDto>> Update(int id, [FromBody] CreateOrderRequest request)
    {
        var entity = new OrderEntity
        {
            Discount = request.Discount,
            Comments = request.Comments,
            CashierId = request.CashierId,
            CustomerId = request.CustomerId,
            OrderDetails = request.OrderDetails.Select(d => new OrderDetailEntity
            {
                UnitPrice = d.UnitPrice,
                Quantity = d.Quantity,
                Discount = d.Discount,
                ProductId = d.ProductId
            }).ToList()
        };

        var updated = await _repository.UpdateAsync(id, entity);
        if (updated == null)
            return NotFound();

        return Ok(MapToDto(updated));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    private static OrderDto MapToDto(OrderEntity entity) => new(
        entity.Id,
        entity.Discount,
        entity.Comments,
        entity.CashierId,
        entity.CustomerId,
        entity.CreatedDate,
        entity.UpdatedDate,
        entity.OrderDetails.Select(d => new OrderDetailDto(
            d.Id, d.UnitPrice, d.Quantity, d.Discount, d.ProductId, d.OrderId
        )).ToList()
    );
}
