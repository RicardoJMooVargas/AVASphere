using AVASphere.ApplicationCore.Sales.DTOs.ChartDTOs;
using AVASphere.ApplicationCore.Sales.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AVASphere.WebApi.Sale.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "Sales")]
[Tags("Sales")]
public class SalesChartDataController : ControllerBase
{
    private readonly ISaleChartService _saleChartService;

    public SalesChartDataController(ISaleChartService saleChartService)
    {
        _saleChartService = saleChartService ?? throw new ArgumentNullException(nameof(saleChartService));
    }
    
    /// <summary>
    /// Obtiene estadísticas principales de ventas (TotalAmount + total de ventas)
    /// </summary>
    [HttpPost("sales-summary")]
    public async Task<IActionResult> GetSalesSummary([FromBody] SaleByCostChartFilter filter)
    {
        try
        {
            var result = await _saleChartService.GetSalesSummaryAsync(filter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener resumen de ventas", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene estadísticas de ventas por agente
    /// </summary>
    [HttpPost("sales-by-agent")]
    public async Task<IActionResult> GetSalesByAgent([FromBody] SalesByAgentChartFilter filter)
    {
        try
        {
            var result = await _saleChartService.GetSalesByAgentAsync(filter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener ventas por agente", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene estadísticas de ventas por producto
    /// </summary>
    [HttpPost("sales-by-product")]
    public async Task<IActionResult> GetSalesByProduct([FromBody] SalesByProductChartFilter filter)
    {
        try
        {
            var result = await _saleChartService.GetSalesByProductAsync(filter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener ventas por producto", error = ex.Message });
        }
    }
}
