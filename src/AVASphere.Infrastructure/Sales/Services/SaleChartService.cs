// SaleChartService.cs
using Microsoft.EntityFrameworkCore;
using AVASphere.ApplicationCore.Sales.DTOs.ChartDTOs;
using AVASphere.ApplicationCore.Sales.Interfaces;
using AVASphere.ApplicationCore.Sales.Entities;
using AVASphere.Infrastructure;

namespace AVASphere.Infrastructure.Sales.Services
{
    public class SaleChartService : ISaleChartService
    {
        private readonly MasterDbContext _context;

        public SaleChartService(MasterDbContext context)
        {
            _context = context;
        }

        public async Task<SalesSummaryResponse> GetSalesSummaryAsync(SaleByCostChartFilter filter)
        {
            var query = BuildBaseQuery(filter);
            
            // Aplicar filtros avanzados
            if (!string.IsNullOrEmpty(filter.CustomerName))
            {
                query = query.Where(s => s.Customer != null && 
                    (s.Customer.Name != null && s.Customer.Name.Contains(filter.CustomerName) ||
                     s.AuxNoteDataJson != null && s.AuxNoteDataJson.NombreCliente.Contains(filter.CustomerName)));
            }

            if (!string.IsNullOrEmpty(filter.ProductName))
            {
                query = query.Where(s => s.ProductsJson != null && 
                    s.ProductsJson.Any(p => p.Description.Contains(filter.ProductName)));
            }

            var sales = await query.ToListAsync();
            
            var response = new SalesSummaryResponse
            {
                TotalAmount = sales.Sum(s => s.TotalAmount),
                TotalSalesCount = sales.Count,
                Metadata = BuildMetadata(filter)
            };

        // Agrupar según el tipo de filtro
        switch (filter.Type)
        {
            case AVASphere.ApplicationCore.Sales.TypeFilter.daily:
                    response.Details = sales
                        .GroupBy(s => s.SaleDate.Date)
                        .Select(g => new SalesSummaryDetail
                        {
                            Period = g.Key.ToString("yyyy-MM-dd"),
                            Amount = g.Sum(s => s.TotalAmount),
                            SalesCount = g.Count()
                        }).ToList();
                    break;

                case AVASphere.ApplicationCore.Sales.TypeFilter.monthly:
                    response.Details = sales
                        .GroupBy(s => new { s.SaleDate.Year, s.SaleDate.Month })
                        .Select(g => new SalesSummaryDetail
                        {
                            Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                            Amount = g.Sum(s => s.TotalAmount),
                            SalesCount = g.Count()
                        }).ToList();
                    break;

                case AVASphere.ApplicationCore.Sales.TypeFilter.personalized:
                    response.Details = sales
                        .GroupBy(s => s.SaleDate.Date)
                        .Select(g => new SalesSummaryDetail
                        {
                            Period = g.Key.ToString("yyyy-MM-dd"),
                            Amount = g.Sum(s => s.TotalAmount),
                            SalesCount = g.Count()
                        }).ToList();
                    break;
            }

            return response;
        }

