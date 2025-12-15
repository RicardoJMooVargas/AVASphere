using Microsoft.AspNetCore.Mvc;
using AVASphere.ApplicationCore.Sales.Interfaces;
using AVASphere.ApplicationCore.Sales.Entities;
using AVASphere.ApplicationCore.Sales.DTOs;
using AVASphere.ApplicationCore.Common.Entities.Jsons;

namespace AVASphere.WebApi.Sale.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "Sales")]
[Tags("Quotations")]
public class QuotationManagerController : ControllerBase
{
    private readonly IQuotationService _quotationService;

    public QuotationManagerController(IQuotationService quotationService)
    {
        _quotationService = quotationService;
    }
    // POST: api/QuotationManager
    [HttpPost("Register/Quotation")]
    public async Task<ActionResult> CreateQuotation(CreateQuotationDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var created = await _quotationService.CreateQuotationAsync(dto, User?.Identity?.Name ?? "system");
        return CreatedAtRoute(
            "GetQuotationById",
            new { id = created.IdQuotation },
            created
        );
    }

    // GET: api/QuotationManager
    [HttpGet("GetAll/Quotations")]
    public async Task<ActionResult> GetAll([FromQuery] QuotationFilterDto? filter = null)
    {
        var items = await _quotationService.GetQuotationsAsync(filter);
        return Ok(items);
    }

    [HttpPut("Update/{IdQuotation}")]
    public async Task<IActionResult> Update(int IdQuotation, QuotationUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var updated = await _quotationService.UpdateIdQuotation(IdQuotation, dto);
            if (updated == null)
                return NotFound("Quotation not found");

            return Ok(updated);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // DELETE: api/QuotationManager/Delete/IdQuotation
    [HttpDelete("Delete/IdQuotation")]
    public async Task<IActionResult> Delete(int IdQuotation)
    {
        try
        {
            var ok = await _quotationService.DeleteQuotationAsync(IdQuotation);
            return ok ? NoContent() : NotFound("Failed to delete quotation.");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            // Retorna 409 Conflict cuando está vinculada a una venta
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // DELETE: api/QuotationManager/Delete/IdFollowupsJson
    [HttpDelete("Delete/IdFollowupsJson")]
    public async Task<IActionResult> DeleteFollowup(int IdQuotation, int IdFollowupsJson)
    {
        try
        {
            var ok = await _quotationService.DeleteFollowupFromQuotationAsync(IdQuotation, IdFollowupsJson);
            return ok ? NoContent() : NotFound("Failed to delete followup.");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

}