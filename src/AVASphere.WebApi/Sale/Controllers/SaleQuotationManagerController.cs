using Microsoft.AspNetCore.Mvc;
using AVASphere.ApplicationCore.Sales.Interfaces;
using AVASphere.ApplicationCore.Sales.Entities;
using AVASphere.ApplicationCore.Sales.DTOs;


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

    public SaleQuotationManagerController(ISaleQuotationService saleQuotationService)
    {
        _saleQuotationService = saleQuotationService;
    }
    // Marca una cotización como primaria para una venta.
    // - Desmarca la primaria anterior (si existe) y marca la nueva.
    // - No cambia la venta en sí (SourceQuotationVersionId), solo la marca en la relación N:N.

    [HttpPost("InsertAndSaleQuotation")]
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



    // Obtiene todas las relaciones (SaleQuotation) asociadas a una venta.
    [HttpGet("GetAllBySale")]
    public async Task<IActionResult> GetBySale([FromQuery] int IdSale)
    {
        var list = await _saleQuotationService.GetQuotationsForSaleAsync(IdSale);
        return Ok(list);
    }


    // Desvincula (elimina) la relación entre una venta y una cotización.
    // Uso: se expone cuando quieres permitir eliminar manualmente el link N:N.

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