using VYAACentralInforApi.ApplicationCore.Sales.Entities;

namespace VYAACentralInforApi.ApplicationCore.Sales.Interfaces;

public interface ICustomerService
{
    Task<IEnumerable<Customer>> SearchCustomersByNameAsync(string name);
    Task<Customer> CreateCustomerAsync(Customer customer);
    Task<Customer> UpdateCustomerAsync(Customer customer);
}