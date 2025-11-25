using System.Collections.Generic;
using System.Threading.Tasks;
using AVASphere.ApplicationCore.Common.Entities.Jsons;
using AVASphere.ApplicationCore.Sales.Entities;
using AVASphere.ApplicationCore.Sales.Interfaces;

namespace AVASphere.Infrastructure.Sales.Services
{
    public class SaleQuotationService : ISaleQuotationService
    {
        private readonly ISaleQuotationRepository _repo;
        private readonly ISaleRepository _saleRepository;
        private readonly IQuotationRepository _quotationRepository;


        public SaleQuotationService(ISaleQuotationRepository repo, ISaleRepository saleRepository, IQuotationRepository quotationRepository)
        {
            _repo = repo;
            _saleRepository = saleRepository;
            _quotationRepository = quotationRepository;
        }

        // GET: listar links para una venta (endpoint público)
        public async Task<IEnumerable<SaleQuotation>> GetQuotationsForSaleAsync(int saleId)
        {
            return await _repo.GetBySaleIdAsync(saleId);
        }

        public async Task<SaleQuotation?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        // DELETE: desvincular (endpoint público si lo necesitas)
        public async Task<bool> UnlinkQuotationFromSaleAsync(int saleId, int quotationId)
        {
            return await _repo.DeleteAsync(saleId, quotationId);
        }


        public async Task<bool> MarkPrimaryQuotationAsync(int idSale, int idQuotation, string createdBy)
        {
            // 1) Validar venta
            var sale = await _saleRepository.GetSaleByIdAsync(idSale);
            if (sale == null) return false;

            // 2) Validar cotización
            var quotation = await _quotationRepository.GetByIdAsync(idQuotation);
            if (quotation == null) return false;

            // 3) Verificar si ya existe el enlace
            if (await _repo.LinkExistsAsync(idSale, idQuotation))
                return true; // Ya existe → no duplicar

            // 4. Crear registro SaleQuotation
            var saleQuotation = new SaleQuotation
            {
                IdSale = idSale,
                IdQuotation = idQuotation,
                CreatedBy = createdBy,  // ✔️ OK
                CreatedAt = DateTime.UtcNow,
                GeneralComment = quotation.GeneralComment,

                // 3. Copiar productos desde quotation
                ProductsJson = quotation.ProductsJson?
                    .Select(p => new SingleProductJson
                    {
                        Unit = p.Unit,
                        Quantity = p.Quantity,
                        ProductId = p.ProductId,
                        UnitPrice = p.UnitPrice,
                        TotalPrice = p.TotalPrice,
                        Description = p.Description
                    }).ToList() ?? new List<SingleProductJson>(),

                // 4. Crear snapshot de precios desde quotation
                PriceSnapshot = new PriceSnapshotJson
                {
                    Subtotal = quotation.ProductsJson?.Sum(x => x.TotalPrice) ?? 0,
                    TaxAmount = 0, // Si tienes un campo de IVA real, cámbialo aquí.
                    TotalAmount = quotation.ProductsJson?.Sum(x => x.TotalPrice) ?? 0,
                    Currency = "MXN"
                }

            };

            // 5) Guardar con repositorio real
            await _repo.CreateAsync(saleQuotation);

            return true;
        }

    }
}