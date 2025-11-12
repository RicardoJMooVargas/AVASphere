using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AVASphere.ApplicationCore.Sales.DTOs;
using AVASphere.ApplicationCore.Sales.Entities;
using AVASphere.ApplicationCore.Sales.Interfaces;
using AVASphere.ApplicationCore.Common.Entities.Jsons;
using Microsoft.EntityFrameworkCore;

namespace AVASphere.Infrastructure.Sales.Services;

public class QuotationService : IQuotationService
{

    private readonly IQuotationRepository _quotationRepository;
    private readonly IQuotationVersionRepository _versionRepository;


    public QuotationService(IQuotationRepository quotationRepository, IQuotationVersionRepository versionRepository)
    {
        _quotationRepository = quotationRepository ?? throw new ArgumentNullException(nameof(quotationRepository));
        _versionRepository = versionRepository ?? throw new ArgumentNullException(nameof(versionRepository));
    }

    public async Task<Quotation> CreateQuotationAsync(CreateQuotationDto createQuotationDto, string createdByUserId)
    {
        if (createQuotationDto is null) throw new ArgumentNullException(nameof(createQuotationDto));
        if (string.IsNullOrEmpty(createdByUserId)) throw new ArgumentException("El ID del usuario creador es requerido.", nameof(createdByUserId));

        var quotation = new Quotation
        {
            Folio = createQuotationDto.Folio,
            SaleDate = createQuotationDto.SaleDate ?? DateTime.UtcNow,
            Status = createQuotationDto.Status ?? "PENDIENTE",
            GeneralComment = createQuotationDto.GeneralComment,
            CustomerId = createQuotationDto.CustomerId,
            SalesExecutives = createQuotationDto.SalesExecutives ?? new List<string> { createdByUserId },
            Followups = new List<QuotationFollowupsJson>(), // Inicializamos la lista vacía
            Products = createQuotationDto.Products,
            IdConfigSys = createQuotationDto.IdConfigSys,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (quotation.Folio != 0)
        {
            var exists = await _quotationRepository.QuotationExistsByFolioAsync(quotation.Folio);
            if (exists) throw new InvalidOperationException($"Folio {quotation.Folio} already exists.");
        }

        // Primero creamos la cotización
        var createdQuotation = await _quotationRepository.CreateQuotationAsync(quotation);

        // Si hay followups iniciales, los agregamos uno por uno para asignar IDs incrementales
        if (createQuotationDto.Followups != null && createQuotationDto.Followups.Any())
        {
            foreach (var followupDto in createQuotationDto.Followups)
            {
                var followup = new QuotationFollowupsJson
                {
                    Id = await _quotationRepository.GetNextFollowupIdAsync(createdQuotation.QuotationId),
                    Date = followupDto.Date ?? DateTime.UtcNow,
                    Comment = followupDto.Comment,
                    UserId = followupDto.UserId ?? createdByUserId,
                    CreatedAt = DateTime.UtcNow
                };
                createdQuotation.Followups.Add(followup);
            }
            // Actualizamos la cotización con los followups
            createdQuotation.UpdatedAt = DateTime.UtcNow;
            await _quotationRepository.UpdateQuotationAsync(createdQuotation);
        }

        // --- NUEVO: crear QuotationVersion automáticamente si hay productos/total ---
        try
        {
            var hasProducts = createQuotationDto.Products != null;
            // o validar que createQuotationDto.Products tenga elementos según tipo
            if (hasProducts)
            {
                var version = new QuotationVersion
                {
                    IdQuotation = createdQuotation.QuotationId,
                    VersionNumber = await _versionRepository.GetNextVersionNumberAsync(createdQuotation.QuotationId),
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = createdByUserId,
                    // Ajusta la asignación según tu propiedad en QuotationVersion (ProductsJson/Products/etc.)
                    Products = createQuotationDto.Products?.ToList() ?? new List<SingleProductJson>()
                };

                await _versionRepository.CreateAsync(version);
            }
        }
        catch (Exception ex)
        {
            // Loguea o maneja según tu política. Por ahora lanzamos para que el fallo sea visible.
            throw new Exception($"Error al crear QuotationVersion para QuotationId={createdQuotation.QuotationId}: {ex.Message}", ex);
        }

        return createdQuotation;
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

    // 🔹 Obtener por ID (solo lectura)
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
        quotation.Followups ??= new List<QuotationFollowupsJson>();

        // 🔹 Agregar el nuevo followup
        quotation.Followups.Add(followup);

        quotation.UpdatedAt = DateTime.UtcNow;

        // 🔹 Guardar cambios
        await _quotationRepository.UpdateQuotationAsync(quotation);

        return true;
    }

    public async Task<bool> DeleteFollowupFromQuotationAsync(int IdQuotation, int followupId)
    {
        var quotation = await _quotationRepository.GetByIdAsync(IdQuotation);
        if (quotation == null || quotation.Followups == null) return false;
        var removed = quotation.Followups.RemoveAll(f => f.Id == followupId) > 0;
        if (!removed) return false;
        quotation.UpdatedAt = DateTime.UtcNow;
        await _quotationRepository.UpdateQuotationAsync(quotation);
        return true;
    }

}