using Microsoft.AspNetCore.Mvc;
using AVASphere.ApplicationCore.Sales.Interfaces;
using AVASphere.ApplicationCore.Sales.Entities;
using AVASphere.ApplicationCore.Sales.DTOs;

namespace AVASphere.WebApi.Sale.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "Sales")]
    [Tags("Sales")]
    public class SaleManagerController : ControllerBase
    {
        private readonly ISaleService _saleService;
        public SaleManagerController(ISaleService saleService)
        {
            _saleService = saleService;
        }

        // POST: api/Sale
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] global::AVASphere.ApplicationCore.Sales.Entities.Sale sale)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _saleService.CreateSaleAsync(sale, User?.Identity?.Name ?? "system");
            return CreatedAtAction(nameof(GetById), new { id = created.SaleId }, created);
        }

        // GET: api/Sale/{id}
        [HttpGet("GetByIdSale")]
        public async Task<ActionResult> GetById(int IdSale)
        {
            var sale = await _saleService.GetSaleByIdAsync(IdSale);
            if (sale == null) return NotFound();
            return Ok(sale);
        }

        // GET: api/Sale/folio/{folio}
        [HttpGet("GetByfolio")]
        public async Task<ActionResult> GetByFolio(string folio)
        {
            var sale = await _saleService.GetSaleByFolioAsync(folio);
            if (sale == null) return NotFound();
            return Ok(sale);
        }

        // DELETE: api/Sale/{id}
        [HttpDelete("DeleteIdSale")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _saleService.DeleteSaleAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        // POST: api/Sale/from-quotations
        [HttpPost("CreateFromQuotations")]
        public async Task<IActionResult> CreateFromQuotations([FromBody] CreateSaleFromQuotationsDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var sale = new global::AVASphere.ApplicationCore.Sales.Entities.Sale
            {
                SalesExecutive = dto.SalesExecutive,
                Date = dto.Date,
                Type = dto.Type,
                CustomerId = dto.CustomerId,
                Folio = dto.Folio,
                TotalAmount = dto.TotalAmount,
                DeliveryDriver = dto.DeliveryDriver,
                HomeDelivery = dto.HomeDelivery,
                DeliveryDate = dto.DeliveryDate,
                SatisfactionLevel = dto.SatisfactionLevel,
                SatisfactionReason = dto.SatisfactionReason,
                Comment = dto.Comment,
                AfterSalesFollowupDate = dto.AfterSalesFollowupDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IdConfigSys = dto.IdConfigSys
            };

            var created = await _saleService.CreateSaleFromQuotationsAsync(dto.QuotationIds, sale, User?.Identity?.Name ?? "system");
            return CreatedAtAction(nameof(GetById), new { id = created.SaleId }, created);
        }


    }
}