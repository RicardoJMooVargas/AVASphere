using System.Collections.Generic;
using System.Threading.Tasks;
using AVASphere.ApplicationCore.Common.Entities.Jsons;
using AVASphere.ApplicationCore.Sales.DTOs;
using AVASphere.ApplicationCore.Sales.Entities;
using AVASphere.ApplicationCore.Sales.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AVASphere.Infrastructure.Sales.Services
{
    public class SaleQuotationService : ISaleQuotationService
    {
        private readonly ISaleQuotationRepository _repo;
        private readonly ISaleRepository _saleRepository;
        private readonly IQuotationRepository _quotationRepository;
        private readonly MasterDbContext _dbContext;


        public SaleQuotationService(
            ISaleQuotationRepository repo,
            ISaleRepository saleRepository,
            IQuotationRepository quotationRepository,
            MasterDbContext dbContext)
        {
            _repo = repo;
            _saleRepository = saleRepository;
            _quotationRepository = quotationRepository;
            _dbContext = dbContext;
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

        /// <summary>
        /// Gestiona relaciones complejas entre ventas y cotizaciones.
        /// Soporta operaciones de eliminación, reasignación y limpieza de datos.
        /// </summary>
        public async Task<ManageSaleQuotationRelationshipResponse> ManageRelationshipAsync(
            ManageSaleQuotationRelationshipRequest request,
            string requestedByUserId)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var response = new ManageSaleQuotationRelationshipResponse
            {
                IdSale = request.IdSale,
                IdQuotation = request.IdQuotation,
                Operation = request.Operation
            };

            try
            {
                // Validación inicial
                var sale = await _saleRepository.GetSaleByIdAsync(request.IdSale);
                if (sale == null)
                    throw new InvalidOperationException($"Sale {request.IdSale} not found.");

                var quotation = await _quotationRepository.GetByIdAsync(request.IdQuotation);
                if (quotation == null)
                    throw new InvalidOperationException($"Quotation {request.IdQuotation} not found.");

                // Verificar que la relación existe
                var relationExists = await _repo.LinkExistsAsync(request.IdSale, request.IdQuotation);
                if (!relationExists)
                    throw new InvalidOperationException(
                        $"Relationship between Sale {request.IdSale} and Quotation {request.IdQuotation} does not exist.");

                // Ejecutar operación según tipo
                switch (request.Operation.ToUpper())
                {
                    case "DELETE":
                        await HandleDeleteOperation(request, response, requestedByUserId);
                        break;

                    case "DELETE_WITH_SALE":
                        await HandleDeleteWithSaleOperation(request, response, requestedByUserId, sale);
                        break;

                    case "REASSIGN":
                        await HandleReassignOperation(request, response, requestedByUserId);
                        break;

                    default:
                        throw new ArgumentException($"Unknown operation: {request.Operation}");
                }

                response.Success = true;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                throw;
            }
        }

        /// <summary>
        /// Elimina solo la relación (venta persiste).
        /// </summary>
        private async Task HandleDeleteOperation(
            ManageSaleQuotationRelationshipRequest request,
            ManageSaleQuotationRelationshipResponse response,
            string requestedByUserId)
        {
            var deleted = await _repo.DeleteAsync(request.IdSale, request.IdQuotation);

            if (!deleted)
                throw new InvalidOperationException("Failed to delete the relationship.");

            response.Message = $"Relationship between Sale {request.IdSale} and Quotation {request.IdQuotation} has been deleted. Sale persists in the system.";
        }

        /// <summary>
        /// Elimina la relación y la venta asociada (cascada).
        /// Operación transaccional para garantizar consistencia.
        /// </summary>
        private async Task HandleDeleteWithSaleOperation(
            ManageSaleQuotationRelationshipRequest request,
            ManageSaleQuotationRelationshipResponse response,
            string requestedByUserId,
            Sale sale)
        {

            await using var tx = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // 1️⃣ Capturar información de la venta para auditoría
                var deletedInfo = new DeletedItemsInfo
                {
                    SaleDeleted = true,
                    DeletedSaleFolio = sale.Folio,
                    DeletedProductCount = sale.ProductsJson?.Count ?? 0,
                    DeletedSaleAmount = sale.TotalAmount
                };

                // 2️⃣ Eliminar todas las relaciones de esta venta
                var relations = await _repo.GetBySaleIdAsync(request.IdSale);
                foreach (var rel in relations)
                {
                    await _repo.DeleteAsync(rel.IdSale, rel.IdQuotation);
                }

                // 3️⃣ Eliminar la venta
                var saleDeleted = await _saleRepository.DeleteSaleAsync(request.IdSale);
                if (!saleDeleted)
                    throw new InvalidOperationException($"Failed to delete Sale {request.IdSale}.");

                response.DeletedInfo = deletedInfo;
                response.Message = $"Sale {request.IdSale} (Folio: {sale.Folio}) and all its relationships have been deleted. " +
                    $"Deleted {deletedInfo.DeletedProductCount} products with total amount of {deletedInfo.DeletedSaleAmount:C}.";

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Reasigna la cotización de una venta a otra venta.
        /// </summary>
        private async Task HandleReassignOperation(
            ManageSaleQuotationRelationshipRequest request,
            ManageSaleQuotationRelationshipResponse response,
            string requestedByUserId)
        {
            if (!request.IdNewSale.HasValue || request.IdNewSale <= 0)
                throw new ArgumentException(
                    "REASSIGN operation requires IdNewSale to be specified and greater than 0.");

            if (request.IdNewSale == request.IdSale)
                throw new ArgumentException(
                    "IdNewSale cannot be the same as IdSale. Specify a different sale.");

            // Validar que la nueva venta existe
            var newSale = await _saleRepository.GetSaleByIdAsync(request.IdNewSale.Value);
            if (newSale == null)
                throw new InvalidOperationException($"New Sale {request.IdNewSale} not found.");

            // Verificar que no exista ya una relación entre la nueva venta y la cotización
            var relationAlreadyExists = await _repo.LinkExistsAsync(request.IdNewSale.Value, request.IdQuotation);
            if (relationAlreadyExists)
                throw new InvalidOperationException(
                    $"Quotation {request.IdQuotation} is already linked to Sale {request.IdNewSale}.");

            await using var tx = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // 1️⃣ Eliminar relación antigua
                var deletedOld = await _repo.DeleteAsync(request.IdSale, request.IdQuotation);
                if (!deletedOld)
                    throw new InvalidOperationException("Failed to delete the old relationship.");

                // 2️⃣ Crear nueva relación
                var newRelation = new SaleQuotation
                {
                    IdSale = request.IdNewSale.Value,
                    IdQuotation = request.IdQuotation,
                    CreatedBy = requestedByUserId,
                    CreatedAt = DateTime.UtcNow,
                    GeneralComment = $"Reassigned from Sale {request.IdSale}. Reason: {request.Reason ?? "Not specified"}",
                    ProductsJson = new List<SingleProductJson>(),
                    PriceSnapshot = new PriceSnapshotJson
                    {
                        Subtotal = 0,
                        TaxAmount = 0,
                        TotalAmount = 0,
                        Currency = "MXN"
                    }
                };

                await _repo.CreateAsync(newRelation);

                response.IdNewSale = request.IdNewSale;
                response.Message = $"Quotation {request.IdQuotation} has been reassigned from Sale {request.IdSale} to Sale {request.IdNewSale}. " +
                    $"Reason: {request.Reason ?? "Not specified"}";

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

    }
}