        public async Task<SalesByAgentResponse> GetSalesByAgentAsync(SalesByAgentChartFilter filter)
        {
            var query = BuildBaseQuery(filter);
            
            // Aplicar filtro de agente si se especifica
            if (!string.IsNullOrEmpty(filter.SalesAgent))
            {
                query = query.Where(s => s.SalesExecutive == filter.SalesAgent || 
                    (s.AuxNoteDataJson != null && s.AuxNoteDataJson.Agente == filter.SalesAgent));
            }

            // Aplicar filtro de cliente si se especifica
            if (!string.IsNullOrEmpty(filter.CustomerName))
            {
                query = query.Where(s => s.Customer != null && 
                    (s.Customer.Name != null && s.Customer.Name.Contains(filter.CustomerName) ||
                     s.AuxNoteDataJson != null && s.AuxNoteDataJson.NombreCliente.Contains(filter.CustomerName)));
            }

            var sales = await query.Include(s => s.Customer).ToListAsync();
            
            var response = new SalesByAgentResponse
            {
                TotalAmount = sales.Sum(s => s.TotalAmount),
                Metadata = BuildMetadata(filter)
            };

            // Agrupar por agente
            var agentGroups = sales
                .GroupBy(s => GetAgentName(s, filter.SalesAgent))
                .Where(g => !string.IsNullOrEmpty(g.Key));

            foreach (var agentGroup in agentGroups)
            {
                var agentDetail = new AgentSalesDetail
                {
                    AgentName = agentGroup.Key,
                    TotalAmount = agentGroup.Sum(s => s.TotalAmount),
                    SalesCount = agentGroup.Count()
                };

                // Si hay filtro de cliente, agregar detalles por cliente
                if (!string.IsNullOrEmpty(filter.CustomerName))
                {
                    agentDetail.CustomerDetails = agentGroup
                        .GroupBy(s => GetCustomerName(s))
                        .Select(g => new CustomerSalesDetail
                        {
                            CustomerName = g.Key,
                            Amount = g.Sum(s => s.TotalAmount),
                            SalesCount = g.Count()
                        }).ToList();
                }

                response.Agents.Add(agentDetail);
            }

            return response;
        }

        public async Task<SalesByProductResponse> GetSalesByProductAsync(SalesByProductChartFilter filter)
        {
            var query = BuildBaseQuery(filter);
            
            var sales = await query.ToListAsync();
            
            var response = new SalesByProductResponse
            {
                Metadata = BuildMetadata(filter)
            };

            // Extraer todos los productos de las ventas
            var allProducts = new Dictionary<string, ProductSalesDetail>();
            var frequencyDict = new Dictionary<string, Dictionary<string, int>>();

            foreach (var sale in sales)
            {
                if (sale.ProductsJson == null) continue;

                foreach (var product in sale.ProductsJson)
                {
                    if (!string.IsNullOrEmpty(filter.ProductName) && 
                        !product.Description.Contains(filter.ProductName))
                        continue;

                    var productKey = product.Description;
                    var periodKey = GetPeriodKey(sale.SaleDate, filter.Type);

                    // Actualizar frecuencia
                    if (!frequencyDict.ContainsKey(periodKey))
                        frequencyDict[periodKey] = new Dictionary<string, int>();
                    
                    if (!frequencyDict[periodKey].ContainsKey(productKey))
                        frequencyDict[periodKey][productKey] = 0;
                    
                    frequencyDict[periodKey][productKey]++;

                    // Actualizar detalles del producto
                    if (!allProducts.ContainsKey(productKey))
                    {
                        allProducts[productKey] = new ProductSalesDetail
                        {
                            ProductName = productKey,
                            TotalQuantity = 0,
                            TotalAmount = 0,
                            SalesCount = 0,
                            PeriodQuantities = new List<PeriodQuantity>()
                        };
                    }

                    var productDetail = allProducts[productKey];
                    productDetail.TotalQuantity += product.Quantity;
                    productDetail.TotalAmount += product.TotalPrice;
                    productDetail.SalesCount++;

                    // Actualizar cantidad por período
                    var periodQuantity = productDetail.PeriodQuantities
                        .FirstOrDefault(pq => pq.Period == periodKey);
                    
                    if (periodQuantity == null)
                    {
                        productDetail.PeriodQuantities.Add(new PeriodQuantity
                        {
                            Period = periodKey,
                            Quantity = product.Quantity,
                            Amount = product.TotalPrice
                        });
                    }
                    else
                    {
                        periodQuantity.Quantity += product.Quantity;
                        periodQuantity.Amount += product.TotalPrice;
                    }
                }
            }

            response.Products = allProducts.Values.ToList();

            // Calcular frecuencias más altas
            response.TopFrequencies = frequencyDict
                .SelectMany(period => period.Value
                    .Select(product => new SalesFrequency
                    {
                        Period = period.Key,
                        ProductName = product.Key,
                        Frequency = product.Value
                    }))
                .OrderByDescending(f => f.Frequency)
                .Take(10)
                .ToList();

            return response;
        }

