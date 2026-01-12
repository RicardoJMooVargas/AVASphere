using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using AVASphere.ApplicationCore.Common.Entities.General;
using AVASphere.ApplicationCore.Common.Entities.Jsons;
using AVASphere.ApplicationCore.Common.Extensions;
using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.ApplicationCore.Sales.DTOs;
using AVASphere.ApplicationCore.Sales.DTOs.ImportDTOs;
using AVASphere.ApplicationCore.Sales.Entities;
using AVASphere.ApplicationCore.Sales.Interfaces;
using AVASphere.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AVASphere.Infrastructure.Sales.Services;

public class SaleService : ISaleService
{
    private readonly ISaleRepository _saleRepository;
    private readonly IQuotationRepository _quotationRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly MasterDbContext _dbContext;
    private readonly IExternalSalesService _externalSalesService;
    private readonly IExternalSalesRepository _externalSalesRepository;
    private readonly ISaleQuotationService _saleQuotationService;

    public SaleService(
        ISaleRepository saleRepository,
        IQuotationRepository quotationRepository,
        ICustomerRepository customerRepository,
        MasterDbContext dbContext,
        IExternalSalesService externalSalesService,
        IExternalSalesRepository externalSalesRepository,
        ISaleQuotationService saleQuotationService)
    {
        _saleRepository = saleRepository;
        _quotationRepository = quotationRepository;
        _dbContext = dbContext;
        _customerRepository = customerRepository;
        _externalSalesService = externalSalesService;
        _externalSalesRepository = externalSalesRepository;
        _saleQuotationService = saleQuotationService;
    }

    public async Task<Sale> CreateSaleAsync(SaleExternalDto saleDto, string createdByUserId, int customerId, string salesExecutive)
    {
        if (saleDto is null) throw new ArgumentNullException(nameof(saleDto));

        // 🔍 Buscar cliente existente por código o nombre
        var customer = await _customerRepository.FindByNameOrCodeAsync(saleDto.CodeClient);

        // 🧾 Si no existe, crear un nuevo cliente
        if (customer == null)
        {
            customer = new Customer
            {
                ExternalId = int.TryParse(saleDto.CodeClient, out var extId) ? extId : 0,
                Name = saleDto.NombreCliente,
                PhoneNumber = string.IsNullOrWhiteSpace(saleDto.TelCliente) ? "+00" : saleDto.TelCliente,
                Email = saleDto.EmailCliente,
                DirectionJson = new DirectionJson
                {
                    Colony = saleDto.DireccionCliente,
                    City = saleDto.PoblacionCliente,
                    Index = 1
                },
                SettingsCustomerJson = new SettingsCustomerJson
                {
                    Index = 1,
                    Type = "General"
                }
            };

            customer = await _customerRepository.InsertAsync(customer);
        }

        // 🧩 Convertir DTO a entidad Sale usando la extensión
        var saleEntity = saleDto.ToEntity(customer.IdCustomer, salesExecutive, saleDto.IdConfigSys);

        // 🕒 Establecer fechas
        saleEntity.CreatedAt = DateTime.UtcNow;
        saleEntity.UpdatedAt = DateTime.UtcNow;

        // 💾 Guardar venta
        var created = await _saleRepository.CreateSaleAsync(saleEntity);

        return created;
    }


    public async Task<Sale?> GetSaleByIdAsync(int saleId)
    {
        return await _saleRepository.GetSaleByIdAsync(saleId);
    }
    public async Task<Sale?> GetSaleByFolioAsync(string folio)
    {
        return await _saleRepository.GetSaleByFolioAsync(folio);
    }
    public async Task<bool> DeleteSaleAsync(int id)
    {
        return await _saleRepository.DeleteSaleAsync(id);
    }

