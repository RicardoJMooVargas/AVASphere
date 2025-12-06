using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AVASphere.ApplicationCore.Sales.DTOs;
using AVASphere.ApplicationCore.Sales.Entities;
using AVASphere.ApplicationCore.Sales.Interfaces;
using AVASphere.ApplicationCore.Common.Entities.Jsons;
using Microsoft.EntityFrameworkCore;
using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.ApplicationCore.Common.Entities.General;
using AVASphere.ApplicationCore.Sales;


namespace AVASphere.Infrastructure.Sales.Services;

public class QuotationService : IQuotationService
{

    private readonly IQuotationRepository _quotationRepository;
    private readonly IQuotationVersionRepository _versionRepository;
    private readonly ICustomerRepository _customerRepository;


    public QuotationService(IQuotationRepository quotationRepository, IQuotationVersionRepository versionRepository, ICustomerRepository customerRepository)
    {
        _quotationRepository = quotationRepository;
        _versionRepository = versionRepository;
        _customerRepository = customerRepository;
    }

    public async Task<Quotation> CreateQuotationAsync(CreateQuotationDto dto, string createdByUserId)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        // ✅ Validar que el folio no exista
        var folioExists = await _quotationRepository.QuotationExistsByFolioAsync(dto.Folio);
        if (folioExists)
            throw new Exception($"Ya existe una cotización con el folio {dto.Folio}. Por favor, use un folio diferente.");

        Customer? customer = null;

        // 1️⃣ Si envía IdCustomer válido, usarlo
        if (dto.CustomerId > 0)
        {
            customer = await _customerRepository.GetByIdAsync(dto.CustomerId);
            if (customer == null)
                throw new Exception($"IdCustomer {dto.CustomerId} no existe.");
        }
        else
        {
            // 2️⃣ Si NO envió IdCustomer → revisar NewCustomers o crear cliente genérico
            if (dto.NewCustomers != null && dto.NewCustomers.Any())
            {
                var nc = dto.NewCustomers.First();

                // Validaciones mínimas
                if (string.IsNullOrWhiteSpace(nc.Name))
                    throw new Exception("El nombre del cliente es obligatorio para crear uno nuevo.");

                // 3️⃣ Crear nuevo customer con ExternalId auto-generado
                var nextExternalId = await _customerRepository.GetNextExternalIdAsync();
                
                customer = new Customer
                {
                    ExternalId = nextExternalId,
                    Name = nc.Name,
                    Email = nc.Email,
                    PhoneNumber = string.IsNullOrWhiteSpace(nc.Phone) ? "+00" : nc.Phone,
                    DirectionJson = new DirectionJson { Colony = nc.Direction },
                    SettingsCustomerJson = new SettingsCustomerJson { Index = 1, Type = "General" }
                };

                customer = await _customerRepository.InsertAsync(customer);
            }
            else
            {
                // 4️⃣ Crear cliente genérico si no se proporcionaron datos
                var nextExternalId = await _customerRepository.GetNextExternalIdAsync();
                
                customer = new Customer
                {
                    ExternalId = nextExternalId,
                    Name = "Cliente",
                    LastName = "Genérico",
                    Email = null,
                    PhoneNumber = "+00",
                    DirectionJson = new DirectionJson { Index = 1 },
                    SettingsCustomerJson = new SettingsCustomerJson { Index = 1, Type = "General" }
                };

                customer = await _customerRepository.InsertAsync(customer);
            }
        }

