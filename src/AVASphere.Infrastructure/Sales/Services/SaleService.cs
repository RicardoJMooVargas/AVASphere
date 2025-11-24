
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

namespace VYAACentralInforApi.ApplicationCore.Sales.Services;

public class SaleService : ISaleService
{
    private readonly ISaleRepository _saleRepository;
    private readonly IQuotationRepository _quotationRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly MasterDbContext _dbContext;

    public SaleService(ISaleRepository saleRepository, IQuotationRepository quotationRepository, ICustomerRepository customerRepository, MasterDbContext dbContext)
    {
        _saleRepository = saleRepository;
        _quotationRepository = quotationRepository;
        _dbContext = dbContext;
        _customerRepository = customerRepository;
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
}