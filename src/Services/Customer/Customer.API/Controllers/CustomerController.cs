using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.DTOs;

namespace Customer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ILogger<CustomerController> _logger;

    // In-memory seed data for local development and contract testing.
    // Production implementation would use CustomerDbContext via a repository.
    private static readonly List<CustomerDto> SeedCustomers =
    [
        new(Guid.Parse("c1d2e3f4-a5b6-4c7d-8e9f-0a1b2c3d4e5f"), "Alice Smith", "alice@example.com"),
        new(Guid.Parse("d2e3f4a5-b6c7-4d8e-9f0a-1b2c3d4e5f6a"), "Bob Johnson", "bob@example.com")
    ];

    public CustomerController(ILogger<CustomerController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(SeedCustomers);
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id)
    {
        var customer = SeedCustomers.FirstOrDefault(c => c.Id == id);
        if (customer is null)
            return NotFound();

        return Ok(customer);
    }
}
