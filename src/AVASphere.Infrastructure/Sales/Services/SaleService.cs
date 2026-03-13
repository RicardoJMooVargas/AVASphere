using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using System.Globalization;
using AVASphere.ApplicationCore.Common.Entities.General;
using AVASphere.ApplicationCore.Common.Entities.Jsons;
using AVASphere.ApplicationCore.Common.Extensions;
using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.ApplicationCore.Sales.DTOs;
using AVASphere.ApplicationCore.Sales.DTOs.ImportDTOs;
using AVASphere.ApplicationCore.Sales.DTOs.SaleDTOs;
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
    /// Crea una venta y la vincula inmediatamente con una cotización existente.
    /// Usado cuando desde el frontend se convierte una cotización en venta.
    /// 
    /// FLUJO TRANSACCIONAL:
    /// 1. Valida que la cotización exista
    /// 2. Valida que el cliente exista
    /// 3. Crea el registro en la tabla Sales
    /// 4. Crea el registro en la tabla SaleQuotations (vinculación)
    /// 5. Opcionalmente marca la cotización como primaria
    /// 6. Actualiza la cotización con los datos de la venta vinculada
    /// 7. Commit de transacción
    /// </summary>
    public async Task<Sale> CreateSaleAndLinkQuotationAsync(
        CreateSaleWithQuotationLinkDto dto,
        string createdByUserId)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "DTO cannot be null");

        await using var tx = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // 1️⃣ VALIDAR COTIZACIÓN
            var quotation = await _quotationRepository.GetByIdAsync(dto.IdQuotation);
            if (quotation == null)
                throw new InvalidOperationException($"Quotation with ID {dto.IdQuotation} not found");

            // 2️⃣ VALIDAR CLIENTE
            var customer = await _customerRepository.GetByIdAsync(dto.IdCustomer);
            if (customer == null)
                throw new InvalidOperationException($"Customer with ID {dto.IdCustomer} not found");

            // 3️⃣ VALIDAR FOLIO ÚNICO
            var existingSale = await _saleRepository.GetSaleByFolioAsync(dto.Folio);
            if (existingSale != null)
                throw new InvalidOperationException($"A sale with folio '{dto.Folio}' already exists");

            // 4️⃣ CREAR ENTIDAD SALE
            var sale = new Sale
            {
                IdCustomer = dto.IdCustomer,
                SalesExecutive = dto.SalesExecutive,
                SaleDate = dto.SaleDate ?? DateTime.UtcNow,
                Type = dto.Type,
                Folio = dto.Folio,
                TotalAmount = dto.TotalAmount,
                DeliveryDriver = dto.DeliveryDriver,
                HomeDelivery = dto.HomeDelivery,
                DeliveryDate = dto.DeliveryDate,
                SatisfactionLevel = dto.SatisfactionLevel,
                SatisfactionReason = dto.SatisfactionReason,
                Comment = dto.Comment,
                AfterSalesFollowupDate = dto.AfterSalesFollowupDate,
                IdConfigSys = dto.IdConfigSys,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,

                // Inicializar colecciones vacías
                ProductsJson = new List<SingleProductJson>(),
                LinkedQuotations = new List<QuotationReference>()
            };

            // 5️⃣ INSERTAR VENTA
            var createdSale = await _saleRepository.CreateSaleAsync(sale);

            // 6️⃣ CREAR RELACIÓN SALEQUOTATION
            var saleQuotation = new SaleQuotation
            {
                IdSale = createdSale.IdSale,
                IdQuotation = dto.IdQuotation,
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.UtcNow,
                GeneralComment = dto.GeneralCommentForLink ?? $"Venta creada desde cotización {dto.IdQuotation}",

                // Copiar productos desde la cotización si existen
                ProductsJson = quotation.ProductsJson ?? new List<SingleProductJson>(),

                // Crear snapshot de precios
                PriceSnapshot = new PriceSnapshotJson
                {
                    Subtotal = dto.TotalAmount,
                    TaxAmount = 0,
                    TotalAmount = dto.TotalAmount,
                    Currency = "MXN"
                }
            };

            await _dbContext.Set<SaleQuotation>().AddAsync(saleQuotation);
            await _dbContext.SaveChangesAsync();

            // 7️⃣ ACTUALIZAR COTIZACIÓN CON DATOS DE LA VENTA
            quotation.LinkedSaleId = createdSale.IdSale.ToString();
            quotation.LinkedSaleFolio = createdSale.Folio;
            quotation.UpdatedAt = DateTime.UtcNow;
            await _quotationRepository.UpdateQuotationAsync(quotation);

            // 8️⃣ SI MARCAR COMO PRIMARIA
            if (dto.MarkAsPrimary)
            {
                await _saleQuotationService.MarkPrimaryQuotationAsync(
                    createdSale.IdSale,
                    dto.IdQuotation,
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
                $"Error creating sale and linking quotation: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Desvincula una venta de una cotización existente y elimina completamente la venta.
    /// Elimina el registro de SaleQuotations, actualiza la Quotation removiendo la referencia y elimina la Sale.
    /// </summary>
    public async Task<bool> UnlinkSaleFromQuotationAsync(int saleId, int quotationId, string requestedByUserId)
    {
        await using var tx = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // 1️⃣ VALIDAR QUE LA VENTA EXISTA
            var sale = await _saleRepository.GetSaleByIdAsync(saleId);
            if (sale == null)
                throw new InvalidOperationException($"Sale with ID {saleId} not found");

            // 2️⃣ VALIDAR QUE LA COTIZACIÓN EXISTA
            var quotation = await _quotationRepository.GetByIdAsync(quotationId);
            if (quotation == null)
                throw new InvalidOperationException($"Quotation with ID {quotationId} not found");

            // 3️⃣ VALIDAR QUE EXISTA LA RELACIÓN
            var saleQuotations = await _dbContext.Set<SaleQuotation>()
                .Where(sq => sq.IdSale == saleId && sq.IdQuotation == quotationId)
                .ToListAsync();

            if (saleQuotations == null || saleQuotations.Count == 0)
                throw new InvalidOperationException(
                    $"No relationship found between Sale {saleId} and Quotation {quotationId}");

            // 4️⃣ ELIMINAR LA RELACIÓN EN SALEQUOTATIONS
            _dbContext.Set<SaleQuotation>().RemoveRange(saleQuotations);
            await _dbContext.SaveChangesAsync();

            // 5️⃣ ACTUALIZAR COTIZACIÓN REMOVIENDO REFERENCIA A LA VENTA
            quotation.LinkedSaleId = null;
            quotation.LinkedSaleFolio = null;
            quotation.UpdatedAt = DateTime.UtcNow;
            await _quotationRepository.UpdateQuotationAsync(quotation);

            // 6️⃣ ELIMINAR LA VENTA COMPLETAMENTE
            var saleDeleted = await _saleRepository.DeleteSaleAsync(saleId);
            if (!saleDeleted)
                throw new InvalidOperationException($"Failed to delete Sale {saleId}");

            await tx.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            throw new InvalidOperationException(
                $"Error unlinking and deleting sale {saleId} from quotation {quotationId}: {ex.Message}",
                ex
            );
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
    /// Importa ventas del sistema externo InforAVA para un mes completo de forma optimizada.
    ///
    /// ESTRATEGIAS DE OPTIMIZACIÓN:
    /// <list type="number">
    ///   <item>Procesamiento por lotes de días para reducir el número de llamadas a la API externa.</item>
    ///   <item>Cache en memoria de clientes (<c>Dictionary&lt;string, Customer&gt;</c>) indexado por <c>ExternalId</c>
    ///         para evitar consultas repetitivas a la BD durante el mismo proceso.</item>
    ///   <item>Pre-carga masiva de clientes existentes en una sola consulta por lote.</item>
    ///   <item>Verificación previa de folios duplicados antes de intentar insertar.</item>
    ///   <item>Manejo de errores individuales por venta: un fallo no cancela el lote completo.</item>
    ///   <item>Pausa de 2 segundos entre lotes para no saturar la API externa.</item>
    ///   <item>Pausa de 500 ms entre llamadas día a día dentro del mismo lote.</item>
    /// </list>
    ///
    /// FLUJO DETALLADO:
    /// <list type="number">
    ///   <item>Validar parámetros (mes válido, no futuro, idConfigSys existente).</item>
    ///   <item>Dividir el mes en sub-rangos de <paramref name="batchSize"/> días.</item>
    ///   <item>
    ///     Para cada lote (<see cref="ProcessDateBatchAsync"/>):
    ///     <list type="bullet">
    ///       <item>Obtener ventas externas día a día (<see cref="GetExternalSalesForDateRangeAsync"/>).</item>
    ///       <item>Filtrar folios ya existentes en la BD (<see cref="GetExistingSaleFoliosAsync"/>).</item>
    ///       <item>Crear/encontrar clientes necesarios (<see cref="ProcessCustomersForSalesAsync"/>).</item>
    ///       <item>Procesar cada venta nueva individualmente (<see cref="ProcessSingleSaleAsync"/>).</item>
    ///     </list>
    ///   </item>
    ///   <item>Compilar estadísticas finales de clientes creados vs reutilizados.</item>
    ///   <item>Calcular tiempos totales y promedios de procesamiento.</item>
    /// </list>
    /// </summary>
    /// <param name="year">Año a importar. Debe estar entre 2020 y el año en curso.</param>
    /// <param name="month">Mes a importar, de 1 a 12. No puede ser futuro.</param>
    /// <param name="idConfigSys">ID de la sucursal del sistema (<c>ConfigSys</c>) a la que se asociarán las ventas.</param>
    /// <param name="createdByUserId">Identificador del usuario que ejecuta la importación, para auditoría.</param>
    /// <param name="batchSize">Número de días por lote. Por defecto 5. Rango válido: 1–15.</param>
    /// <returns>
    ///   <see cref="ImportSalesResult"/> con estadísticas completas del proceso:
    ///   totales encontrados/importados/omitidos/con error, métricas de clientes,
    ///   tiempos de ejecución, resumen por lote (<see cref="BatchProcessingSummary"/>)
    ///   y detalle de errores (<see cref="ImportErrorDetail"/>).
    /// </returns>
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
    /// Valida los parámetros de importación antes de iniciar el proceso.
    /// Lanza <see cref="ArgumentException"/> si algún parámetro es inválido:
    /// año fuera de rango, mes fuera de rango o fecha en el futuro.
    /// </summary>
    /// <exception cref="ArgumentException">Si cualquier parámetro no cumple las restricciones.</exception>
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
    /// Divide un rango de fechas en sub-rangos (lotes) de <paramref name="batchSize"/> días.
    /// El último lote puede tener menos días si el rango no es múltiplo exacto.
    ///
    /// Ejemplo con batchSize=5 y mes de 31 días:
    ///   Lote 1: días 1-5 | Lote 2: días 6-10 | … | Lote 7: días 31-31
    /// </summary>
    /// <param name="startDate">Fecha de inicio del rango (primer día del mes).</param>
    /// <param name="endDate">Fecha de fin del rango (último día del mes).</param>
    /// <param name="batchSize">Número de días por lote.</param>
    /// <returns>Lista de tuplas (StartDate, EndDate) para cada lote.</returns>
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
    /// Procesa un lote de fechas específico dentro del flujo de importación mensual.
    ///
    /// PASOS:
    /// <list type="number">
    ///   <item>Obtiene ventas externas para el rango de fechas del lote.</item>
    ///   <item>Filtra las ventas cuyo folio ya existe en la BD (omitidas).</item>
    ///   <item>Pre-carga y/o crea los clientes necesarios en el cache compartido.</item>
    ///   <item>Itera sobre las ventas nuevas e invoca <see cref="ProcessSingleSaleAsync"/> para cada una.</item>
    ///   <item>Los errores individuales incrementan el contador <c>SalesError</c> sin detener el lote.</item>
    /// </list>
    /// </summary>
    /// <param name="batch">Tupla con la fecha de inicio y fin del lote.</param>
    /// <param name="idConfigSys">ID de la sucursal para asociar las ventas.</param>
    /// <param name="createdByUserId">Usuario que ejecuta la importación.</param>
    /// <param name="customerCache">Cache en memoria de clientes, compartido entre lotes para optimización.</param>
    /// <param name="batchNumber">Número secuencial del lote (para reportes).</param>
    /// <returns><see cref="BatchProcessingSummary"/> con contadores y tiempo del lote.</returns>
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
    /// Obtiene ventas del sistema externo InforAVA para un rango de fechas, consultando día a día.
    ///
    /// Itera desde <paramref name="startDate"/> hasta <paramref name="endDate"/> inclusive,
    /// haciendo una llamada al repositorio externo por cada día.
    /// Incluye una pausa de 500 ms entre días para no sobrecargar la API.
    /// Si una fecha falla, se ignora el error y se continúa con el siguiente día.
    ///
    /// Usa el catálogo fijo <c>"AVA01"</c>.
    /// </summary>
    /// <param name="startDate">Fecha de inicio del rango.</param>
    /// <param name="endDate">Fecha de fin del rango.</param>
    /// <returns>Lista acumulada de <see cref="ExternalSalesDto"/> encontradas en el rango.</returns>
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
    /// Consulta la BD para obtener los folios de ventas que ya fueron importados previamente.
    /// Se usa para filtrar duplicados antes de intentar insertar ventas nuevas.
    /// </summary>
    /// <param name="folios">Colección de folios a verificar.</param>
    /// <returns>
    ///   <see cref="HashSet{T}"/> con los folios ya existentes en la BD
    ///   (comparación sin distinción de mayúsculas/minúsculas).
    /// </returns>
    private async Task<HashSet<string>> GetExistingSaleFoliosAsync(IEnumerable<string> folios)
    {
        var existingSales = await _saleRepository.GetSalesByFoliosAsync(folios);
        return new HashSet<string>(existingSales.Select(s => s.Folio), StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Crea o recupera los clientes necesarios para el lote de ventas a importar,
    /// actualizando el cache compartido para evitar consultas redundantes.
    ///
    /// ESTRATEGIA EN TRES FASES:
    /// <list type="number">
    ///   <item>
    ///     <b>Pre-carga masiva:</b> extrae todos los <c>ExternalId</c> únicos del lote
    ///     y consulta la BD en una sola llamada.
    ///   </item>
    ///   <item>
    ///     <b>Resolución por lote:</b> para cada venta, si el cliente no está en cache,
    ///     busca primero en los pre-cargados por <c>ExternalId</c>; si no, hace un fallback
    ///     a búsqueda por nombre (<see cref="ICustomerRepository.FindByNameOrCodeAsync"/>).
    ///   </item>
    ///   <item>
    ///     <b>Creación de nuevos:</b> los clientes no encontrados se crean usando
    ///     <see cref="CreateBasicCustomerFromSaleDataAsync"/>. Si hay conflicto de
    ///     <c>IX_Customers_ExternalId</c>, reintenta recuperar el existente o usa
    ///     <see cref="CreateFallbackCustomerAsync"/> como último recurso.
    ///   </item>
    /// </list>
    /// </summary>
    /// <param name="sales">Ventas externas del lote actual.</param>
    /// <param name="customerCache">
    ///   Cache compartido entre lotes, indexado por <c>ExternalId</c> (string).
    ///   Se actualiza en esta llamada con los clientes encontrados o creados.
    /// </param>
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
    /// Crea un cliente de fallback con un <c>ExternalId</c> garantizado único,
    /// generado por el repositorio (<see cref="ICustomerRepository.GetNextExternalIdAsync"/>).
    /// Se usa como último recurso cuando la creación normal falla por conflictos de clave.
    /// Los campos opcionales como email, teléfono y dirección se dejan vacíos.
    /// </summary>
    /// <param name="customerName">Nombre del cliente a crear.</param>
    /// <returns>El nuevo <see cref="Customer"/> persistido en la BD.</returns>
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
    /// Crea un cliente básico a partir de los datos disponibles en una venta externa.
    ///
    /// Si <paramref name="useAlternativeExternalId"/> es <c>false</c>:
    /// - Intenta usar el <c>ExternalId</c> del sistema externo (campo <c>Cliente</c>).
    /// - Verifica primero que no exista ya un cliente con ese <c>ExternalId</c>; si existe, lo retorna directamente.
    ///
    /// Si <paramref name="useAlternativeExternalId"/> es <c>true</c>:
    /// - Genera un nuevo <c>ExternalId</c> único para evitar conflictos de índice.
    ///
    /// En caso de colisión de <c>IX_Customers_ExternalId</c> durante el insert,
    /// reintenta automáticamente con un nuevo <c>ExternalId</c>.
    /// </summary>
    /// <param name="sale">Datos de la venta externa con información del cliente.</param>
    /// <param name="useAlternativeExternalId">
    ///   Si es <c>true</c>, fuerza la generación de un <c>ExternalId</c> nuevo en lugar de
    ///   usar el del sistema externo. Útil para reintentos tras conflictos de unicidad.
    /// </param>
    /// <returns>El <see cref="Customer"/> creado o recuperado de la BD.</returns>
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
    /// Procesa e inserta en la BD una venta individual proveniente del sistema externo.
    ///
    /// PASOS:
    /// <list type="number">
    ///   <item>Recupera el cliente del cache (lanza excepción si no está).</item>
    ///   <item>Obtiene los detalles de productos vía <see cref="GetSaleProductDetailsAsync"/>.</item>
    ///   <item>Parsea la fecha y hora de la venta (<see cref="ParseSaleDateTime"/>).</item>
    ///   <item>Construye la entidad <see cref="Sale"/> con todos sus campos y el <see cref="AuxNoteDataJson"/>.</item>
    ///   <item>Persiste la venta usando el repositorio.</item>
    /// </list>
    ///
    /// Si el cliente no se encuentra en el cache se lanza <see cref="InvalidOperationException"/>,
    /// lo que hace que el lote registre esa venta como error sin interrumpir las demás.
    /// </summary>
    /// <param name="externalSale">Datos de la venta obtenidos del sistema externo.</param>
    /// <param name="idConfigSys">ID de la sucursal a asignar.</param>
    /// <param name="createdByUserId">Usuario que ejecuta la importación.</param>
    /// <param name="customerCache">Cache de clientes ya procesados en este flujo de importación.</param>
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
    /// Obtiene los detalles de productos (líneas) de una venta del sistema externo InforAVA.
    ///
    /// Requiere que la venta tenga los campos <c>NF</c>, <c>Caja</c>, <c>Serie</c> y <c>Folio</c>
    /// no vacíos para poder consultar el detalle. Si alguno falta, devuelve <c>null</c>.
    ///
    /// Cada línea se mapea a un <see cref="SingleProductJson"/> con:
    /// código + descripción concatenados, cantidad, precio unitario, total y unidad de medida.
    /// El campo <c>ProductId</c> queda en <c>null</c> ya que la vinculación con el catálogo
    /// local es opcional y no se realiza en este flujo.
    ///
    /// Si la consulta al repositorio externo falla por cualquier motivo, devuelve <c>null</c>
    /// (la venta se guarda sin líneas de detalle, no se interrumpe el proceso).
    /// </summary>
    /// <param name="externalSale">Datos de la venta externa con los campos de identificación.</param>
    /// <returns>Lista de <see cref="SingleProductJson"/> o <c>null</c> si no hay datos disponibles.</returns>
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
    /// Parsea la fecha y hora de una venta externa y los combina en un único <see cref="DateTime"/>.
    ///
    /// Si <paramref name="fecha"/> no puede interpretarse como fecha válida, se usa la fecha UTC actual.
    /// Si <paramref name="hora"/> no puede interpretarse como <see cref="TimeSpan"/> válido, se retorna
    /// la fecha sin componente de hora (medianoche).
    /// </summary>
    /// <param name="fecha">Fecha de la venta en formato libre (ej. "05/03/2026").</param>
    /// <param name="hora">Hora de la venta en formato HH:mm:ss (ej. "07:53:00").</param>
    /// <returns><see cref="DateTime"/> combinado con fecha y hora de la venta.</returns>
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
    /// Construye el objeto <see cref="AuxNoteDataJson"/> a partir de los datos de una venta
    /// obtenida del sistema externo InforAVA.
    ///
    /// Este objeto se almacena como JSONB en la columna <c>AuxNoteDataJson</c> de la tabla
    /// <c>Sales</c> y conserva todos los campos originales del sistema externo para
    /// trazabilidad y conciliación posterior.
    ///
    /// El campo <c>ExisteEnDB</c> se marca como <c>true</c> indicando que la venta fue
    /// importada desde el sistema fuente y existe un registro correspondiente allí.
    ///
    /// Nota: los campos <c>ImportePagado</c> y <c>Saldo</c> no se llenan en este método;
    /// se completan posteriormente mediante la importación de PAGADOS.xlsx
    /// (ver <see cref="ImportFromPagadosAsync"/>).
    /// </summary>
    /// <param name="externalSale">Datos de la venta externa.</param>
    /// <returns>Objeto <see cref="AuxNoteDataJson"/> listo para persistir.</returns>
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

    // ────────────────────────────────────────────────────────────────────────────
    // IMPORTACIÓN DESDE PAGADOS.xlsx
    // ────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Importa o actualiza ventas a partir del JSON generado desde el archivo <c>PAGADOS.xlsx</c>.
    ///
    /// PROPÓSITO:
    /// Permite registrar los montos pagados y saldos de ventas ya existentes (o crear nuevas entradas
    /// parciales) enriqueciendo el campo <c>AuxNoteDataJson.ImportePagado</c> y
    /// <c>AuxNoteDataJson.Saldo</c>, lo que posibilita calcular ganancias reales con mayor exactitud.
    ///
    /// ESTRUCTURA DE ENTRADA:
    /// El request (<see cref="ImportPagadosRequestDto"/>) replica la estructura del archivo Excel:
    /// <list type="bullet">
    ///   <item>Metadatos globales: nombre de archivo, fecha de proceso, totales.</item>
    ///   <item>Bloques (<see cref="ImportPagadosBlockDto"/>): agrupaciones de registros del Excel.</item>
    ///   <item>Registros (<see cref="ImportPagadosRecordDto"/>): una fila = una venta.</item>
    /// </list>
    ///
    /// FLUJO POR CADA REGISTRO:
    /// <list type="number">
    ///   <item>Parsear <c>Fecha</c> (formato <c>dd/MM/yyyy</c>). Si es inválida → omitir.</item>
    ///   <item>
    ///     Parsear <c>NombreCliente</c> con <see cref="ParseNombreCliente"/> para extraer
    ///     <c>ExternalId</c> y nombre del cliente.
    ///   </item>
    ///   <item>
    ///     <b>BUSCAR coincidencia:</b> ventas con mismo <c>Folio</c>, misma fecha y mismo
    ///     <c>IdConfigSys</c>, filtrando además por <c>Customer.ExternalId</c>.
    ///   </item>
    ///   <item>
    ///     <b>Si existe → ACTUALIZAR:</b> inyectar <c>ImportePagado</c> y <c>Saldo</c>
    ///     en <c>AuxNoteDataJson</c> y actualizar <c>UpdatedAt</c>.
    ///   </item>
    ///   <item>
    ///     <b>Si no existe → CREAR:</b> buscar o crear el cliente por <c>ExternalId</c>,
    ///     luego crear una venta nueva con datos parciales y <c>Type = "Imported-Pagados"</c>.
    ///     Los campos de nota externa (<c>Serie</c>, <c>Caja</c>, <c>Agente</c>, etc.) se
    ///     dejan vacíos para completarse con una importación posterior.
    ///   </item>
    /// </list>
    ///
    /// Los cambios se persisten en una sola llamada a <c>SaveChangesAsync</c> al final del proceso.
    /// </summary>
    /// <param name="request">JSON completo del archivo PAGADOS.xlsx pre-procesado.</param>
    /// <param name="idConfigSys">ID de la sucursal para filtrar y asociar ventas.</param>
    /// <param name="createdByUserId">
    ///   Identificador del usuario importador. Se usa como <c>SalesExecutive</c>
    ///   en las ventas creadas (tipo "Imported-Pagados").
    /// </param>
    /// <returns>
    ///   <see cref="ImportPagadosResultDto"/> con:
    ///   <c>TotalProcessed</c>, <c>TotalUpdated</c>, <c>TotalCreated</c>, <c>TotalSkipped</c>,
    ///   lista de errores y detalle de cada registro procesado.
    /// </returns>
    /// <inheritdoc />
    public async Task<ImportPagadosResultDto> ImportFromPagadosAsync(
        ImportPagadosRequestDto request,
        int idConfigSys,
        string createdByUserId)
    {
        var result = new ImportPagadosResultDto();

        // Aplanar todos los records de todos los bloques en una sola lista
        var allRecords = request.Blocks
            .SelectMany(b => b.Records)
            .ToList();

        foreach (var record in allRecords)
        {
            result.TotalProcessed++;
            try
            {
                // ── Parsear fecha (formato dd/MM/yyyy) ──────────────────────────
                if (!DateTime.TryParseExact(
                        record.Fecha,
                        "dd/MM/yyyy",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime saleDate))
                {
                    result.TotalSkipped++;
                    result.Errors.Add($"Folio {record.Folio}: Fecha inválida '{record.Fecha}'");
                    result.Details.Add(new ImportPagadosDetailDto
                    {
                        Folio = record.Folio,
                        Fecha = record.Fecha,
                        NombreCliente = record.NombreCliente,
                        Action = "Skipped",
                        Message = $"Fecha inválida: {record.Fecha}"
                    });
                    continue;
                }

                // ── Parsear nombreCliente → externalId + name ───────────────────
                var (externalId, customerName) = ParseNombreCliente(record.NombreCliente);

                // ── Buscar venta existente: Folio + Fecha + ConfigSys ───────────
                var existingSales = await _dbContext.Sales
                    .Include(s => s.Customer)
                    .Where(s => s.Folio == record.Folio
                             && s.SaleDate.Date == saleDate.Date
                             && s.IdConfigSys == idConfigSys)
                    .ToListAsync();

                // Filtrar por ExternalId del cliente
                var matchedSale = existingSales
                    .FirstOrDefault(s => s.Customer != null && s.Customer.ExternalId == externalId);

                if (matchedSale != null)
                {
                    // ✅ ACTUALIZAR: inyectar ImportePagado y Saldo en AuxNoteDataJson
                    matchedSale.AuxNoteDataJson ??= new AuxNoteDataJson();
                    matchedSale.AuxNoteDataJson.ImportePagado = record.ImportePagado;
                    matchedSale.AuxNoteDataJson.Saldo = record.Saldo;
                    matchedSale.UpdatedAt = DateTime.UtcNow;

                    _dbContext.Sales.Update(matchedSale);
                    result.TotalUpdated++;
                    result.Details.Add(new ImportPagadosDetailDto
                    {
                        Folio = record.Folio,
                        Fecha = record.Fecha,
                        NombreCliente = record.NombreCliente,
                        Action = "Updated",
                        Message = $"ImportePagado={record.ImportePagado}, Saldo={record.Saldo}"
                    });
                }
                else
                {
                    // 🆕 CREAR: buscar Customer por ExternalId + Name, luego sólo por ExternalId
                    var customer = await _dbContext.Customers
                        .FirstOrDefaultAsync(c => c.ExternalId == externalId && c.Name == customerName);

                    customer ??= await _dbContext.Customers
                        .FirstOrDefaultAsync(c => c.ExternalId == externalId);

                    if (customer == null)
                    {
                        customer = new Customer
                        {
                            ExternalId = externalId,
                            Name = customerName,
                            DirectionJson = new DirectionJson
                            {
                                Index = await _customerRepository.GetNextIndexForDirectionAsync()
                            }
                        };
                        _dbContext.Customers.Add(customer);
                        await _dbContext.SaveChangesAsync();
                    }

                    var newSale = new Sale
                    {
                        IdCustomer = customer.IdCustomer,
                        IdConfigSys = idConfigSys,
                        Folio = record.Folio,
                        SaleDate = saleDate,
                        TotalAmount = record.Importe,
                        // Guardamos el IdUser del token como referencia del importador
                        SalesExecutive = createdByUserId,
                        Type = "Imported-Pagados",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        AuxNoteDataJson = new AuxNoteDataJson
                        {
                            NombreCliente = record.NombreCliente,
                            Folio = record.Folio,
                            Fecha = record.Fecha,
                            Hora = record.Hora,
                            Importe = record.Importe,
                            ImportePagado = record.ImportePagado,
                            Saldo = record.Saldo,
                            // Campos vacíos para completar con importación posterior
                            Cliente = string.Empty,
                            Serie = string.Empty,
                            Caja = string.Empty,
                            Zn = string.Empty,
                            Nf = string.Empty,
                            Agente = string.Empty,
                            DireccionCliente = string.Empty,
                            PoblacionCliente = string.Empty,
                            EmailCliente = string.Empty,
                            TelCliente = string.Empty,
                            ExisteEnDB = false
                        }
                    };

                    _dbContext.Sales.Add(newSale);
                    result.TotalCreated++;
                    result.Details.Add(new ImportPagadosDetailDto
                    {
                        Folio = record.Folio,
                        Fecha = record.Fecha,
                        NombreCliente = record.NombreCliente,
                        Action = "Created",
                        Message = "Venta creada con datos parciales desde PAGADOS.xlsx"
                    });
                }
            }
            catch (Exception ex)
            {
                result.TotalSkipped++;
                result.Errors.Add($"Folio {record.Folio}: {ex.Message}");
                result.Details.Add(new ImportPagadosDetailDto
                {
                    Folio = record.Folio,
                    Fecha = record.Fecha,
                    NombreCliente = record.NombreCliente,
                    Action = "Skipped",
                    Message = ex.Message
                });
            }
        }

        // Guardar todos los cambios acumulados
        await _dbContext.SaveChangesAsync();
        return result;
    }

    /// <summary>
    /// Parsea el campo <c>NombreCliente</c> del archivo PAGADOS.xlsx, que viene en el formato
    /// <c>"000055 PUBLICO GENERAL"</c>, y extrae por separado el <c>ExternalId</c> numérico
    /// y el nombre del cliente.
    ///
    /// FORMATO ESPERADO: <c>"{código_con_ceros} {nombre completo}"</c>
    ///
    /// EJEMPLOS:
    /// <list type="table">
    ///   <item>
    ///     <term>"001078 CARLOS JONATHAN DEL RIVERO IRIGOYEN"</term>
    ///     <description>→ externalId=1078, name="CARLOS JONATHAN DEL RIVERO IRIGOYEN"</description>
    ///   </item>
    ///   <item>
    ///     <term>"000055 PUBLICO GENERAL"</term>
    ///     <description>→ externalId=55, name="PUBLICO GENERAL"</description>
    ///   </item>
    ///   <item>
    ///     <term>"006423 HUGO MAGAÑA LOPEZ 9812421639 SAMULA"</term>
    ///     <description>→ externalId=6423, name="HUGO MAGAÑA LOPEZ 9812421639 SAMULA"</description>
    ///   </item>
    /// </list>
    ///
    /// CASOS LÍMITE:
    /// <list type="bullet">
    ///   <item>Cadena vacía o nula → retorna (0, string.Empty).</item>
    ///   <item>Sin espacio → retorna (0, texto_completo).</item>
    ///   <item>Código no numérico → retorna externalId=0.</item>
    ///   <item>Código "000000" → externalId=0 (todos los ceros se eliminan).</item>
    /// </list>
    /// </summary>
    /// <param name="nombreCliente">Campo NombreCliente del registro PAGADOS.</param>
    /// <returns>Tupla con (<c>externalId</c> entero, <c>name</c> nombre limpio).</returns>
    private static (int externalId, string name) ParseNombreCliente(string nombreCliente)
    {
        if (string.IsNullOrWhiteSpace(nombreCliente))
            return (0, string.Empty);

        var trimmed = nombreCliente.Trim();
        var spaceIndex = trimmed.IndexOf(' ');

        if (spaceIndex <= 0)
            return (0, trimmed);

        var codePart = trimmed[..spaceIndex].TrimStart('0');
        var namePart = trimmed[(spaceIndex + 1)..].Trim();

        if (!int.TryParse(codePart, out int externalId))
            externalId = 0;

        return (externalId, namePart);
    }
}

