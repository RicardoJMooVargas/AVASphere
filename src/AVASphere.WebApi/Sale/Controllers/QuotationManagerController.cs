using Microsoft.AspNetCore.Mvc;
using AVASphere.ApplicationCore.Sales.Interfaces;
using AVASphere.ApplicationCore.Sales.Entities;
using AVASphere.ApplicationCore.Sales.DTOs;
using MongoDB.Bson;

namespace AVASphere.WebApi.Sale.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "Sales")]
[Tags("Quotations")]
public class QuotationManagerController : ControllerBase
{
    private readonly IQuotationService _quotationService;
    private readonly ICustomerService _customerService;

    public QuotationManagerController(
        IQuotationService quotationService,
        ICustomerService customerService)
    {
        _quotationService = quotationService;
        _customerService = customerService;
    }

    [HttpPost]
    public async Task<ActionResult<QuotationResponseDto>> CreateQuotation([FromBody] CreateQuotationDto createQuotationDto, [FromHeader(Name = "UserId")] string createdByUserId)
    {
        try
        {
            if (string.IsNullOrEmpty(createdByUserId))
            {
                return BadRequest("El ID del usuario creador es requerido en el header 'UserId'.");
            }

            // Validar campos obligatorios de Customer
            if (string.IsNullOrWhiteSpace(createQuotationDto.Customer.FullName))
            {
                return BadRequest("El nombre completo del cliente es obligatorio.");
            }

            if (createQuotationDto.Customer.Phones == null || 
                !createQuotationDto.Customer.Phones.Any() || 
                createQuotationDto.Customer.Phones.All(p => string.IsNullOrWhiteSpace(p)))
            {
                return BadRequest("Al menos un número de teléfono del cliente es obligatorio.");
            }

            // Manejar el cliente (crear nuevo o actualizar existente)
            Customer customer;
            if (string.IsNullOrEmpty(createQuotationDto.Customer.CustomerId))
            {
                // Crear nuevo cliente
                customer = new Customer
                {
                    CustomerId = ObjectId.GenerateNewId().ToString(),
                    Code = createQuotationDto.Customer.Code,
                    FullName = createQuotationDto.Customer.FullName,
                    Email = createQuotationDto.Customer.Email,
                    Phones = createQuotationDto.Customer.Phones,
                    CreatedAt = DateTime.UtcNow,
                    Status = true
                };
                customer = await _customerService.CreateCustomerAsync(customer);
            }
            else
            {
                // Actualizar cliente existente
                customer = new Customer
                {
                    CustomerId = createQuotationDto.Customer.CustomerId,
                    Code = createQuotationDto.Customer.Code,
                    FullName = createQuotationDto.Customer.FullName,
                    Email = createQuotationDto.Customer.Email,
                    Phones = createQuotationDto.Customer.Phones,
                    Status = true
                };
                customer = await _customerService.UpdateCustomerAsync(customer);
            }

            // Crear la cotización
            var quotation = new Quotation
            {
                Folio = createQuotationDto.Folio,
                SaleDate = createQuotationDto.SaleDate ?? DateTime.UtcNow.Date,
                CustomerId = customer.CustomerId,
                GeneralComment = createQuotationDto.GeneralComment,
                Followups = createQuotationDto.Followups?.Select(f => new QuotationFollowups
                {
                    Comment = f.Comment,
                    Date = f.Date ?? DateTime.UtcNow,
                    UserId = f.UserId ?? createdByUserId
                }).ToList() ?? new List<QuotationFollowups>()
            };

            var createdQuotation = await _quotationService.CreateQuotationAsync(quotation, createdByUserId);

            // Mapear a DTO de respuesta
            var response = MapToQuotationResponseDto(createdQuotation);

            return CreatedAtAction(
                nameof(GetQuotationById),
                new { id = createdQuotation.QuotationId },
                response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<QuotationResponseDto>>> GetQuotations([FromQuery] GetQuotationsQueryDto query)
    {
        try
        {
            var quotations = await _quotationService.GetQuotationsAsync(
                query.StartDate, 
                query.EndDate, 
                query.CustomerName, 
                query.Folio);

            var response = quotations.Select(MapToQuotationResponseDto).ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<QuotationResponseDto>> GetQuotationById(string id)
    {
        try
        {
            var quotation = await _quotationService.GetQuotationByIdAsync(id);

            if (quotation == null)
            {
                return NotFound($"No se encontró la cotización con ID {id}.");
            }

            var response = MapToQuotationResponseDto(quotation);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

    [HttpPost("{quotationId}/followups")]
    public async Task<ActionResult<QuotationFollowupResponseDto>> AddFollowupToQuotation(
        string quotationId, 
        [FromBody] CreateFollowupDto createFollowupDto, 
        [FromHeader(Name = "UserId")] string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("El ID del usuario es requerido en el header 'UserId'.");
            }

            if (string.IsNullOrWhiteSpace(createFollowupDto.Comment))
            {
                return BadRequest("El comentario del seguimiento es obligatorio.");
            }

            var followup = new QuotationFollowups
            {
                Comment = createFollowupDto.Comment,
                Date = createFollowupDto.Date ?? DateTime.UtcNow
            };

            var createdFollowup = await _quotationService.AddFollowupToQuotationAsync(quotationId, followup, userId);

            var response = new QuotationFollowupResponseDto
            {
                Id = createdFollowup.Id,
                Date = createdFollowup.Date,
                Comment = createdFollowup.Comment,
                UserId = createdFollowup.UserId,
                CreatedAt = createdFollowup.CreatedAt
            };

            return CreatedAtAction(
                nameof(GetQuotationById),
                new { id = quotationId },
                response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

    [HttpPut("{quotationId}/followups/{followupId}")]
    public async Task<ActionResult<QuotationFollowupResponseDto>> UpdateFollowupInQuotation(
        string quotationId, 
        string followupId, 
        [FromBody] UpdateFollowupDto updateFollowupDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(updateFollowupDto.Comment))
            {
                return BadRequest("El comentario del seguimiento es obligatorio.");
            }

            var updatedFollowup = new QuotationFollowups
            {
                Comment = updateFollowupDto.Comment,
                Date = updateFollowupDto.Date ?? DateTime.UtcNow
            };

            var result = await _quotationService.UpdateFollowupInQuotationAsync(quotationId, followupId, updatedFollowup);

            var response = new QuotationFollowupResponseDto
            {
                Id = result.Id,
                Date = result.Date,
                Comment = result.Comment,
                UserId = result.UserId,
                CreatedAt = result.CreatedAt
            };

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

    [HttpDelete("{quotationId}/followups/{followupId}")]
    public async Task<ActionResult> DeleteFollowupFromQuotation(string quotationId, string followupId)
    {
        try
        {
            var deleted = await _quotationService.DeleteFollowupFromQuotationAsync(quotationId, followupId);

            if (!deleted)
            {
                return NotFound($"No se encontró el seguimiento con ID {followupId} en la cotización {quotationId}.");
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

    private static QuotationResponseDto MapToQuotationResponseDto(Quotation quotation)
    {
        return new QuotationResponseDto
        {
            QuotationId = quotation.QuotationId,
            SaleDate = quotation.SaleDate,
            Status = quotation.Status,
            SalesExecutives = quotation.SalesExecutives,
            Folio = quotation.Folio,
            CustomerId = quotation.CustomerId,
            Customer = quotation.Customer != null ? new CustomerResponseDto
            {
                CustomerId = quotation.Customer.CustomerId,
                Code = quotation.Customer.Code,
                FullName = quotation.Customer.FullName,
                Email = quotation.Customer.Email,
                Phones = quotation.Customer.Phones,
                CreatedAt = quotation.Customer.CreatedAt,
                Status = quotation.Customer.Status
            } : null,
            GeneralComment = quotation.GeneralComment,
            Followups = quotation.Followups.Select(f => new QuotationFollowupResponseDto
            {
                Id = f.Id,
                Date = f.Date,
                Comment = f.Comment,
                UserId = f.UserId,
                CreatedAt = f.CreatedAt
            }).ToList(),
            CreatedAt = quotation.CreatedAt,
            UpdatedAt = quotation.UpdatedAt
        };
    }
}