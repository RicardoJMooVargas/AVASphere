using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.ApplicationCore.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace AVASphere.WebApi.Common.Controllers;

[ApiController]
[Route("api/common/[controller]")]
[ApiExplorerSettings(GroupName = nameof(SystemModule.Common))]
[Tags("Common - Customer")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }
    // Obtiene clientes con filtros opcionales
    [HttpGet("get")]
    public async Task<ActionResult> GetCustomers([FromQuery] int? idCustomer, [FromQuery] string? lastName, [FromQuery] int? externalId)
    {
        try
        {
            var filters = new CustomerFilterDto
            {
                IdCustomer = idCustomer,
                LastName = lastName,
                ExternalId = externalId
            };

            var customers = await _customerService.GetAsync(filters);
            
            return Ok(new ApiResponse(customers, "Customers retrieved successfully"));
        }
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    // Crea un nuevo cliente con auto-incremento automático de índices
    [HttpPost("create")]
    public async Task<ActionResult> CreateCustomer([FromBody] CustomerCreateRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse("Invalid model state", 400, ModelState));
            }

            var createdCustomer = await _customerService.NewAsync(request);
            
            return Ok(new ApiResponse(createdCustomer, "Customer created successfully", 201));
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    // Actualiza un cliente existente con reemplazo completo de JSONs
    [HttpPut("update")]
    public async Task<ActionResult> UpdateCustomer([FromBody] CustomerUpdateRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse("Invalid model state", 400, ModelState));
            }

            var updatedCustomer = await _customerService.EditAsync(request);
            
            return Ok(new ApiResponse(updatedCustomer, "Customer updated successfully"));
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse(ex.Message, 404));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    // Elimina un cliente por IdCustomer
    [HttpDelete("delete/{idCustomer}")]
    public async Task<ActionResult> DeleteCustomer(int idCustomer)
    {
        try
        {
            var deleted = await _customerService.DeleteAsync(idCustomer);
            
            if (!deleted)
            {
                return NotFound(new ApiResponse($"Customer with Id {idCustomer} not found", 404));
            }
            
            return Ok(new ApiResponse(true, "Customer deleted successfully"));
        }
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    
    // Búsqueda inteligente de clientes por nombre completo
    [HttpGet("search")]
    public async Task<ActionResult> SearchCustomers([FromQuery] string searchText)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return BadRequest(new ApiResponse("Search text cannot be empty", 400));
            }

            var customers = await _customerService.SearchAsync(searchText);
            
            return Ok(new ApiResponse(customers, "Customer search completed successfully"));
        }
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }
}