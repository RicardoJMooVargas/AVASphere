using VYAACentralInforApi.ApplicationCore.Sales.Entities;
using VYAACentralInforApi.ApplicationCore.Sales.Interfaces;

namespace VYAACentralInforApi.ApplicationCore.Sales.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<IEnumerable<Customer>> SearchCustomersByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Enumerable.Empty<Customer>();
        return await _customerRepository.GetCustomersByNameAsync(name);
    }

    public async Task<Customer> CreateCustomerAsync(Customer customer)
    {
        // Aquí podrías agregar validaciones adicionales si es necesario
        return await _customerRepository.CreateCustomerAsync(customer);
    }

    public async Task<Customer> UpdateCustomerAsync(Customer customer)
    {
        // Aquí podrías agregar validaciones adicionales si es necesario
        return await _customerRepository.UpdateCustomerAsync(customer);
    }
}

