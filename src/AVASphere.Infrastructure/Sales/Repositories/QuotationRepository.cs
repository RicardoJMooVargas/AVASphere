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
            .FirstOrDefaultAsync(q => q.QuotationId == id);

        if (quotation == null)
            return null;

        // 🔸 Solo actualiza si el campo tiene datos nuevos
        if (dto.Folio != 0 && dto.Folio != quotation.Folio)
            quotation.Folio = dto.Folio;

        if (!string.IsNullOrWhiteSpace(dto.Status) && dto.Status != quotation.Status)
            quotation.Status = dto.Status;

        if (!string.IsNullOrWhiteSpace(dto.GeneralComment) && dto.GeneralComment != quotation.GeneralComment)
            quotation.GeneralComment = dto.GeneralComment;

        if (dto.CustomerId != 0 && dto.CustomerId != quotation.CustomerId)
            quotation.CustomerId = dto.CustomerId;

        if (dto.IdConfigSys != 0 && dto.IdConfigSys != quotation.IdConfigSys)
            quotation.IdConfigSys = dto.IdConfigSys;

        if (dto.SalesExecutives != null && dto.SalesExecutives.Any())
            quotation.SalesExecutives = dto.SalesExecutives;
        else
            quotation.SalesExecutives ??= new List<string>(); // asegura que no sea null

        // 🔹 FOLLOWUPS
        bool hasValidFollowups = dto.Followups != null &&
            dto.Followups.Any(f =>
                (!string.IsNullOrWhiteSpace(f.Comment) && f.Comment != "string") ||
                (!string.IsNullOrWhiteSpace(f.UserId) && f.UserId != "string"));

        if (hasValidFollowups)
        {
            quotation.Followups = dto.Followups.Select(f => new QuotationFollowupsJson
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
            quotation.Followups = quotation.Followups;
        }

        // 🔹 PRODUCTS
        bool hasValidProducts = dto.Products != null &&
            dto.Products.Any(p =>
                p.ProductId != 0 ||
                p.Quantity != 0 ||
                (!string.IsNullOrWhiteSpace(p.Description) && p.Description != "string"));

        if (hasValidProducts)
        {
            quotation.Products = dto.Products.Select(p => new SingleProductJson
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
            quotation.Products = quotation.Products;
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
            .FirstOrDefaultAsync(q => q.QuotationId == quotation.QuotationId);

        if (tracked == null)
            throw new KeyNotFoundException($"Quotation with ID {quotation.QuotationId} not found.");

        // Copiamos valores escalares
        _context.Entry(tracked).CurrentValues.SetValues(quotation);

        // Reemplazamos colecciones explícitamente (o ajusta según tu estrategia)
        tracked.Followups = quotation.Followups;
        tracked.SalesExecutives = quotation.SalesExecutives;
        tracked.Products = quotation.Products;

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
            .Where(q => q.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Quotation>> GetQuotationsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Quotations
            .AsNoTracking()
            .Where(q => q.SaleDate >= startDate && q.SaleDate <= endDate)
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
            .FirstOrDefaultAsync(q => q.QuotationId == id);
    }

    public async Task<bool> DeleteQuotationAsync(int id)
    {
        var entity = await _context.Quotations.FindAsync(id);
        if (entity == null) return false;
        _context.Quotations.Remove(entity);
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
            .FirstOrDefaultAsync(q => q.QuotationId == quotationId);

        if (quotation?.Followups == null || !quotation.Followups.Any())
        {
            return 1;
        }

        // Ordenamos por ID descendente y tomamos el último
        var lastFollowup = quotation.Followups
            .OrderByDescending(f => f.Id)
            .FirstOrDefault();

        // Incrementamos el último ID en 1
        return (lastFollowup?.Id ?? 0) + 1;
    }
}