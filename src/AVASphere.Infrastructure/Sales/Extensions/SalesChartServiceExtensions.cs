// ServiceExtensions.cs (agregar en tu proyecto de infraestructura)
using Microsoft.Extensions.DependencyInjection;
using AVASphere.ApplicationCore.Sales.Interfaces;
using AVASphere.Infrastructure.Sales.Services;

namespace AVASphere.Infrastructure.Sales.Extensions;

public static class SalesChartServiceExtensions
{
    public static IServiceCollection AddSalesChartServices(this IServiceCollection services)
    {
        services.AddScoped<ISaleChartService, SaleChartService>();
        return services;
    }
}
