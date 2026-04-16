using AVASphere.ApplicationCore.Sales;
using AVASphere.ApplicationCore.Sales.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AVASphere.WebApi.Sale.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "Sales")]
[Tags("Quotations")]
public class QuotationChartDataController : ControllerBase
{
    private readonly IQuotationRepository _quotationRepository;

    public QuotationChartDataController(IQuotationRepository quotationRepository)
    {
        _quotationRepository = quotationRepository ?? throw new ArgumentNullException(nameof(quotationRepository));
    }

    /// <summary>
    /// Obtiene el conteo de cotizaciones por ejecutivo y estatus para un mes/año.
    /// Si no se envía SalesExecutive, devuelve todos los agentes encontrados en el periodo.
    /// </summary>
    [HttpGet("quotations-by-agent-status")]
    public async Task<IActionResult> GetQuotationsByAgentStatus([FromQuery] int? month = null, [FromQuery] int? year = null, [FromQuery] string? salesExecutive = null)
    {
        try
        {
            var now = DateTime.UtcNow;
            var selectedMonth = month ?? now.Month;
            var selectedYear = year ?? now.Year;

            if (selectedMonth is < 1 or > 12)
                return BadRequest("Month debe ser un valor entre 1 y 12.");

            if (selectedYear < 2000 || selectedYear > 3000)
                return BadRequest("Year debe ser un valor válido.");

            var startDate = new DateTime(selectedYear, selectedMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var quotations = (await _quotationRepository.GetQuotationsByDateRangeAsync(startDate, endDate)).ToList();

            var availableAgents = quotations
                .Where(q => q.SalesExecutives != null)
                .SelectMany(q => q.SalesExecutives)
                .Where(se => !string.IsNullOrWhiteSpace(se))
                .Select(se => se.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(se => se)
                .ToList();

            var flattened = quotations
                .Where(q => q.SalesExecutives != null)
                .SelectMany(q => q.SalesExecutives
                    .Where(se => !string.IsNullOrWhiteSpace(se))
                    .Select(se => se.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(se => new { Agent = se, q.Status }));

            if (!string.IsNullOrWhiteSpace(salesExecutive))
            {
                var filterValue = salesExecutive.Trim();
                flattened = flattened.Where(x => x.Agent.Equals(filterValue, StringComparison.OrdinalIgnoreCase));
            }

            var chartData = flattened
                .GroupBy(x => x.Agent, StringComparer.OrdinalIgnoreCase)
                .Select(g => new QuotationByAgentStatusItem
                {
                    SalesExecutive = g.Key,
                    Pending = g.Count(x => x.Status == StatusEnum.Pending),
                    Acepted = g.Count(x => x.Status == StatusEnum.acepted),
                    Rejected = g.Count(x => x.Status == StatusEnum.rejected)
                })
                .OrderBy(x => x.SalesExecutive)
                .ToList();

            foreach (var item in chartData)
            {
                item.Total = item.Pending + item.Acepted + item.Rejected;
            }

            return Ok(new QuotationByAgentStatusResponse
            {
                Month = selectedMonth,
                Year = selectedYear,
                AppliedSalesExecutive = string.IsNullOrWhiteSpace(salesExecutive) ? null : salesExecutive.Trim(),
                AvailableSalesExecutives = availableAgents,
                Items = chartData
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al generar gráfico de cotizaciones por agente", error = ex.Message });
        }
    }

    public class QuotationByAgentStatusResponse
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string? AppliedSalesExecutive { get; set; }
        public List<string> AvailableSalesExecutives { get; set; } = new();
        public List<QuotationByAgentStatusItem> Items { get; set; } = new();
    }

    public class QuotationByAgentStatusItem
    {
        public string SalesExecutive { get; set; } = string.Empty;
        public int Pending { get; set; }
        public int Acepted { get; set; }
        public int Rejected { get; set; }
        public int Total { get; set; }
    }
}
