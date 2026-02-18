using Microsoft.AspNetCore.Mvc;
using AVASphere.ApplicationCore.Sales.Interfaces;
using AVASphere.ApplicationCore.Sales.Entities;
using AVASphere.ApplicationCore.Sales.DTOs;
using AVASphere.ApplicationCore.Common.Entities.Jsons;


namespace AVASphere.WebApi.SaleQuotation.Controllers;


//Controlador que expone operaciones relacionadas con los enlaces entre Ventas (Sale) y Cotizaciones (Quotation).
//- GET: lista las cotizaciones vinculadas a una venta.
//- DELETE: desvincula una cotización de una venta.
//- POST mark-primary: marca una cotización como primaria para la venta (gestiona desmarcar la primaria anterior).

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "Sales")]
[Tags("Sales")]
public class SaleQuotationManagerController : ControllerBase
{
    private readonly ISaleQuotationService _saleQuotationService;
    private readonly ISaleService _saleService;

    public SaleQuotationManagerController(ISaleQuotationService saleQuotationService, ISaleService saleService)
    {
        _saleQuotationService = saleQuotationService;
        _saleService = saleService;
    }
    // Marca una cotización como primaria para una venta.
    // - Desmarca la primaria anterior (si existe) y marca la nueva.
    // - No cambia la venta en sí (SourceQuotationVersionId), solo la marca en la relación N:N.

    [HttpPost("MarkPrimary")]
    public async Task<IActionResult> MarkPrimary([FromBody] MarkPrimaryRequest req)
    {
        if (req is null)
            return BadRequest("Request cannot be null.");

        if (req.IdSale <= 0 || req.IdQuotation <= 0)
            return BadRequest("IdSale and IdQuotation must be greater than zero.");

        var userName = User?.Identity?.Name ?? "system";

        var success = await _saleQuotationService.MarkPrimaryQuotationAsync(
            req.IdSale,
            req.IdQuotation,
            userName
        );

        if (!success)
            return NotFound("Sale or quotation not found, or operation could not be completed.");

        return NoContent();
    }


    // Inserta una venta desde el sistema externo (InforAVA) y la vincula automáticamente con una cotización.
    [HttpPost("InsertAndSaleQuotationExternal")]
    public async Task<IActionResult> InsertAndSaleQuotationExternal([FromBody] InsertExternalSaleAndQuotationRequest req)
    {
        if (req is null)
            return BadRequest("Request cannot be null.");

        // Validar request básicamente
        if (string.IsNullOrWhiteSpace(req.Catalogo))
            return BadRequest("Catalogo is required.");

        if (string.IsNullOrWhiteSpace(req.Folio))
            return BadRequest("Folio is required.");

        if (req.IdQuotation <= 0)
            return BadRequest("IdQuotation must be greater than zero.");

        var userName = User?.Identity?.Name ?? "system";

        try
        {
            // Verificar si la venta ya existe
            var existingSale = await _saleService.GetSaleByFolioAsync(req.Folio);
            if (existingSale != null)
                return Conflict(new { message = "Sale with this folio already exists.", saleId = existingSale.IdSale });

            // Ejecutar operación transaccional
            var createdSale = await _saleService.InsertExternalSaleAndLinkQuotationAsync(req, userName);

            // Crear DTO de respuesta
            var responseDto = new SaleResponseDto
            {
                IdSale = createdSale.IdSale,
                Folio = createdSale.Folio,
                TotalAmount = createdSale.TotalAmount,
                SaleDate = createdSale.SaleDate,
                Type = createdSale.Type,
                LinkedQuotationCount = createdSale.HasLinkedQuotations ? createdSale.LinkedQuotations.Count : 0,
                ProductCount = createdSale.HasProducts ? createdSale.ProductsJson?.Count ?? 0 : 0
            };

            return StatusCode(StatusCodes.Status201Created, responseDto);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while processing the request.", detail = ex.Message });
        }
    }

    // Obtiene los detalles de una relación específica entre una venta y una cotización utilizando SaleQuotationDto.
    // Obtiene todas las relaciones (SaleQuotation) asociadas a una venta.
    [HttpGet("GetAllBySale")]
    public async Task<IActionResult> GetBySale([FromQuery] int IdSale)
    {
        if (IdSale <= 0)
            return BadRequest(new { message = "IdSale must be greater than 0." });

        var list = (await _saleQuotationService.GetQuotationsForSaleAsync(IdSale)).ToList();

        if (list.Count == 0)
            return NotFound(new { message = "No quotations found for this sale." });

        // Mapear a SaleQuotationDto
        var dtoList = list.Select(sq => new SaleQuotationDto
        {
            IdSaleQuotation = sq.IdSaleQuotation,
            IdQuotation = sq.IdQuotation,
            IdSale = sq.IdSale,
            CreatedAt = sq.CreatedAt,
            CreatedBy = sq.CreatedBy,
            IsPrimary = false, // Se puede calcular si es necesario
            ProductsJson = sq.ProductsJson ?? new List<SingleProductJson>(),
            PriceSnapshot = sq.PriceSnapshot,
            GeneralComment = sq.GeneralComment
        }).ToList();

        return Ok(dtoList);
    }

