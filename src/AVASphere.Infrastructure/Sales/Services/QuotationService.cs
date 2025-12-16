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

        if (dto.CustomerId > 0)
        {
            customer = await _customerRepository.GetByIdAsync(dto.CustomerId);
            if (customer == null)
                throw new Exception($"IdCustomer {dto.CustomerId} no existe.");
        }
        else
        {
            // Si NO envió IdCustomer → revisar NewCustomers
            if (dto.NewCustomers == null || !dto.NewCustomers.Any())
                throw new Exception("No se envió un IdCustomer válido ni datos de NewCustomer.");

            var nc = dto.NewCustomers.First();

            // Validaciones mínimas
            if (string.IsNullOrWhiteSpace(nc.Name))
                throw new Exception("El nombre del cliente es obligatorio para crear uno nuevo.");

            // Crear nuevo customer
            customer = new Customer
            {
                ExternalId = int.TryParse(nc.ExternalId, out var externalId) ? externalId : 0,
                Name = nc.Name,
                Email = nc.Email,
                PhoneNumber = string.IsNullOrWhiteSpace(nc.PhoneNumber) ? "+00" : nc.PhoneNumber,
                DirectionJson = new DirectionJson
                {
                    Colony = nc.Direction
                },
                SettingsCustomerJson = new SettingsCustomerJson
                {
                    Index = 1,
                    Type = "General"
                }
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


    public async Task<IEnumerable<Quotation>> GetQuotationsAsync(QuotationFilterDto? filter = null)
    {
        // Si no se proporciona filtro, crear uno con valores por defecto
        filter ??= new QuotationFilterDto();

        // 1️⃣ Si se especifica IdQuotation, buscarlo directamente
        if (filter.IdQuotation.HasValue)
        {
            var q = await _quotationRepository.GetByIdAsync(filter.IdQuotation.Value);
            return q != null ? new[] { q } : Array.Empty<Quotation>();
        }

        // 2️⃣ Si se especifica Folio, buscarlo
        if (filter.Folio.HasValue)
        {
            var q = await _quotationRepository.GetQuotationByFolioAsync(filter.Folio.Value);
            return q != null ? new[] { q } : Array.Empty<Quotation>();
        }

        // 3️⃣ Buscar por rango de fechas (por defecto mes actual)
        var quotations = await _quotationRepository.GetQuotationsByDateRangeAsync(filter.StartDate, filter.EndDate);

        // 4️⃣ Filtrar por IdCustomer si se especifica
        if (filter.IdCustomer.HasValue)
        {
            quotations = quotations.Where(q => q.IdCustomer == filter.IdCustomer.Value);
        }

        return quotations;
    }

    public async Task<Quotation?> UpdateIdQuotation(int IdQuotation, QuotationUpdateDto dto)
    {
        if (dto is null)
            throw new ArgumentNullException(nameof(dto));

        // 1️⃣ Validar que hay al menos UN campo para actualizar
        bool hasChanges = !string.IsNullOrWhiteSpace(dto.Folio) ||
                          dto.SaleDate.HasValue ||
                          dto.Status.HasValue ||
                          !string.IsNullOrWhiteSpace(dto.GeneralComment) ||
                          (dto.SalesExecutives != null && dto.SalesExecutives.Any()) ||
                          dto.IdConfigSys.HasValue ||
                          (dto.Products != null && dto.Products.Any()) ||
                          (dto.FollowupsToAdd != null && dto.FollowupsToAdd.Any()) ||
                          (dto.FollowupsToEdit != null && dto.FollowupsToEdit.Any()) ||
                          (dto.FollowupsToDelete != null && dto.FollowupsToDelete.Any());

        if (!hasChanges)
            throw new Exception("No se especificó ningún campo para actualizar.");

        // 2️⃣ Obtener la cotización actual
        var quotation = await _quotationRepository.GetByIdAsync(IdQuotation);
        if (quotation == null)
            return null;

        // 3️⃣ Actualizar SOLO los campos que fueron especificados
        if (!string.IsNullOrWhiteSpace(dto.Folio))
            quotation.Folio = int.Parse(dto.Folio);

        if (dto.SaleDate.HasValue)
            quotation.SaleDate = dto.SaleDate.Value;

        if (dto.Status.HasValue)
            quotation.Status = dto.Status.Value;

        if (!string.IsNullOrWhiteSpace(dto.GeneralComment))
            quotation.GeneralComment = dto.GeneralComment;

        if (dto.SalesExecutives != null && dto.SalesExecutives.Any())
            quotation.SalesExecutives = dto.SalesExecutives;

        if (dto.IdConfigSys.HasValue)
            quotation.IdConfigSys = dto.IdConfigSys.Value;

        if (dto.Products != null && dto.Products.Any())
            quotation.ProductsJson = dto.Products;

        // 4️⃣ Manejar followups
        quotation.FollowupsJson ??= new List<QuotationFollowupsJson>();

        // Eliminar followups especificados
        if (dto.FollowupsToDelete != null && dto.FollowupsToDelete.Any())
        {
            foreach (var followupId in dto.FollowupsToDelete)
            {
                quotation.FollowupsJson.RemoveAll(f => f.Id == followupId);
            }
        }

        // Editar followups existentes
        if (dto.FollowupsToEdit != null && dto.FollowupsToEdit.Any())
        {
            foreach (var editDto in dto.FollowupsToEdit)
            {
                // Buscar el followup existente por Id
                var existingFollowup = quotation.FollowupsJson.FirstOrDefault(f => f.Id == editDto.Id);
                if (existingFollowup != null)
                {
                    // Actualizar campos del followup existente
                    existingFollowup.Date = editDto.Date;
                    existingFollowup.Comment = editDto.Comment;
                    existingFollowup.UserId = editDto.UserId;
                    // CreatedAt NO se actualiza, mantiene el original
                }
                else
                {
                    throw new KeyNotFoundException($"Followup with Id {editDto.Id} not found in quotation {IdQuotation}.");
                }
            }
        }

        // Agregar nuevos followups (sin Id)
        if (dto.FollowupsToAdd != null && dto.FollowupsToAdd.Any())
        {
            foreach (var createDto in dto.FollowupsToAdd)
            {
                var newFollowup = new QuotationFollowupsJson
                {
                    Id = await _quotationRepository.GetNextFollowupIdAsync(IdQuotation),
                    Date = createDto.Date ?? DateTime.UtcNow,
                    Comment = createDto.Comment,
                    UserId = createDto.UserId ?? "system",
                    CreatedAt = DateTime.UtcNow
                };
                quotation.FollowupsJson.Add(newFollowup);
            }
        }

        // 5️⃣ Actualizar timestamp
        quotation.UpdatedAt = DateTime.UtcNow;

        // 6️⃣ Guardar cambios
        await _quotationRepository.UpdateQuotationAsync(quotation);

        return quotation;
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
        // 1️⃣ Verificar que la cotización existe
        var quotation = await _quotationRepository.GetByIdAsync(id);
        if (quotation == null)
            throw new KeyNotFoundException($"Quotation with ID {id} not found.");

        // 2️⃣ Validar que NO está vinculada a ventas
        // CAMBIO: Se requiere validar antes de eliminar para evitar eliminar cotizaciones en uso
        if (!string.IsNullOrEmpty(quotation.LinkedSaleId))
            throw new InvalidOperationException(
                $"Cannot delete quotation linked to sale. Sale ID: {quotation.LinkedSaleId}, Folio: {quotation.LinkedSaleFolio}.");

        // 3️⃣ Proceder con la eliminación
        return await _quotationRepository.DeleteQuotationAsync(id);
    }

    public async Task<bool> DeleteFollowupFromQuotationAsync(int IdQuotation, int followupId)
    {
        // 1️⃣ Verificar que la cotización existe
        var quotation = await _quotationRepository.GetByIdAsync(IdQuotation);
        if (quotation == null)
            throw new KeyNotFoundException($"Quotation with ID {IdQuotation} not found.");

        // 2️⃣ Inicializar lista si está vacía
        if (quotation.FollowupsJson == null || !quotation.FollowupsJson.Any())
            throw new KeyNotFoundException($"No followups found in quotation {IdQuotation}.");

        // 3️⃣ Buscar y eliminar el followup específico
        // CAMBIO: Usar FindIndex y validar existencia para dar error específico si no existe
        var followupIndex = quotation.FollowupsJson.FindIndex(f => f.Id == followupId);
        if (followupIndex < 0)
            throw new KeyNotFoundException($"Followup with ID {followupId} not found in quotation {IdQuotation}.");

        // 4️⃣ Eliminar el followup
        quotation.FollowupsJson.RemoveAt(followupIndex);

        // 5️⃣ Actualizar timestamp y guardar
        quotation.UpdatedAt = DateTime.UtcNow;
        await _quotationRepository.UpdateQuotationAsync(quotation);
        return true;
    }

}