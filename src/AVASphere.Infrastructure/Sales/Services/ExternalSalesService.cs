using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AVASphere.ApplicationCore.Sales.DTOs;
using AVASphere.ApplicationCore.Sales.Interfaces;
using AVASphere.ApplicationCore.Sales.Entities;

namespace AVASphere.Infrastructure.Sales.Services;

/// <summary>
/// Servicio que combina datos de ventas externas (InforAVA) con información interna.
/// 
/// MOTIVO DE ESTA CLASE:
/// 1. LÓGICA DE NEGOCIO: Implementar la regla de combinar datos externos e internos.
/// 2. REUTILIZACIÓN: El controlador y otros servicios usan esta lógica sin duplicarla.
/// 3. TESTABILIDAD: Fácil de mockear para pruebas unitarias.
/// 4. MANTENIBILIDAD: Si cambia la lógica de combinación, se actualiza en un solo lugar.
/// </summary>
public class ExternalSalesService : IExternalSalesService
{
    private readonly IExternalSalesRepository _externalSalesRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleQuotationRepository _saleQuotationRepository;

    public ExternalSalesService(
        IExternalSalesRepository externalSalesRepository,
        ISaleRepository saleRepository,
        ISaleQuotationRepository saleQuotationRepository)
    {
        _externalSalesRepository = externalSalesRepository ?? throw new ArgumentNullException(nameof(externalSalesRepository));
        _saleRepository = saleRepository ?? throw new ArgumentNullException(nameof(saleRepository));
        _saleQuotationRepository = saleQuotationRepository ?? throw new ArgumentNullException(nameof(saleQuotationRepository));
    }