    // Obtiene los detalles de una relación específica entre una venta y una cotización.
    [HttpGet("GetRelationship")]
    public async Task<IActionResult> GetRelationship([FromQuery] int IdSale, [FromQuery] int IdQuotation)
    {
        if (IdSale <= 0 || IdQuotation <= 0)
            return BadRequest(new { message = "IdSale and IdQuotation must be greater than 0." });

        var list = (await _saleQuotationService.GetQuotationsForSaleAsync(IdSale)).ToList();
        var saleQuotation = list.FirstOrDefault(sq => sq.IdQuotation == IdQuotation);

        if (saleQuotation == null)
            return NotFound(new { message = "Relationship between sale and quotation not found." });

        var dto = new SaleQuotationDto
        {
            IdSaleQuotation = saleQuotation.IdSaleQuotation,
            IdQuotation = saleQuotation.IdQuotation,
            IdSale = saleQuotation.IdSale,
            CreatedAt = saleQuotation.CreatedAt,
            CreatedBy = saleQuotation.CreatedBy,
            IsPrimary = false,
            ProductsJson = saleQuotation.ProductsJson ?? new List<SingleProductJson>(),
            PriceSnapshot = saleQuotation.PriceSnapshot,
            GeneralComment = saleQuotation.GeneralComment
        };

        return Ok(dto);
    }

    // Gestiona relaciones complejas entre ventas y cotizaciones.
    // Permite eliminar, reasignar o limpiar relaciones de forma granular. 
    [HttpPut("ManageRelationship")]
    public async Task<IActionResult> ManageRelationship([FromBody] ManageSaleQuotationRelationshipRequest req)
    {
        if (req is null)
            return BadRequest(new { message = "Request cannot be null." });

        // Validar request
        if (req.IdSale <= 0)
            return BadRequest(new { message = "IdSale must be greater than 0." });

        if (req.IdQuotation <= 0)
            return BadRequest(new { message = "IdQuotation must be greater than 0." });

        if (string.IsNullOrWhiteSpace(req.Operation))
            return BadRequest(new { message = "Operation must be specified (DELETE, DELETE_WITH_SALE, REASSIGN)." });

        // Validar operación específica
        var validOperations = new[] { "DELETE", "DELETE_WITH_SALE", "REASSIGN" };
        if (!validOperations.Contains(req.Operation.ToUpper()))
            return BadRequest(new { message = $"Invalid operation. Must be one of: {string.Join(", ", validOperations)}" });

        // Validaciones específicas por operación
        if (req.Operation.ToUpper() == "DELETE_WITH_SALE" && !req.ConfirmDeletionWithSale)
            return BadRequest(new { message = "DELETE_WITH_SALE requires ConfirmDeletionWithSale = true to prevent accidental deletions." });

        if (req.Operation.ToUpper() == "REASSIGN")
        {
            if (!req.IdNewSale.HasValue || req.IdNewSale <= 0)
                return BadRequest(new { message = "REASSIGN operation requires IdNewSale to be specified and greater than 0." });

            if (req.IdNewSale == req.IdSale)
                return BadRequest(new { message = "IdNewSale cannot be the same as IdSale." });
        }

        var userName = User?.Identity?.Name ?? "system";

        try
        {
            var response = await _saleQuotationService.ManageRelationshipAsync(req, userName);

            return Ok(response);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            // Puede ser 404 o 409 según el contexto
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while managing the relationship.", detail = ex.Message });
        }
    }

    // Desvincula (elimina) la relación entre una venta y una cotización.
    // Uso: se expone cuando quieres permitir eliminar manualmente el link N:N.
    // ⚠️ DEPRECATED: Usar ManageRelationship con operation="DELETE" en su lugar.

    [HttpDelete("DeleteRelationship")]
    public async Task<IActionResult> Unlink([FromQuery] int IdSale, [FromQuery] int IdQuotation)
    {
        var ok = await _saleQuotationService.UnlinkQuotationFromSaleAsync(IdSale, IdQuotation);
        if (!ok) return NotFound();
        return NoContent();
    }
    // DTO interno para la petición de marcar primaria.

    public class MarkPrimaryRequest
    {
        public int IdSale { get; set; }
        public int IdQuotation { get; set; }
    }
}