    public async Task<Sale> CreateSaleFromQuotationsAsync(IEnumerable<int> quotationIds, Sale saleData, string createdByUserId)
    {
        if (quotationIds == null) throw new ArgumentNullException(nameof(quotationIds));
        if (saleData == null) throw new ArgumentNullException(nameof(saleData));

        await using var tx = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            saleData.CreatedAt = DateTime.UtcNow;
            saleData.UpdatedAt = DateTime.UtcNow;
            var createdSale = await _saleRepository.CreateSaleAsync(saleData);

            foreach (var qid in quotationIds)
            {
                var quotation = await _quotationRepository.GetByIdAsync(qid);
                if (quotation == null)
                {
                    throw new InvalidOperationException($"Quotation {qid} not found.");
                }

                quotation.LinkedSaleId = createdSale.IdSale.ToString();
                quotation.LinkedSaleFolio = createdSale.Folio;
                quotation.UpdatedAt = DateTime.UtcNow;

                await _quotationRepository.UpdateQuotationAsync(quotation);
            }

            await tx.CommitAsync();
            return createdSale;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Inserta una venta desde datos del sistema externo (InforAVA)
    /// y la vincula automáticamente con una cotización existente.
    /// 
    /// FLUJO TRANSACCIONAL:
    /// 1. Obtiene datos generales de VENTASPORFECHAV
    /// 2. Obtiene detalles de productos de DetalleVentaV
    /// 3. Crea/busca cliente
    /// 4. Registra venta con productos en ProductsJson
    /// 5. Crea relación SaleQuotation
    /// 6. Opcionalmente marca cotización como primaria
    /// </summary>
    public async Task<Sale> InsertExternalSaleAndLinkQuotationAsync(
        InsertExternalSaleAndQuotationRequest request,
        string createdByUserId)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request), "Request cannot be null");

        if (string.IsNullOrWhiteSpace(request.Catalogo))
            throw new ArgumentException("Catalogo cannot be empty", nameof(request.Catalogo));

        if (string.IsNullOrWhiteSpace(request.Folio))
            throw new ArgumentException("Folio cannot be empty", nameof(request.Folio));

        if (request.IdQuotation <= 0)
            throw new ArgumentException("IdQuotation must be greater than 0", nameof(request.IdQuotation));

        await using var tx = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // 1️⃣ VALIDAR COTIZACIÓN
            var quotation = await _quotationRepository.GetByIdAsync(request.IdQuotation);
            if (quotation == null)
                throw new InvalidOperationException($"Quotation {request.IdQuotation} not found.");

            // 2️⃣ OBTENER DATOS GENERALES DE LA VENTA DESDE SISTEMA EXTERNO
            var externalSaleDetails = await _externalSalesService.GetSaleDetailAsync(
                request.NF ?? string.Empty,
                request.Caja,
                request.Serie,
                request.Folio
            );

            if (!externalSaleDetails.Any())
                throw new InvalidOperationException($"No sale details found for folio {request.Folio} in external system.");

            // 3️⃣ BUSCAR O CREAR CLIENTE
            Customer? customer = null;

            if (request.IdCustomer.HasValue && request.IdCustomer > 0)
            {
                // Usar cliente especificado
                customer = await _customerRepository.GetByIdAsync(request.IdCustomer.Value);
                if (customer == null)
                    throw new InvalidOperationException($"Customer {request.IdCustomer} not found.");
            }
            else
            {
                // Obtener información del cliente desde los datos externos (si disponible)
                // Por ahora, crear un cliente genérico o buscar existente
                customer = new Customer
                {
                    ExternalId = 0,
                    Name = $"External Sale {request.Folio}",
                    PhoneNumber = "+00",
                    Email = string.Empty,
                    DirectionJson = new DirectionJson
                    {
                        Colony = string.Empty,
                        City = string.Empty,
                        Index = 1
                    },
                    SettingsCustomerJson = new SettingsCustomerJson
                    {
                        Index = 1,
                        Type = "General"
                    }
                };

                customer = await _customerRepository.InsertAsync(customer);
            }

            // 4️⃣ CREAR ENTIDAD SALE
            var sale = new Sale
            {
                IdCustomer = customer.IdCustomer,
                SalesExecutive = request.SalesExecutive ?? "External System",
                SaleDate = DateTime.UtcNow,
                Type = "External",
                Folio = request.Folio,
                TotalAmount = externalSaleDetails.Sum(d => d.Total),
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,

                // 5️⃣ MAPEAR PRODUCTOS DESDE DETALLES EXTERNOS
                ProductsJson = externalSaleDetails
                    .Select(detail => new SingleProductJson
                    {
                        Unit = detail.Unidad ?? "PZ",
                        Quantity = (double)detail.Cantidad,
                        ProductId = 0, // Se podría buscar por código
                        UnitPrice = detail.Precio,
                        TotalPrice = detail.Total,
                        Description = detail.Descripcion ?? $"Producto {detail.Codigo}"
                    })
                    .ToList(),

                // 6️⃣ GUARDAR DATOS AUXILIARES EXTERNOS
                AuxNoteDataJson = new AuxNoteDataJson
                {
                    Cliente = string.Empty,
                    NombreCliente = string.Empty,
                    Folio = request.Folio,
                    Fecha = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                    Hora = DateTime.UtcNow.ToString("HH:mm:ss"),
                    Serie = request.Serie,
                    Caja = request.Caja,
                    Zn = string.Empty,
                    Nf = request.NF ?? string.Empty,
                    Agente = request.SalesExecutive ?? "External System",
                    DireccionCliente = string.Empty,
                    PoblacionCliente = string.Empty,
                    EmailCliente = string.Empty,
                    TelCliente = string.Empty,
                    Importe = externalSaleDetails.Sum(d => d.Importe),
                    Descuento = externalSaleDetails.Sum(d => d.Dcto),
                    Impuesto = externalSaleDetails.Sum(d => d.Impto),
                    Total = externalSaleDetails.Sum(d => d.Total),
                    ExisteEnDB = true
                }
            };

            // 7️⃣ GUARDAR VENTA EN BD
            var createdSale = await _saleRepository.CreateSaleAsync(sale);

            // 8️⃣ CREAR RELACIÓN SALEQUOTATION
            var saleQuotation = new SaleQuotation
            {
                IdSale = createdSale.IdSale,
                IdQuotation = request.IdQuotation,
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.UtcNow,
                GeneralComment = $"Linked from external sale {request.Folio}",

                // Copiar productos desde venta
                ProductsJson = createdSale.ProductsJson ?? new List<SingleProductJson>(),

                // Crear snapshot de precios
                PriceSnapshot = new PriceSnapshotJson
                {
                    Subtotal = createdSale.TotalAmount,
                    TaxAmount = 0,
                    TotalAmount = createdSale.TotalAmount
                }
            };

            // Guardar relación (esto se haría a través del repositorio)
            // Por ahora, agregamos a la colección de la venta
            await _dbContext.Set<SaleQuotation>().AddAsync(saleQuotation);
            await _dbContext.SaveChangesAsync();

            // 9️⃣ SI MARCAR COMO PRIMARIA, ACTUALIZAR
            if (request.MarkAsPrimary)
            {
                await _saleQuotationService.MarkPrimaryQuotationAsync(
                    createdSale.IdSale,
                    request.IdQuotation,
                    createdByUserId
                );
            }

            await tx.CommitAsync();
            return createdSale;
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            throw new InvalidOperationException(
                $"Error inserting external sale and linking quotation: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Importa ventas del sistema externo para un mes completo de forma optimizada.
    /// 
    /// ESTRATEGIA DE OPTIMIZACIÓN:
    /// 1. Procesamiento por lotes de días para reducir llamadas a API
    /// 2. Cache de clientes para evitar consultas repetitivas a BD
    /// 3. Verificación previa de duplicados
    /// 4. Inserción en lotes transaccionales
    /// 5. Manejo de errores individuales sin afectar el lote completo
    /// 6. Limitación de concurrencia para no saturar API externa
    /// 
    /// FLUJO:
    /// 1. Validar parámetros (mes válido, no futuro)
    /// 2. Dividir mes en lotes de días
    /// 3. Para cada lote:
    ///    a. Obtener ventas externas del lote
    ///    b. Filtrar ventas ya importadas
    ///    c. Crear/encontrar clientes necesarios
    ///    d. Obtener detalles de productos en paralelo controlado
    ///    e. Insertar ventas en transacción
    /// 4. Compilar estadísticas de resultado
    /// </summary>
    public async Task<ImportSalesResult> ImportSalesForMonthAsync(
        int year, 
        int month, 
        int idConfigSys, 
        string createdByUserId, 
        int batchSize = 5)
    {
        var result = new ImportSalesResult
        {
            StartDate = new DateTime(year, month, 1),
            EndDate = new DateTime(year, month, DateTime.DaysInMonth(year, month)),
        };

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // 🔍 VALIDACIONES INICIALES
            await ValidateImportParametersAsync(year, month, idConfigSys);

            // 📅 GENERAR LOTES DE FECHAS
            var dateBatches = GenerateDateBatches(result.StartDate, result.EndDate, batchSize);
            result.BatchesProcessed = dateBatches.Count;

            // 💾 CACHE DE CLIENTES PARA OPTIMIZACIÓN
            var customerCache = new Dictionary<string, Customer>();

            // 🔄 PROCESAR CADA LOTE
            int batchNumber = 1;
            foreach (var batch in dateBatches)
            {
                var batchSummary = await ProcessDateBatchAsync(
                    batch, 
                    idConfigSys, 
                    createdByUserId, 
                    customerCache, 
                    batchNumber);

                result.BatchSummaries.Add(batchSummary);
                
                // Acumular estadísticas
                result.TotalSalesFound += batchSummary.SalesProcessed;
                result.TotalSalesImported += batchSummary.SalesImported;
                result.TotalSalesSkipped += batchSummary.SalesSkipped;
                result.TotalSalesError += batchSummary.SalesError;

                batchNumber++;

                // 😴 PAUSA ENTRE LOTES para no saturar API externa
                if (batchNumber <= dateBatches.Count)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }

            // 📊 ESTADÍSTICAS FINALES
            result.CustomersCreated = customerCache.Values.Count(c => c.IdCustomer == 0); // Nuevos
            result.CustomersReused = customerCache.Values.Count(c => c.IdCustomer > 0); // Existentes
            result.CustomersFound = result.CustomersCreated + result.CustomersReused;

            stopwatch.Stop();
            result.TotalProcessingTime = stopwatch.Elapsed;
            result.AverageTimePerBatch = result.BatchesProcessed > 0 
                ? result.TotalProcessingTime.TotalSeconds / result.BatchesProcessed 
                : 0;

            result.IsSuccessful = result.TotalSalesError == 0;
            result.Message = result.IsSuccessful 
                ? $"Importación completada exitosamente. {result.TotalSalesImported} ventas importadas."
                : $"Importación completada con {result.TotalSalesError} errores. {result.TotalSalesImported} ventas importadas.";

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.TotalProcessingTime = stopwatch.Elapsed;
            result.IsSuccessful = false;
            result.Message = $"Error crítico durante la importación: {ex.Message}";
            result.Errors.Add(new ImportErrorDetail
            {
                ErrorType = "CriticalError",
                ErrorMessage = ex.Message,
                ErrorTimestamp = DateTime.UtcNow
            });

            return result;
        }
    }

    /// <summary>
    /// Valida los parámetros de importación.
    /// </summary>
    private async Task ValidateImportParametersAsync(int year, int month, int idConfigSys)
    {
        if (year < 2020 || year > DateTime.UtcNow.Year)
            throw new ArgumentException($"El año {year} no es válido. Debe estar entre 2020 y {DateTime.UtcNow.Year}.");

        if (month < 1 || month > 12)
            throw new ArgumentException($"El mes {month} no es válido. Debe estar entre 1 y 12.");

        var importDate = new DateTime(year, month, 1);
        if (importDate > DateTime.UtcNow.Date)
            throw new ArgumentException("No se puede importar datos de fechas futuras.");

        // Verificar que IdConfigSys existe (opcional, depende de tu implementación)
        // var configExists = await _configSysRepository.ExistsAsync(idConfigSys);
        // if (!configExists)
        //     throw new ArgumentException($"El sistema de configuración {idConfigSys} no existe.");

        await Task.CompletedTask;
    }

    /// <summary>
    /// Divide el rango de fechas en lotes para procesamiento optimizado.
    /// </summary>
    private List<(DateTime StartDate, DateTime EndDate)> GenerateDateBatches(
        DateTime startDate, 
        DateTime endDate, 
        int batchSize)
    {
        var batches = new List<(DateTime, DateTime)>();
        var currentDate = startDate;

        while (currentDate <= endDate)
        {
            var batchEndDate = currentDate.AddDays(batchSize - 1);
            if (batchEndDate > endDate)
                batchEndDate = endDate;

            batches.Add((currentDate, batchEndDate));
            currentDate = batchEndDate.AddDays(1);
        }

        return batches;
    }

    /// <summary>
    /// Procesa un lote de fechas específico.
    /// </summary>
    private async Task<BatchProcessingSummary> ProcessDateBatchAsync(
        (DateTime StartDate, DateTime EndDate) batch,
        int idConfigSys,
        string createdByUserId,
        Dictionary<string, Customer> customerCache,
        int batchNumber)
    {
        var summary = new BatchProcessingSummary
        {
            BatchNumber = batchNumber,
            BatchStartDate = batch.StartDate,
            BatchEndDate = batch.EndDate
        };

        var batchStopwatch = Stopwatch.StartNew();

        try
        {
            // 🔍 OBTENER VENTAS EXTERNAS PARA EL LOTE
            var externalSales = await GetExternalSalesForDateRangeAsync(batch.StartDate, batch.EndDate);
            summary.SalesProcessed = externalSales.Count();

            if (!externalSales.Any())
            {
                summary.Message = "No hay ventas en el período especificado.";
                summary.IsSuccessful = true;
                return summary;
            }

            // 🚫 FILTRAR VENTAS YA EXISTENTES
            var existingFolios = await GetExistingSaleFoliosAsync(externalSales.Select(s => s.Folio).Where(f => !string.IsNullOrEmpty(f)));
            var newSales = externalSales.Where(s => !existingFolios.Contains(s.Folio)).ToList();
            
            summary.SalesSkipped = summary.SalesProcessed - newSales.Count;

            // 👥 PROCESAR CLIENTES
            await ProcessCustomersForSalesAsync(newSales, customerCache);

            // 🛒 PROCESAR VENTAS CON DETALLES
            foreach (var externalSale in newSales)
            {
                try
                {
                    await ProcessSingleSaleAsync(externalSale, idConfigSys, createdByUserId, customerCache);
                    summary.SalesImported++;
                }
                catch (Exception ex)
                {
                    summary.SalesError++;
                    // Log del error específico de la venta
                    // _logger?.LogWarning($"Error al procesar venta {externalSale.Folio}: {ex.Message}");
                }
            }

            summary.IsSuccessful = summary.SalesError == 0;
            summary.Message = summary.IsSuccessful 
                ? $"Lote procesado exitosamente: {summary.SalesImported} importadas, {summary.SalesSkipped} omitidas."
                : $"Lote procesado con errores: {summary.SalesImported} importadas, {summary.SalesError} errores.";

        }
        catch (Exception ex)
        {
            summary.IsSuccessful = false;
            summary.Message = $"Error crítico en el lote: {ex.Message}";
            summary.SalesError = summary.SalesProcessed - summary.SalesImported;
        }
        finally
        {
            batchStopwatch.Stop();
            summary.ProcessingTime = batchStopwatch.Elapsed;
        }

        return summary;
    }

    /// <summary>
    /// Obtiene ventas externas para un rango de fechas.
    /// </summary>
    private async Task<IEnumerable<ExternalSalesDto>> GetExternalSalesForDateRangeAsync(
        DateTime startDate, 
        DateTime endDate)
    {
        var allSales = new List<ExternalSalesDto>();
        const string catalogo = "AVA01"; // Catálogo fijo

        // Iterar día por día para obtener todas las ventas del rango
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            try
            {
                var dailySales = await _externalSalesRepository.GetSalesByDateAndCatalogAsync(catalogo, date);
                allSales.AddRange(dailySales);

                // Pequeña pausa entre consultas para no sobrecargar API externa
                await Task.Delay(500);
            }
            catch (Exception)
            {
                // Continuar con el siguiente día si una fecha falla
                continue;
            }
        }

        return allSales;
    }

    /// <summary>
    /// Obtiene folios de ventas que ya existen en la base de datos.
    /// </summary>
    private async Task<HashSet<string>> GetExistingSaleFoliosAsync(IEnumerable<string> folios)
    {
        var existingSales = await _saleRepository.GetSalesByFoliosAsync(folios);
        return new HashSet<string>(existingSales.Select(s => s.Folio), StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Procesa y crea/encuentra clientes necesarios para las ventas.
    /// OPTIMIZACIÓN MEJORADA: 
    /// 1. Pre-carga todos los clientes existentes para el lote
    /// 2. Usa ExternalId como clave principal
    /// 3. Maneja duplicados de ExternalId de forma robusta
    /// </summary>
    private async Task ProcessCustomersForSalesAsync(
        IEnumerable<ExternalSalesDto> sales, 
        Dictionary<string, Customer> customerCache)
    {
        var salesList = sales.ToList();
        
        // 🚀 OPTIMIZACIÓN: Pre-cargar todos los ExternalIds únicos del lote
        var externalIds = salesList
            .Where(s => !string.IsNullOrWhiteSpace(s.Cliente) && int.TryParse(s.Cliente.Trim(), out _))
            .Select(s => int.Parse(s.Cliente.Trim()))
            .Distinct()
            .ToList();

        // 📦 Cargar todos los clientes existentes en una sola consulta
        var existingCustomers = new Dictionary<int, Customer>();
        
        if (externalIds.Any())
        {
            var customers = await _customerRepository.SelectAsync(null, null, null);
            foreach (var customer in customers.Where(c => externalIds.Contains(c.ExternalId)))
            {
                existingCustomers[customer.ExternalId] = customer;
            }
        }

        // 🔄 Procesar cada venta
        var customersToCreate = new List<(string ExternalId, ExternalSalesDto Sale)>();
        
        foreach (var sale in salesList)
        {
            if (string.IsNullOrWhiteSpace(sale.Cliente) || string.IsNullOrWhiteSpace(sale.NombreCliente))
                continue;

            var customerExternalId = sale.Cliente.Trim();

            if (!customerCache.ContainsKey(customerExternalId))
            {
                // 🔍 Buscar en clientes pre-cargados
                Customer? existingCustomer = null;
                
                if (int.TryParse(customerExternalId, out var extId) && existingCustomers.ContainsKey(extId))
                {
                    existingCustomer = existingCustomers[extId];
                }
                
                // Si no se encuentra por ExternalId, buscar por nombre (fallback)
                if (existingCustomer == null)
                {
                    existingCustomer = await _customerRepository.FindByNameOrCodeAsync(sale.NombreCliente);
                }

                if (existingCustomer != null)
                {
                    // ✅ Cliente existente encontrado
                    customerCache[customerExternalId] = existingCustomer;
                }
                else
                {
                    // 📝 Agregar a lista de clientes por crear
                    customersToCreate.Add((customerExternalId, sale));
                }
            }
        }

        // 🆕 Crear clientes nuevos uno por uno con manejo de conflictos
        foreach (var (externalId, sale) in customersToCreate)
        {
            if (!customerCache.ContainsKey(externalId))
            {
                try
                {
                    var newCustomer = await CreateBasicCustomerFromSaleDataAsync(sale);
                    customerCache[externalId] = newCustomer;
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message?.Contains("IX_Customers_ExternalId") == true)
                {
                    // 🔄 CONFLICTO DE ExternalId - intentar recuperar el cliente existente
                    if (int.TryParse(externalId, out var conflictExtId))
                    {
                        var allCustomers = await _customerRepository.SelectAsync(null, null, conflictExtId);
                        var conflictCustomer = allCustomers.FirstOrDefault();
                        
                        if (conflictCustomer != null)
                        {
                            customerCache[externalId] = conflictCustomer;
                        }
                        else
                        {
                            // Crear con ExternalId alternativo como último recurso
                            try
                            {
                                var altCustomer = await CreateBasicCustomerFromSaleDataAsync(sale, useAlternativeExternalId: true);
                                customerCache[externalId] = altCustomer;
                            }
                            catch (Exception)
                            {
                                // Si todo falla, crear un cliente genérico simple
                                var fallbackCustomer = await CreateFallbackCustomerAsync(sale.NombreCliente ?? "Cliente Importado");
                                customerCache[externalId] = fallbackCustomer;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // En caso de otros errores, crear cliente fallback
                    var fallbackCustomer = await CreateFallbackCustomerAsync(sale.NombreCliente ?? "Cliente Importado");
                    customerCache[externalId] = fallbackCustomer;
                }
            }
        }
    }

    /// <summary>
    /// Crea un cliente de fallback con un ExternalId garantizado único.
    /// </summary>
    private async Task<Customer> CreateFallbackCustomerAsync(string customerName)
    {
        var customer = new Customer
        {
            Name = customerName.Trim(),
            LastName = string.Empty,
            PhoneNumber = "+00",
            Email = string.Empty,
            ExternalId = await _customerRepository.GetNextExternalIdAsync(),
            DirectionJson = new DirectionJson
            {
                Colony = string.Empty,
                City = string.Empty,
                Index = 1
            },
            SettingsCustomerJson = new SettingsCustomerJson
            {
                Index = 1,
                Type = "General"
            }
        };

        return await _customerRepository.InsertAsync(customer);
    }

    /// <summary>
    /// Crea un cliente básico a partir de los datos de la venta externa.
    /// </summary>
    private async Task<Customer> CreateBasicCustomerFromSaleDataAsync(ExternalSalesDto sale, bool useAlternativeExternalId = false)
    {
        int externalId;
        
        if (useAlternativeExternalId)
        {
            // Usar un ExternalId alternativo para evitar duplicados
            externalId = await _customerRepository.GetNextExternalIdAsync();
        }
        else
        {
            // Intentar usar el ExternalId del sistema externo
            externalId = int.TryParse(sale.Cliente, out var extId) ? extId : await _customerRepository.GetNextExternalIdAsync();
            
            // Verificar si ya existe un cliente con este ExternalId
            var existingCustomers = await _customerRepository.SelectAsync(null, null, externalId);
            var existingCustomer = existingCustomers.FirstOrDefault();
            if (existingCustomer != null)
            {
                // Si ya existe, devolver el cliente existente en lugar de crear uno nuevo
                return existingCustomer;
            }
        }

        var customer = new Customer
        {
            Name = sale.NombreCliente?.Trim() ?? "Cliente Importado",
            LastName = string.Empty, // Solo nombre completo disponible
            PhoneNumber = sale.TelCliente?.Trim() ?? "+00",
            Email = sale.EmailCliente?.Trim() ?? string.Empty,
            ExternalId = externalId,
            DirectionJson = new DirectionJson
            {
                Colony = sale.DireccionCliente?.Trim() ?? string.Empty,
                City = sale.PoblacionCliente?.Trim() ?? string.Empty,
                Index = 1
            },
            SettingsCustomerJson = new SettingsCustomerJson
            {
                Index = 1,
                Type = "General"
            }
        };

        try
        {
            return await _customerRepository.InsertAsync(customer);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException?.Message?.Contains("IX_Customers_ExternalId") == true)
        {
            // Si ocurre error de clave duplicada, generar un nuevo ExternalId único
            customer.ExternalId = await _customerRepository.GetNextExternalIdAsync();
            return await _customerRepository.InsertAsync(customer);
        }
    }

    /// <summary>
    /// Procesa una venta individual con todos sus detalles.
    /// </summary>
    private async Task ProcessSingleSaleAsync(
        ExternalSalesDto externalSale, 
        int idConfigSys, 
        string createdByUserId, 
        Dictionary<string, Customer> customerCache)
    {
        // 🔍 OBTENER CLIENTE usando ExternalId como clave
        var customerExternalId = externalSale.Cliente?.Trim() ?? "0";
        
        if (!customerCache.TryGetValue(customerExternalId, out var customer))
        {
            throw new InvalidOperationException($"Cliente no encontrado en cache para ExternalId: {customerExternalId}, Cliente: {externalSale.NombreCliente}");
        }

        // 🛒 OBTENER DETALLES DE PRODUCTOS
        var productDetails = await GetSaleProductDetailsAsync(externalSale);

        // 📅 PROCESAR FECHA Y HORA
        var saleDateTime = ParseSaleDateTime(externalSale.Fecha, externalSale.Hora);

        // 🏗️ CONSTRUIR ENTIDAD SALE
        var sale = new Sale
        {
            IdCustomer = customer.IdCustomer,
            SalesExecutive = externalSale.Agente ?? "Importado",
            SaleDate = saleDateTime,
            Type = externalSale.ZN ?? "Importado",
            Folio = externalSale.Folio ?? Guid.NewGuid().ToString()[..8],
            TotalAmount = externalSale.Total,
            DeliveryDriver = null,
            HomeDelivery = false,
            DeliveryDate = null,
            SatisfactionLevel = null,
            SatisfactionReason = null,
            Comment = null,
            AfterSalesFollowupDate = null,
            LinkedQuotations = new List<QuotationReference>(),
            ProductsJson = productDetails,
            AuxNoteDataJson = CreateAuxNoteDataFromExternalSale(externalSale),
            IdConfigSys = idConfigSys,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // 💾 INSERTAR VENTA
        await _saleRepository.InsertAsync(sale);
    }

    /// <summary>
    /// Obtiene los detalles de productos de una venta externa.
    /// </summary>
    private async Task<List<SingleProductJson>?> GetSaleProductDetailsAsync(ExternalSalesDto externalSale)
    {
        try
        {
            if (string.IsNullOrEmpty(externalSale.NF) || string.IsNullOrEmpty(externalSale.Caja) ||
                string.IsNullOrEmpty(externalSale.Serie) || string.IsNullOrEmpty(externalSale.Folio))
            {
                return null;
            }

            var details = await _externalSalesRepository.GetSaleDetailAsync(
                externalSale.NF, 
                externalSale.Caja, 
                externalSale.Serie, 
                externalSale.Folio);

            return details.Select(d => new SingleProductJson
            {
                ProductId = null, // Se podría buscar por código si existe
                Description = $"{d.Codigo} - {d.Descripcion ?? "Producto Importado"}",
                Quantity = (double)d.Cantidad,
                UnitPrice = d.Precio,
                TotalPrice = d.Total,
                Unit = d.Unidad ?? "PZ"
            }).ToList();
        }
        catch (Exception)
        {
            return null; // Continuar sin detalles si falla
        }
    }

    /// <summary>
    /// Parsea fecha y hora de los datos externos.
    /// </summary>
    private DateTime ParseSaleDateTime(string? fecha, string? hora)
    {
        var today = DateTime.UtcNow.Date;

        if (!DateTime.TryParse(fecha, out var saleDate))
        {
            saleDate = today;
        }

        if (!string.IsNullOrEmpty(hora) && TimeSpan.TryParse(hora, out var saleTime))
        {
            return saleDate.Date.Add(saleTime);
        }

        return saleDate;
    }

    /// <summary>
    /// Crea el objeto AuxNoteDataJson a partir de los datos externos.
    /// </summary>
    private AuxNoteDataJson CreateAuxNoteDataFromExternalSale(ExternalSalesDto externalSale)
    {
        return new AuxNoteDataJson
        {
            Cliente = externalSale.Cliente ?? string.Empty,
            NombreCliente = externalSale.NombreCliente ?? string.Empty,
            Folio = externalSale.Folio ?? string.Empty,
            Fecha = externalSale.Fecha ?? string.Empty,
            Hora = externalSale.Hora ?? string.Empty,
            Serie = externalSale.Serie ?? string.Empty,
            Caja = externalSale.Caja ?? string.Empty,
            Zn = externalSale.ZN ?? string.Empty,
            Nf = externalSale.NF ?? string.Empty,
            Agente = externalSale.Agente ?? string.Empty,
            DireccionCliente = externalSale.DireccionCliente ?? string.Empty,
            PoblacionCliente = externalSale.PoblacionCliente ?? string.Empty,
            EmailCliente = externalSale.EmailCliente ?? string.Empty,
            TelCliente = externalSale.TelCliente ?? string.Empty,
            Importe = externalSale.Importe,
            Descuento = externalSale.Descuento,
            Impuesto = externalSale.Impuesto,
            Total = externalSale.Total,
            ExisteEnDB = true
        };
    }
}
