using Customer.Domain.Entities;
using Customer.Domain.Interfaces;
using Shared.Contracts.DTOs;

namespace Customer.API.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;

    public CustomerService(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<CustomerDto>> GetAllAsync()
    {
        var customers = await _repository.GetAllAsync();
        return customers.Select(MapToDto).ToList();
    }

    public async Task<CustomerDto?> GetByIdAsync(int id)
    {
        var customer = await _repository.GetByIdAsync(id);
        return customer is null ? null : MapToDto(customer);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
    {
        var entity = new CustomerEntity
        {
            Name = dto.Name,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address,
            City = dto.City,
            Gender = Enum.TryParse<Gender>(dto.Gender, true, out var g) ? g : Gender.None
        };

        var created = await _repository.AddAsync(entity);
        return MapToDto(created);
    }

    public async Task<CustomerDto?> UpdateAsync(int id, UpdateCustomerDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return null;

        existing.Name = dto.Name;
        existing.Email = dto.Email;
        existing.PhoneNumber = dto.PhoneNumber;
        existing.Address = dto.Address;
        existing.City = dto.City;
        existing.Gender = Enum.TryParse<Gender>(dto.Gender, true, out var g) ? g : Gender.None;

        await _repository.UpdateAsync(existing);
        return MapToDto(existing);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return false;

        await _repository.DeleteAsync(id);
        return true;
    }

    private static CustomerDto MapToDto(CustomerEntity entity) => new(
        entity.Id,
        entity.Name,
        entity.Email,
        entity.PhoneNumber,
        entity.Address,
        entity.City,
        entity.Gender.ToString()
    );
}
