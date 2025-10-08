using Microsoft.AspNetCore.Mvc;
using VYAACentralInforApi.ApplicationCore.Sales.Interfaces;
using VYAACentralInforApi.ApplicationCore.Sales.DTOs;

namespace VYAACentralInforApi.WebApi.Sale.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "Sales")]
[Tags("Customers")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    /// <summary>
    /// Busca clientes por coincidencias en el nombre
    /// </summary>
    /// <param name="name">Término de búsqueda para el nombre del cliente</param>
    /// <returns>Lista de clientes que coinciden con el término de búsqueda</returns>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<CustomerResponseDto>>> SearchCustomersByName([FromQuery] string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("El parámetro de búsqueda 'name' es requerido.");
            }

            if (name.Length < 2)
            {
                return BadRequest("El término de búsqueda debe tener al menos 2 caracteres.");
            }

            var customers = await _customerService.SearchCustomersByNameAsync(name);

            var response = customers.Select(customer => new CustomerResponseDto
            {
                CustomerId = customer.CustomerId,
                Code = customer.Code ?? string.Empty,
                FullName = customer.FullName,
                Email = customer.Email,
                Phones = customer.Phones,
                CreatedAt = customer.CreatedAt,
                Status = customer.Status
            }).ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }
}