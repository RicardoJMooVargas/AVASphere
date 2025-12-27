using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AVASphere.ApplicationCore.Common.Entities.General;
using AVASphere.ApplicationCore.Common.Entities.Jsons;
using AVASphere.ApplicationCore.Common.Extensions;
using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.ApplicationCore.Sales.DTOs;
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
    private readonly ISaleQuotationService _saleQuotationService;

    public SaleService(
        ISaleRepository saleRepository,
        IQuotationRepository quotationRepository,
        ICustomerRepository customerRepository,
        MasterDbContext dbContext,
        IExternalSalesService externalSalesService,
        ISaleQuotationService saleQuotationService)
    {
        _saleRepository = saleRepository;
        _quotationRepository = quotationRepository;
        _dbContext = dbContext;
        _customerRepository = customerRepository;
        _externalSalesService = externalSalesService;
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
}