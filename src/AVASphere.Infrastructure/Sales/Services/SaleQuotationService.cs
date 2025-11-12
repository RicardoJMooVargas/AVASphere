using System.Collections.Generic;
using System.Threading.Tasks;
using AVASphere.ApplicationCore.Sales.Entities;
using AVASphere.ApplicationCore.Sales.Interfaces;

namespace AVASphere.Infrastructure.Sales.Services
{
    public class SaleQuotationService : ISaleQuotationService
    {
        private readonly ISaleQuotationRepository _repo;

        public SaleQuotationService(ISaleQuotationRepository repo)
        {
            _repo = repo;
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


        public async Task<bool> MarkPrimaryQuotationAsync(int saleId, int quotationId, string requestedByUserId)
        {
            // Implementación: validar existencia del quotation vinculado a la venta.
            // Nota: la propiedad `IsPrimary` ya no se utiliza, por lo que no se realizan asignaciones ni llamadas a UpdateAsync aquí.
            var target = (await _repo.GetByQuotationIdAsync(quotationId)).FirstOrDefault(sq => sq.IdSale == saleId);
            if (target == null) return false;

            // Si en el futuro se requiere persistir un cambio de 'principalidad', implementar un método específico en el repositorio.
            return true;
        }
    }
}