        private IQueryable<Sale> BuildBaseQuery(SaleByCostChartFilter filter)
        {
            IQueryable<Sale> query = _context.Set<Sale>()
                .Include(s => s.Customer)
                .AsQueryable();

            return ApplyDateFilterForCost(query, filter.Type, filter);
        }

        private IQueryable<Sale> BuildBaseQuery(SalesByAgentChartFilter filter)
        {
            IQueryable<Sale> query = _context.Set<Sale>()
                .Include(s => s.Customer)
                .AsQueryable();

            return ApplyDateFilterForAgent(query, filter.Type, filter);
        }

        private IQueryable<Sale> BuildBaseQuery(SalesByProductChartFilter filter)
        {
            IQueryable<Sale> query = _context.Set<Sale>()
                .AsQueryable();

            return ApplyDateFilterForProduct(query, filter.Type, filter);
        }

        private IQueryable<Sale> ApplyDateFilterForCost(IQueryable<Sale> query, AVASphere.ApplicationCore.Sales.TypeFilter type, SaleByCostChartFilter filter)
        {
            switch (type)
            {
                case AVASphere.ApplicationCore.Sales.TypeFilter.daily:
                    if (filter.SpecificDate.HasValue)
                    {
                        var date = filter.SpecificDate.Value.Date;
                        query = query.Where(s => s.SaleDate.Date == date);
                    }
                    break;

                case AVASphere.ApplicationCore.Sales.TypeFilter.monthly:
                    if (filter.Year.HasValue && filter.Month.HasValue)
                    {
                        query = query.Where(s => 
                            s.SaleDate.Year == filter.Year.Value && 
                            s.SaleDate.Month == filter.Month.Value);
                    }
                    break;

                case AVASphere.ApplicationCore.Sales.TypeFilter.personalized:
                    if (filter.StartDate.HasValue && filter.EndDate.HasValue)
                    {
                        query = query.Where(s => 
                            s.SaleDate.Date >= filter.StartDate.Value.Date && 
                            s.SaleDate.Date <= filter.EndDate.Value.Date);
                    }
                    break;
            }
            return query;
        }

        private IQueryable<Sale> ApplyDateFilterForAgent(IQueryable<Sale> query, AVASphere.ApplicationCore.Sales.TypeFilter type, SalesByAgentChartFilter filter)
        {
            switch (type)
            {
                case AVASphere.ApplicationCore.Sales.TypeFilter.daily:
                    if (filter.SpecificDate.HasValue)
                    {
                        var date = filter.SpecificDate.Value.Date;
                        query = query.Where(s => s.SaleDate.Date == date);
                    }
                    break;

                case AVASphere.ApplicationCore.Sales.TypeFilter.monthly:
                    if (filter.Year.HasValue && filter.Month.HasValue)
                    {
                        query = query.Where(s => 
                            s.SaleDate.Year == filter.Year.Value && 
                            s.SaleDate.Month == filter.Month.Value);
                    }
                    break;

                case AVASphere.ApplicationCore.Sales.TypeFilter.personalized:
                    if (filter.StartDate.HasValue && filter.EndDate.HasValue)
                    {
                        query = query.Where(s => 
                            s.SaleDate.Date >= filter.StartDate.Value.Date && 
                            s.SaleDate.Date <= filter.EndDate.Value.Date);
                    }
                    break;
            }
            return query;
        }

