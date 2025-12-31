using AVASphere.ApplicationCore.Sales.DTOs.ChartDTOs;

namespace AVASphere.ApplicationCore.Sales.Interfaces;

public interface ISaleChartService
{
    Task<SalesSummaryResponse> GetSalesSummaryAsync(SaleByCostChartFilter filter);
    Task<SalesByAgentResponse> GetSalesByAgentAsync(SalesByAgentChartFilter filter);
    Task<SalesByProductResponse> GetSalesByProductAsync(SalesByProductChartFilter filter);
}