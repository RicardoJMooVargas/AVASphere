using AVASphere.ApplicationCore.Sales.Entities;

namespace AVASphere.ApplicationCore.Sales.Interfaces;

public interface ICustomerRepository
{
    Task<IEnumerable<Customer>> GetAllCustomersAsync();
    Task<Customer?> GetCustomerByIdAsync(string id);
    Task<Customer?> GetCustomerByEmailAsync(string email);
    Task<IEnumerable<Customer>> GetCustomersByNameAsync(string name);
    Task<Customer> CreateCustomerAsync(Customer customer);
    Task<Customer> UpdateCustomerAsync(Customer customer);
    Task<bool> DeleteCustomerAsync(string id);
    Task<bool> CustomerExistsAsync(string email);
    Task<long> GetTotalCustomersCountAsync();
}