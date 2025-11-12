
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AVASphere.ApplicationCore.Sales.Entities;
using AVASphere.ApplicationCore.Sales.Interfaces;
using AVASphere.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace VYAACentralInforApi.ApplicationCore.Sales.Services;

public class SaleService : ISaleService
{
    private readonly ISaleRepository _saleRepository;
    private readonly IQuotationRepository _quotationRepository;
    private readonly MasterDbContext _dbContext;

    public SaleService(ISaleRepository saleRepository, IQuotationRepository quotationRepository, MasterDbContext dbContext)
    {
        _saleRepository = saleRepository;
        _quotationRepository = quotationRepository;
        _dbContext = dbContext;
    }

    public async Task<Sale> CreateSaleAsync(Sale sale, string createdByUserId)
    {
        if (sale is null) throw new ArgumentNullException(nameof(sale));
        sale.CreatedAt = DateTime.UtcNow;
        sale.UpdatedAt = DateTime.UtcNow;
        var created = await _saleRepository.CreateSaleAsync(sale);
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

                quotation.LinkedSaleId = createdSale.SaleId.ToString();
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
}