        private IQueryable<Sale> ApplyDateFilterForProduct(IQueryable<Sale> query, AVASphere.ApplicationCore.Sales.TypeFilter type, SalesByProductChartFilter filter)
        {
            switch (type)
            {
                case AVASphere.ApplicationCore.Sales.TypeFilter.daily:
                    if (filter.SpecificDate.HasValue)
                    {
                        var date = filter.SpecificDate.Value.Date;
                        query = query.Where(s => s.SaleDate.Date == date);
                    }
                    break;

                case AVASphere.ApplicationCore.Sales.TypeFilter.monthly:
                    if (filter.Year.HasValue && filter.Month.HasValue)
                    {
                        query = query.Where(s => 
                            s.SaleDate.Year == filter.Year.Value && 
                            s.SaleDate.Month == filter.Month.Value);
                    }
                    break;

                case AVASphere.ApplicationCore.Sales.TypeFilter.personalized:
                    if (filter.StartDate.HasValue && filter.EndDate.HasValue)
                    {
                        query = query.Where(s => 
                            s.SaleDate.Date >= filter.StartDate.Value.Date && 
                            s.SaleDate.Date <= filter.EndDate.Value.Date);
                    }
                    break;
            }

            return query;
        }

        private string GetAgentName(Sale sale, string? requestedAgent)
        {
            if (!string.IsNullOrEmpty(requestedAgent))
                return requestedAgent;

            return !string.IsNullOrEmpty(sale.SalesExecutive) 
                ? sale.SalesExecutive 
                : sale.AuxNoteDataJson?.Agente ?? "Sin agente";
        }

        private string GetCustomerName(Sale sale)
        {
            return sale.Customer?.Name ?? 
                   sale.AuxNoteDataJson?.NombreCliente ?? 
                   "Cliente desconocido";
        }

        private string GetPeriodKey(DateTime date, AVASphere.ApplicationCore.Sales.TypeFilter type)
        {
            return type switch
            {
                AVASphere.ApplicationCore.Sales.TypeFilter.daily => date.ToString("yyyy-MM-dd"),
                AVASphere.ApplicationCore.Sales.TypeFilter.monthly => $"{date.Year}-{date.Month:D2}",
                AVASphere.ApplicationCore.Sales.TypeFilter.personalized => date.ToString("yyyy-MM-dd"),
                _ => date.ToString("yyyy-MM-dd")
            };
        }

        private ChartMetadata BuildMetadata(SaleByCostChartFilter filter)
        {
            return new ChartMetadata
            {
                Type = filter.Type,
                Year = filter.Year,
                Month = filter.Month,
                SpecificDate = filter.SpecificDate,
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                AppliedFilters = GetAppliedFilters(filter)
            };
        }

        private ChartMetadata BuildMetadata(SalesByAgentChartFilter filter)
        {
            return new ChartMetadata
            {
                Type = filter.Type,
                Year = filter.Year,
                Month = filter.Month,
                SpecificDate = filter.SpecificDate,
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                AppliedFilters = GetAppliedFilters(filter)
            };
        }

        private ChartMetadata BuildMetadata(SalesByProductChartFilter filter)
        {
            return new ChartMetadata
            {
                Type = filter.Type,
                Year = filter.Year,
                Month = filter.Month,
                SpecificDate = filter.SpecificDate,
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                AppliedFilters = GetAppliedFilters(filter)
            };
        }

        private List<string> GetAppliedFilters(SaleByCostChartFilter filter)
        {
            var filters = new List<string>();
            if (!string.IsNullOrEmpty(filter.CustomerName))
                filters.Add($"Cliente: {filter.CustomerName}");
            if (!string.IsNullOrEmpty(filter.ProductName))
                filters.Add($"Producto: {filter.ProductName}");
            return filters;
        }

        private List<string> GetAppliedFilters(SalesByAgentChartFilter filter)
        {
            var filters = new List<string>();
            if (!string.IsNullOrEmpty(filter.SalesAgent))
                filters.Add($"Agente: {filter.SalesAgent}");
            if (!string.IsNullOrEmpty(filter.CustomerName))
                filters.Add($"Cliente: {filter.CustomerName}");
            return filters;
        }

        private List<string> GetAppliedFilters(SalesByProductChartFilter filter)
        {
            var filters = new List<string>();
            if (!string.IsNullOrEmpty(filter.ProductName))
                filters.Add($"Producto: {filter.ProductName}");
            return filters;
        }
    }
}