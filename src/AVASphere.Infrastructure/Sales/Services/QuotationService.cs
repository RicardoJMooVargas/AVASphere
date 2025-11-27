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
            // 2️⃣ Si NO envió IdCustomer → revisar NewCustomers
            if (dto.NewCustomers == null || !dto.NewCustomers.Any())
                throw new Exception("No se envió un IdCustomer válido ni datos de NewCustomer.");

            var nc = dto.NewCustomers.First();

            // Validaciones mínimas
            if (string.IsNullOrWhiteSpace(nc.Name))
                throw new Exception("El nombre del cliente es obligatorio para crear uno nuevo.");

            // 3️⃣ Crear nuevo customer
            customer = new Customer
            {
                ExternalId = 0,
                Name = nc.Name,
                Email = nc.Email,
                PhoneNumber = string.IsNullOrWhiteSpace(nc.Phone) ? "+00" : nc.Phone,
                DirectionJson = new DirectionJson { Colony = nc.Direction },
                SettingsCustomerJson = new SettingsCustomerJson { Index = 1, Type = "General" }
            };

            customer = await _customerRepository.InsertAsync(customer);
        }

        // 4️⃣ Crear Quotation
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

        // 5️⃣ Agregar followups (opcional)
        if (dto.Followups != null && dto.Followups.Any())
        {
            foreach (var f in dto.Followups)
            {
                createdQuotation.FollowupsJson.Add(new QuotationFollowupsJson
                {
                    Id = await _quotationRepository.GetNextFollowupIdAsync(createdQuotation.IdQuotation),
                    Date = f.Date ?? DateTime.UtcNow,
                    Comment = f.Comment,
                    UserId = f.UserId ?? createdByUserId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _quotationRepository.UpdateQuotationAsync(createdQuotation);
        }

        // 6️⃣ Crear versión
        if (dto.Products != null && dto.Products.Any())
        {
            var version = new QuotationVersion
            {
                IdQuotation = createdQuotation.IdQuotation,
                VersionNumber = await _versionRepository.GetNextVersionNumberAsync(createdQuotation.IdQuotation),
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.UtcNow,
                ProductsJson = dto.Products.ToList(),
                // NO establecer Quotation ni QuotationDataJson aquí (evita ciclos)
                Quotation = null,
                QuotationDataJson = null
            };
            await _versionRepository.CreateAsync(version);
        }

        // 5) Releer la cotización con versiones (para retorno coherente)
        var result = await _quotationRepository.GetByIdAsync(createdQuotation.IdQuotation);

        // Asegurar null-check para evitar CS8603
        if (result == null)
            throw new Exception($"Error inesperado: la cotización {createdQuotation.IdQuotation} no pudo ser recuperada después de ser creada.");

        // 6) SANITIZAR: eliminar referencias circulares antes de devolver
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

        return result;  // <--- Ya NO marca warning

    }
    
    public async Task<IEnumerable<Quotation>> GetQuotationsAsync(DateTime? startDate = null, DateTime? endDate = null, string? customerName = null, int? folio = null)
    {
        if (folio.HasValue)
        {
            var q = await _quotationRepository.GetQuotationByFolioAsync(folio.Value);
            return q != null ? new[] { q } : Array.Empty<Quotation>();
        }

        if (startDate.HasValue && endDate.HasValue)
        {
            return await _quotationRepository.GetQuotationsByDateRangeAsync(startDate.Value, endDate.Value);
        }

        return await _quotationRepository.GetAllQuotationsAsync();
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