        // 5️⃣ Crear Quotation
        var quotation = new Quotation
        {
            Folio = dto.Folio,
            SaleDate = dto.SaleDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            Status = dto.Status ?? StatusEnum.Pending,
            GeneralComment = dto.GeneralComment,
            IdCustomer = customer.IdCustomer,
            SalesExecutives = dto.SalesExecutives ?? new List<string> { createdByUserId },
            FollowupsJson = new List<QuotationFollowupsJson>(),
            ProductsJson = dto.Products,
            IdConfigSys = dto.IdConfigSys,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdQuotation = await _quotationRepository.CreateQuotationAsync(quotation);

        // 6️⃣ Agregar followups iniciales (opcional)
        if (dto.Followups != null && dto.Followups.Any())
        {
            int followupIdCounter = 1;
            foreach (var f in dto.Followups)
            {
                createdQuotation.FollowupsJson.Add(new QuotationFollowupsJson
                {
                    Id = followupIdCounter++,
                    Date = f.Date ?? DateTime.UtcNow,
                    Comment = f.Comment,
                    UserId = f.UserId ?? createdByUserId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _quotationRepository.UpdateQuotationAsync(createdQuotation);
        }

        // 7️⃣ Crear versión inicial si hay productos
        if (dto.Products != null && dto.Products.Any())
        {
            var version = new QuotationVersion
            {
                IdQuotation = createdQuotation.IdQuotation,
                VersionNumber = 1,
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.UtcNow,
                ProductsJson = dto.Products.ToList(),
                Quotation = null,
                QuotationDataJson = null
            };
            await _versionRepository.CreateAsync(version);
        }

        // 8️⃣ Releer la cotización con versiones
        var result = await _quotationRepository.GetByIdAsync(createdQuotation.IdQuotation);

        if (result == null)
            throw new Exception($"Error inesperado: la cotización {createdQuotation.IdQuotation} no pudo ser recuperada después de ser creada.");

        // 9️⃣ SANITIZAR: eliminar referencias circulares antes de devolver
        result.Customer = null;
        result.ConfigSys = null;

        if (result.Versions != null)
        {
            foreach (var v in result.Versions)
            {
                v.Quotation = null;
                v.QuotationDataJson = null;
            }
        }

        return result;
    }
    
    public async Task<IEnumerable<GetQuotationResponseDto>> GetQuotationsAsync(DateTime? startDate = null, DateTime? endDate = null, string? customerName = null, int? folio = null)
    {
        IEnumerable<Quotation> quotations;
        
        if (folio.HasValue)
        {
            var q = await _quotationRepository.GetQuotationByFolioAsync(folio.Value);
            quotations = q != null ? new[] { q } : Array.Empty<Quotation>();
        }
        else if (startDate.HasValue && endDate.HasValue)
        {
            quotations = await _quotationRepository.GetQuotationsByDateRangeAsync(startDate.Value, endDate.Value);
        }
        else
        {
            quotations = await _quotationRepository.GetAllQuotationsAsync();
        }

        // Mapear a DTO incluyendo datos del cliente
        var quotationDtos = new List<GetQuotationResponseDto>();
        
        foreach (var quotation in quotations)
        {
            // Obtener datos del cliente si no están cargados
            if (quotation.Customer == null && quotation.IdCustomer > 0)
            {
                quotation.Customer = await _customerRepository.GetByIdAsync(quotation.IdCustomer);
            }

            var dto = new GetQuotationResponseDto
            {
                IdQuotation = quotation.IdQuotation,
                Folio = quotation.Folio,
                SaleDate = quotation.SaleDate,
                Status = quotation.Status,
                GeneralComment = quotation.GeneralComment,
                SalesExecutives = quotation.SalesExecutives ?? new List<string>(),
                Products = quotation.ProductsJson,
                LinkedSaleId = quotation.LinkedSaleId,
                LinkedSaleFolio = quotation.LinkedSaleFolio,
                IsLinkedToSale = quotation.IsLinkedToSale,
                CreatedAt = quotation.CreatedAt,
                UpdatedAt = quotation.UpdatedAt,
                IdConfigSys = quotation.IdConfigSys,
                Customer = quotation.Customer != null ? new CustomerInQuotationDto
                {
                    IdCustomer = quotation.Customer.IdCustomer,
                    ExternalId = quotation.Customer.ExternalId,
                    Name = quotation.Customer.Name,
                    LastName = quotation.Customer.LastName,
                    PhoneNumber = quotation.Customer.PhoneNumber,
                    Email = quotation.Customer.Email,
                    TaxId = quotation.Customer.TaxId,
                    Direction = quotation.Customer.DirectionJson,
                    Settings = quotation.Customer.SettingsCustomerJson,
                    PaymentMethods = quotation.Customer.PaymentMethodsJson,
                    PaymentTerms = quotation.Customer.PaymentTermsJson
                } : new CustomerInQuotationDto(),
                Followups = quotation.FollowupsJson?.Select(f => new QuotationFollowupResponseDto
                {
                    Id = f.Id,
                    Date = f.Date,
                    Comment = f.Comment,
                    UserId = f.UserId,
                    CreatedAt = f.CreatedAt
                }).ToList() ?? new List<QuotationFollowupResponseDto>()
            };

            // Filtrar por nombre de cliente si se especificó
            if (!string.IsNullOrWhiteSpace(customerName) && dto.Customer != null)
            {
                var fullName = dto.Customer.FullName.ToLowerInvariant();
                if (!fullName.Contains(customerName.ToLowerInvariant()))
                    continue;
            }

            quotationDtos.Add(dto);
        }

        return quotationDtos;
    }

    public async Task<Quotation?> UpdateIdQuotation(int IdQuotation, QuotationUpdateDto dto)
    {
        return await _quotationRepository.UpdateIdQuotation(IdQuotation, dto);
    }

    public async Task<Quotation> GetByIdAsync(int IdQuotation)
    {
        var quotation = await _quotationRepository.GetByIdAsync(IdQuotation);
        if (quotation == null)
            throw new KeyNotFoundException($"Quotation with ID {IdQuotation} not found.");
        return quotation;
    }

    public async Task<bool> DeleteQuotationAsync(int id)
    {
        return await _quotationRepository.DeleteQuotationAsync(id);
    }

    public async Task<bool> AddFollowupAsync(int quotationId, QuotationFollowupsJson followup, string userId)
    {
        if (followup is null)
            throw new ArgumentNullException(nameof(followup));

        // 🔹 Configurar los valores del nuevo followup
        followup.UserId = userId ?? string.Empty;
        followup.CreatedAt = DateTime.UtcNow;
        followup.Date = followup.Date == default ? DateTime.UtcNow : followup.Date;
        followup.Id = await _quotationRepository.GetNextFollowupIdAsync(quotationId);

        // 🔹 Obtener la cotización actual
        var quotation = await _quotationRepository.GetByIdAsync(quotationId);
        if (quotation == null)
            return false;

        // 🔹 Inicializar la lista si está vacía
        quotation.FollowupsJson ??= new List<QuotationFollowupsJson>();

        // 🔹 Agregar el nuevo followup
        quotation.FollowupsJson.Add(followup);

        quotation.UpdatedAt = DateTime.UtcNow;

        // 🔹 Guardar cambios
        await _quotationRepository.UpdateQuotationAsync(quotation);

        return true;
    }

    public async Task<bool> DeleteFollowupFromQuotationAsync(int IdQuotation, int followupId)
    {
        var quotation = await _quotationRepository.GetByIdAsync(IdQuotation);
        if (quotation == null || quotation.FollowupsJson == null) return false;
        var removed = quotation.FollowupsJson.RemoveAll(f => f.Id == followupId) > 0;
        if (!removed) return false;
        quotation.UpdatedAt = DateTime.UtcNow;
        await _quotationRepository.UpdateQuotationAsync(quotation);
        return true;
    }

}