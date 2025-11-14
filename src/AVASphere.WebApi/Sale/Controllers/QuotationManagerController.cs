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
            new { id = created.QuotationId },
            created
        );
    }

    // GET: api/QuotationManager
    [HttpGet("GetAll/Quotations")]
    public async Task<ActionResult> GetAll([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] string? customerName = null, [FromQuery] int? folio = null)
    {
        var items = await _quotationService.GetQuotationsAsync(startDate, endDate, customerName, folio);
        return Ok(items);
    }

    // 🔹 GET: api/Quotation/GetById/{IdQuotation}
    [HttpGet("GetById{id:int}", Name = "GetQuotationById")]
    public async Task<IActionResult> GetById(int id)
    {
        var quotation = await _quotationService.GetByIdAsync(id);
        return Ok(quotation);
    }

    // GET: api/QuotationManager/folio/{folio}
    [HttpGet("Get/Folio")]
    public async Task<ActionResult> GetByFolio(int folio)
    {
        var items = await _quotationService.GetQuotationsAsync(null, null, null, folio);
        var first = items?.FirstOrDefault();
        if (first == null) return NotFound();
        return Ok(first);
    }

    // GET: api/QuotationManager/customer/{customerId}
    [HttpGet("Customer/IdCustomer")]
    public async Task<ActionResult> GetByCustomer(int IdCustomer)
    {
        var items = await _quotationService.GetQuotationsAsync(); // service currently lacks customerName->id filter
        var filtered = items.Where(q => q.CustomerId == IdCustomer);
        return Ok(filtered);
    }

    [HttpPut("Update/{IdQuotation}")]
    public async Task<IActionResult> Update(int IdQuotation, QuotationUpdateDto dto)
    {
        var updated = await _quotationService.UpdateIdQuotation(IdQuotation, dto);
        if (updated == null)
            return NotFound("Quotation not found");

        return Ok(updated);
    }

    // DELETE: api/QuotationManager/{id}
    [HttpDelete("Delete/IdQuotation")]
    public async Task<IActionResult> Delete(int IdQuotation)
    {
        var ok = await _quotationService.DeleteQuotationAsync(IdQuotation);
        if (!ok) return NotFound();
        return NoContent();
    }

    // POST: api/QuotationManager/{id}/followups
    [HttpPut("Register/FollowupsJson")]
    public async Task<IActionResult> AddFollowup(int IdQuotation, CreateFollowupDto dto)
    {
        var followup = new QuotationFollowupsJson
        {
            Id = 0,
            Date = dto.Date ?? DateTime.UtcNow,
            Comment = dto.Comment ?? string.Empty,
            UserId = User?.Identity?.Name ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        var ok = await _quotationService.AddFollowupAsync(IdQuotation, followup, followup.UserId);
        if (!ok) return NotFound();
        return NoContent();
    }

    // DELETE: api/QuotationManager/{id}/followups/{followupId}
    [HttpDelete("Delete/IdFollowupsJson")]
    public async Task<IActionResult> DeleteFollowup(int IdQuotation, int IdFollowupsJson)
    {
        var ok = await _quotationService.DeleteFollowupFromQuotationAsync(IdQuotation, IdFollowupsJson);
        if (!ok) return NotFound();
        return NoContent();
    }

}