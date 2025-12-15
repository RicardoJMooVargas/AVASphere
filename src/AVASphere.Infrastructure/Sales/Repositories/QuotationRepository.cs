using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AVASphere.ApplicationCore.Sales.Entities;
using AVASphere.ApplicationCore.Sales.Interfaces;
using Microsoft.EntityFrameworkCore;
using AVASphere.Infrastructure;
using AVASphere.ApplicationCore.Common.Entities.Jsons;
using AVASphere.ApplicationCore.Sales.DTOs;

namespace AVASphere.Infrastructure.Sales.Repositories;


public class QuotationRepository : IQuotationRepository
{
    private readonly MasterDbContext _context;

    public QuotationRepository(MasterDbContext context)
    {
        _context = context;
    }

    //Lecturas
    public async Task<IEnumerable<Quotation>> GetAllQuotationsAsync()
    {
        return await _context.Quotations
           .AsNoTracking()
           .ToListAsync();
    }

    public async Task<Quotation?> UpdateIdQuotation(int id, QuotationUpdateDto dto)
    {
        var quotation = await _context.Quotations
            .FirstOrDefaultAsync(q => q.IdQuotation == id);

        if (quotation == null)
            return null;

        // 🔸 Solo actualiza si el campo tiene datos nuevos
        // Nota: El Folio en Quotation es int, pero en DTO es string? - no se actualiza aquí

        if (dto.Status.HasValue && dto.Status.Value != quotation.Status)
            quotation.Status = dto.Status.Value;

        if (!string.IsNullOrWhiteSpace(dto.GeneralComment) && dto.GeneralComment != quotation.GeneralComment)
            quotation.GeneralComment = dto.GeneralComment;

        if (dto.IdConfigSys.HasValue && dto.IdConfigSys.Value != quotation.IdConfigSys)
            quotation.IdConfigSys = dto.IdConfigSys.Value;

        if (dto.SalesExecutives != null && dto.SalesExecutives.Any())
            quotation.SalesExecutives = dto.SalesExecutives;
        else
            quotation.SalesExecutives ??= new List<string>(); // asegura que no sea null

        // 🔹 FOLLOWUPS
        bool hasValidFollowups = dto.FollowupsToAdd != null &&
            dto.FollowupsToAdd.Any(f =>
                (!string.IsNullOrWhiteSpace(f.Comment) && f.Comment != "string") ||
                (!string.IsNullOrWhiteSpace(f.UserId) && f.UserId != "string"));

        if (hasValidFollowups)
        {
            quotation.FollowupsJson = dto.FollowupsToAdd.Select(f => new QuotationFollowupsJson
            {
                Date = f.Date,
                Comment = f.Comment,
                UserId = f.UserId,
                CreatedAt = DateTime.UtcNow
            }).ToList();
        }
        else
        {
            // ❌ Mantén los followups existentes
            quotation.FollowupsJson = quotation.FollowupsJson;
        }

        // 🔹 PRODUCTS
        bool hasValidProducts = dto.Products != null &&
            dto.Products.Any(p =>
                p.ProductId != 0 ||
                p.Quantity != 0 ||
                (!string.IsNullOrWhiteSpace(p.Description) && p.Description != "string"));

        if (hasValidProducts)
        {
            quotation.ProductsJson = dto.Products.Select(p => new SingleProductJson
            {
                ProductId = p.ProductId,
                Quantity = p.Quantity,
                Description = p.Description,
                UnitPrice = p.UnitPrice,
                TotalPrice = p.TotalPrice,
                Unit = p.Unit
            }).ToList();
        }
        else
        {
            // ❌ Mantén los productos existentes
            quotation.ProductsJson = quotation.ProductsJson;
        }

        quotation.UpdatedAt = DateTime.UtcNow;

        _context.Quotations.Update(quotation);
        await _context.SaveChangesAsync();

        return quotation;
    }
    // Actualiza/guarda la entidad Quotation completa (usado tras modificar la entidad en memoria)
    public async Task<Quotation> UpdateQuotationAsync(Quotation quotation)
    {
        if (quotation is null) throw new ArgumentNullException(nameof(quotation));

        var tracked = await _context.Quotations
            .FirstOrDefaultAsync(q => q.IdQuotation == quotation.IdQuotation);

        if (tracked == null)
            throw new KeyNotFoundException($"Quotation with ID {quotation.IdQuotation} not found.");

        // Copiamos valores escalares
        _context.Entry(tracked).CurrentValues.SetValues(quotation);

        // Reemplazamos colecciones explícitamente (o ajusta según tu estrategia)
        tracked.FollowupsJson = quotation.FollowupsJson;
        tracked.SalesExecutives = quotation.SalesExecutives;
        tracked.ProductsJson = quotation.ProductsJson;

        tracked.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return tracked;
    }
    public async Task<Quotation?> GetQuotationByFolioAsync(int folio)
    {
        return await _context.Quotations
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Folio == folio);
    }

    public async Task<IEnumerable<Quotation>> GetQuotationsByCustomerIdAsync(int customerId)
    {
        return await _context.Quotations
            .AsNoTracking()
            .Where(q => q.IdCustomer == customerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Quotation>> GetQuotationsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var startDateOnly = DateOnly.FromDateTime(startDate);
        var endDateOnly = DateOnly.FromDateTime(endDate);

        return await _context.Quotations
            .AsNoTracking()
            .Where(q => q.SaleDate >= startDateOnly && q.SaleDate <= endDateOnly)
            .ToListAsync();
    }

    // Esenciales
    public async Task<Quotation> CreateQuotationAsync(Quotation quotation)
    {
        if (quotation is null) throw new ArgumentNullException(nameof(quotation));
        _context.Quotations.Add(quotation);
        await _context.SaveChangesAsync();
        return quotation;
    }

    // 🔹 Obtener una cotización por ID (sin modificar)
    public async Task<Quotation?> GetByIdAsync(int id)
    {
        return await _context.Quotations
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.IdQuotation == id);
    }

    public async Task<bool> DeleteQuotationAsync(int id)
    {
        var quotation = await _context.Quotations
            .Include(q => q.Versions) // Importante para asegurar el tracking
            .FirstOrDefaultAsync(q => q.IdQuotation == id);

        if (quotation == null) return false;

        _context.Quotations.Remove(quotation);
        await _context.SaveChangesAsync();
        return true;
    }

    // Helpers / checks
    public async Task<bool> QuotationExistsByFolioAsync(int folio)
    {
        return await _context.Quotations.AnyAsync(q => q.Folio == folio);
    }

    public async Task<int> GetNextFollowupIdAsync(int quotationId)
    {
        var quotation = await _context.Quotations
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.IdQuotation == quotationId);

        if (quotation?.FollowupsJson == null || !quotation.FollowupsJson.Any())
        {
            return 1;
        }

        // Ordenamos por ID descendente y tomamos el último
        var lastFollowup = quotation.FollowupsJson
            .OrderByDescending(f => f.Id)
            .FirstOrDefault();

        // Incrementamos el último ID en 1
        return (lastFollowup?.Id ?? 0) + 1;
    }
}