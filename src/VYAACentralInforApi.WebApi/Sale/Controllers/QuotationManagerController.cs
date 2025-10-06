using Microsoft.AspNetCore.Mvc;
using VYAACentralInforApi.Application.Sales.Interfaces;
using VYAACentralInforApi.Domain.Sales.Entities;
using VYAACentralInforApi.WebApi.Sale.DTOs;
using MongoDB.Bson;

namespace VYAACentralInforApi.WebApi.Sale.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    public async Task<ActionResult<QuotationResponseDto>> CreateQuotation(
        [FromBody] CreateQuotationDto createQuotationDto,
        [FromHeader(Name = "UserId")] string createdByUserId)
    {
        try
        {
            if (string.IsNullOrEmpty(createdByUserId))
            {
                return BadRequest("El ID del usuario creador es requerido en el header 'UserId'.");
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