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

    /// <summary>
    /// Crea un nuevo cliente con auto-incremento automático de índices
    /// </summary>
    /// <remarks>
    /// Los índices en los JSON se asignan automáticamente de forma transparente:
    /// - Settings: Configuraciones del cliente (ruta, tipo, descuento)
    /// - Direction: Información de dirección (campo requerido si no se proporciona)
    /// - PaymentMethod: Método de pago preferido
    /// - PaymentTerms: Términos de pago del cliente
    /// 
    /// No es necesario especificar índices - se manejan internamente.
    /// </remarks>
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

    /// <summary>
    /// Actualiza un cliente existente con reemplazo completo de JSONs
    /// </summary>
    /// <remarks>
    /// Comportamiento de actualización:
    /// - Solo se actualizan los campos que no son null en el request
    /// - Los JSONs se reemplazan completamente si se proporcionan
    /// - Los índices se asignan automáticamente - transparente al usuario
    /// - Settings: Reemplaza completamente las configuraciones del cliente
    /// - Direction: Reemplaza completamente la información de dirección
    /// - PaymentMethod: Reemplaza completamente el método de pago
    /// - PaymentTerms: Reemplaza completamente los términos de pago
    /// </remarks>
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
}