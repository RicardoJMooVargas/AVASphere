using VYAACentralInforApi.Domain.Sales.Entities;

namespace VYAACentralInforApi.Application.Sales.Interfaces;

public interface ICustomerService
{
    Task<IEnumerable<Customer>> SearchCustomersByNameAsync(string name);
    Task<Customer> CreateCustomerAsync(Customer customer);
    Task<Customer> UpdateCustomerAsync(Customer customer);
}