    /// <summary>
    /// Obtiene ventas externas, las combina con información interna y aplica filtros.
    /// 
    /// ALGORITMO:
    /// 1. Consultar API externa para la fecha/catálogo
    /// 2. Obtener todas las ventas internas del mismo período
    /// 3. Para cada venta externa, buscar su equivalente interno
    /// 4. Aplicar todos los filtros especificados
    /// 5. Retornar lista combinada y filtrada
    /// 
    /// FILTROS DISPONIBLES:
    /// - Search: Búsqueda inteligente (solo números → folio; texto → cliente)
    /// - CustomerName: Filtro por nombre del cliente
    /// - Folio: Filtro por folio
    /// - Auxiliar: Filtro por agente auxiliar
    /// - IsLinked: Filtrar por estado de vinculación
    /// - MinAmount/MaxAmount: Filtro por rango de montos
    /// - SatisfactionLevel: Filtro por nivel de satisfacción
    /// - Limit/Offset: Paginación
    /// 
    /// MOTIVO: Permitir búsquedas complejas sin perder performance.
    /// </summary>
    public async Task<IEnumerable<CombinedSalesDto>> GetExternalSalesWithInternalDataAsync(SaleFilterDto? filter = null)
    {
        try
        {
            // Inicializar filtro si es null
            filter ??= new SaleFilterDto();

            // Validar parámetros obligatorios
            if (string.IsNullOrWhiteSpace(filter.Catalogo))
                throw new ArgumentException("El catálogo es obligatorio.", nameof(filter.Catalogo));

            // Usar fecha actual si no se especifica
            var fecha = filter.Fecha ?? DateTime.UtcNow;

            //Consultar API externa
            var externalSales = await _externalSalesRepository.GetSalesByDateAndCatalogAsync(
                filter.Catalogo,
                fecha);

            if (!externalSales.Any())
            {
                return Array.Empty<CombinedSalesDto>();
            }

            // 2️⃣ Obtener ventas internas del mismo período (expandir rango para permitir desfaces)
            // MOTIVO: Las ventas podrían estar registradas con un día de diferencia
            var startDate = fecha.AddDays(-1);
            var endDate = fecha.AddDays(1);
            var internalSales = await _saleRepository.GetSalesByDateRangeAsync(startDate, endDate);

            // 3️⃣ Combinar datos
            var combinedList = new List<CombinedSalesDto>();

            foreach (var externalSale in externalSales)
            {
                // Buscar venta interna que coincida con la externa
                // CRITERIOS DE MATCH (en orden de especificidad):
                // 1. Folio exacto
                // 2. Cliente + Fecha cercana

                var linkedInternalSale = internalSales.FirstOrDefault(s =>
                    // Opción 1: Folio exacto
                    (s.Folio == externalSale.Folio) ||
                    // Opción 2: Cliente + Fecha cercana
                    (s.Customer?.Name == externalSale.NombreCliente &&
                     externalSale.Fecha != null &&
                     DateTime.TryParse(externalSale.Fecha, out var externalDate) &&
                     s.SaleDate.Date == externalDate.Date)
                );

                // 🔗 Verificar si hay relaciones en SaleQuotations
                int saleQuotationCount = 0;
                bool hasSaleQuotationLinks = false;

                if (linkedInternalSale != null)
                {
                    var saleQuotations = await _saleQuotationRepository.GetBySaleIdAsync(linkedInternalSale.IdSale);
                    saleQuotationCount = saleQuotations.Count();
                    hasSaleQuotationLinks = saleQuotationCount > 0;
                }

                var combined = new CombinedSalesDto
                {
                    ExternalSales = externalSale,
                    InternalSales = linkedInternalSale,
                    IsLinked = linkedInternalSale != null,
                    StatusMessage = linkedInternalSale != null
                        ? $"Venta vinculada con folio interno {linkedInternalSale.Folio}"
                        : "Venta externa no vinculada. Requiere importación o creación en el sistema.",

                    // 🔗 CAMPOS ADICIONALES INTERNOS (mapear solo si existe la venta interna)
                    SatisfactionLevel = linkedInternalSale?.SatisfactionLevel,
                    SatisfactionReason = linkedInternalSale?.SatisfactionReason,
                    Comment = linkedInternalSale?.Comment,
                    AfterSalesFollowupDate = linkedInternalSale?.AfterSalesFollowupDate,
                    LinkedQuotations = linkedInternalSale?.LinkedQuotations ?? new List<QuotationReference>(),

                    // 🔗 VERIFICACIÓN DE SALEQUOTATIONS
                    HasSaleQuotationLinks = hasSaleQuotationLinks,
                    SaleQuotationCount = saleQuotationCount
                };

                combinedList.Add(combined);
            }

            // 4️⃣ Aplicar filtros
            var filtered = ApplyFilters(combinedList, filter);

            return filtered;
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException(
                "No se pudieron obtener datos del sistema externo InforAVA.", ex);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Aplica todos los filtros especificados a la lista de ventas combinadas.
    /// 
    /// ORDEN DE FILTRADO:
    /// 1. Search (búsqueda inteligente)
    /// 2. Folio (búsqueda exacta)
    /// 3. CustomerName (búsqueda de cliente)
    /// 4. Auxiliar (búsqueda de agente)
    /// 5. IsLinked (por estado de vinculación)
    /// 6. Rango de montos (MinAmount/MaxAmount)
    /// 7. SatisfactionLevel (nivel de satisfacción)
    /// 8. Paginación (Limit/Offset)
    /// </summary>
    private IEnumerable<CombinedSalesDto> ApplyFilters(IEnumerable<CombinedSalesDto> sales, SaleFilterDto filter)
    {
        var result = sales.AsEnumerable();

        // FILTRO 1: Search (búsqueda inteligente)
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();

            // Detectar si es solo números
            bool isOnlyNumbers = search.All(char.IsDigit);

            if (isOnlyNumbers)
            {
                // 🔍 Búsqueda por folio (externo o interno)
                result = result.Where(s =>
                    (s.ExternalSales?.Folio?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (s.InternalSales?.Folio?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                );
            }
            else
            {
                // 🔍 Búsqueda por nombre del cliente
                result = result.Where(s =>
                    (s.ExternalSales?.NombreCliente?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (s.InternalSales?.Customer?.Name?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                );
            }
        }

        // FILTRO 2: Folio exacto (si Search no se usó)
        if (!string.IsNullOrWhiteSpace(filter.Folio) && string.IsNullOrWhiteSpace(filter.Search))
        {
            result = result.Where(s =>
                (s.ExternalSales?.Folio == filter.Folio) ||
                (s.InternalSales?.Folio == filter.Folio)
            );
        }

        // FILTRO 3: Nombre del cliente (si Search no se usó)
        if (!string.IsNullOrWhiteSpace(filter.CustomerName) && string.IsNullOrWhiteSpace(filter.Search))
        {
            result = result.Where(s =>
                (s.ExternalSales?.NombreCliente?.Contains(filter.CustomerName, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (s.InternalSales?.Customer?.Name?.Contains(filter.CustomerName, StringComparison.OrdinalIgnoreCase) ?? false)
            );
        }

        // FILTRO 4: Auxiliar (agente)
        if (!string.IsNullOrWhiteSpace(filter.Auxiliar))
        {
            result = result.Where(s =>
                (s.ExternalSales?.Agente?.Contains(filter.Auxiliar, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (s.InternalSales?.AuxNoteDataJson?.Agente?.Contains(filter.Auxiliar, StringComparison.OrdinalIgnoreCase) ?? false)
            );
        }

        // FILTRO 5: Estado de vinculación
        if (filter.IsLinked.HasValue)
        {
            result = result.Where(s => s.IsLinked == filter.IsLinked.Value);
        }

        // FILTRO 6: Rango de montos
        if (filter.MinAmount.HasValue)
        {
            result = result.Where(s => s.ExternalSales?.Total >= filter.MinAmount.Value);
        }

        if (filter.MaxAmount.HasValue)
        {
            result = result.Where(s => s.ExternalSales?.Total <= filter.MaxAmount.Value);
        }

        // FILTRO 7: Nivel de satisfacción (solo para ventas vinculadas)
        if (filter.SatisfactionLevel.HasValue)
        {
            result = result.Where(s =>
                s.IsLinked && s.SatisfactionLevel == filter.SatisfactionLevel.Value
            );
        }

        // FILTRO 8: Paginación
        if (filter.Offset.HasValue && filter.Offset.Value > 0)
        {
            result = result.Skip(filter.Offset.Value);
        }

        if (filter.Limit.HasValue && filter.Limit.Value > 0)
        {
            result = result.Take(filter.Limit.Value);
        }

        return result;
    }

    /// <summary>
    /// Verifica que el servicio externo está disponible.
    /// 
    /// MOTIVO: Implementar health checks y detectar si la API externa está caída.
    /// Permite que la interfaz de usuario muestre alertas apropiadas.
    /// </summary>
    public async Task<bool> VerifyExternalConnectionAsync()
    {
        try
        {
            return await _externalSalesRepository.IsExternalServiceAvailableAsync();
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Obtiene los detalles de productos/movimientos de una venta específica en el sistema externo.
    /// 
    /// PROCESO:
    /// 1. Delegar al repositorio para obtener detalles de la API externa
    /// 2. Retornar lista de ExternalSaleDetailDto
    /// 
    /// MOTIVO: Centralizar acceso a detalles de ventas del sistema externo.
    /// Permite visualizar cada producto vendido (cantidad, precio, descuentos, impuestos).
    /// </summary>
    /// <returns>Lista de detalles de productos en esa venta.</returns>
    public async Task<IEnumerable<ExternalSaleDetailDto>> GetSaleDetailAsync(string nf, string caja, string serie, string folio)
    {
        try
        {
            // Validar parámetros
            if (string.IsNullOrWhiteSpace(nf) || string.IsNullOrWhiteSpace(caja) ||
                string.IsNullOrWhiteSpace(serie) || string.IsNullOrWhiteSpace(folio))
                throw new ArgumentException("Los parámetros NF, Caja, Serie y Folio no pueden estar vacíos.");

            // Delegar al repositorio
            return await _externalSalesRepository.GetSaleDetailAsync(nf, caja, serie, folio);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Error al obtener los detalles de la venta del sistema externo.", ex);
        }
    }
}
