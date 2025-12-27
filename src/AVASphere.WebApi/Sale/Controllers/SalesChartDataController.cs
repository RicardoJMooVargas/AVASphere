using AVASphere.ApplicationCore.Sales.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AVASphere.WebApi.Sale.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "Sales")]
[Tags("Sales")]
public class SalesChartDataController
{
    private readonly ISaleService _saleService;
    private readonly IExternalSalesService _externalSalesService;
    private readonly HttpClient _httpClient;

    private SalesChartDataController(
        ISaleService saleService,
        IExternalSalesService externalSalesService,
        HttpClient httpClient)
    {
        _saleService = saleService;
        _externalSalesService = externalSalesService ?? throw new ArgumentNullException(nameof(externalSalesService));
        _httpClient = httpClient;
    }
    
    
}