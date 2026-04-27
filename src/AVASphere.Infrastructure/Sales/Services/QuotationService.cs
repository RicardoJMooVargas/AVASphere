using System;
using System.Collections.Generic;
using System.Globalization;
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
    private readonly IConfigSysRepository _configSysRepository;


    public QuotationService(
        IQuotationRepository quotationRepository,
        IQuotationVersionRepository versionRepository,
        ICustomerRepository customerRepository,
        IConfigSysRepository configSysRepository)
    {
        _quotationRepository = quotationRepository;
        _versionRepository = versionRepository;
        _customerRepository = customerRepository;
        _configSysRepository = configSysRepository;
    }

    public async Task<Quotation> CreateQuotationAsync(CreateQuotationDto dto, string createdByUserId)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        // ✅ Validar que el folio no exista
        var existingQuotation = await _quotationRepository.GetQuotationByFolioAsync(dto.Folio);
        if (existingQuotation != null)
        {
            var createdAt = existingQuotation.CreatedAt;
            var culture = new CultureInfo("es-MX");
            var diaTexto = createdAt.Day == 1 ? "1ro" : createdAt.Day.ToString(culture);
            var mesAnio = $"{createdAt.ToString("MMMM", culture)} ({createdAt:MM}) de {createdAt:yyyy}";
            var hora = createdAt.ToString("HH:mm", culture);
            var fechaCreacion = $"{diaTexto} de {mesAnio} a las {hora}";
            var usuarioCreador = existingQuotation.Versions?
                .OrderBy(v => v.CreatedAt)
                .Select(v => v.CreatedBy)
                .FirstOrDefault(u => !string.IsNullOrWhiteSpace(u))
                ?? existingQuotation.SalesExecutives?.FirstOrDefault()
                ?? "No disponible";
            var nombreCliente = string.IsNullOrWhiteSpace(existingQuotation.Customer?.Name)
                ? "No disponible"
                : existingQuotation.Customer.Name;

            throw new Exception($"Ya existe una cotización con el folio {dto.Folio}. Fecha de creación: {fechaCreacion}. Usuario creador: {usuarioCreador}. Cliente asociado: {nombreCliente}. Por favor, use un folio diferente.");
        }

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
                DirectionJson = !string.IsNullOrWhiteSpace(nc.Direction) ? new DirectionJson
                {
                    Colony = nc.Direction
                } : null,
                SettingsCustomerJson = new SettingsCustomerJson
                {
                    Index = 1,
                    Type = "General"
                }
            };

            customer = await _customerRepository.InsertAsync(customer);
        }

        // 4️⃣ Validar y obtener IdConfigSys válido
        int validIdConfigSys = dto.IdConfigSys;
        if (validIdConfigSys <= 0)
        {
            // Si no se proporciona un IdConfigSys válido, obtener el primer registro disponible
            validIdConfigSys = await GetValidConfigSysIdAsync();
        }

        // 5️⃣ Crear Quotation
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
            IdConfigSys = validIdConfigSys,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdQuotation = await _quotationRepository.CreateQuotationAsync(quotation);

        // 6️⃣ Agregar followups iniciales (opcional)
        if (dto.Followups != null && dto.Followups.Any())
        {
            int followupIdCounter = 1;
            foreach (var f in dto.Followups)
            {
                createdQuotation.FollowupsJson.Add(new QuotationFollowupsJson
                {
                    Id = followupIdCounter++,
                    Date = f.Date ?? DateTime.UtcNow,
                    Comment = f.Comment,
                    UserId = f.UserId ?? createdByUserId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _quotationRepository.UpdateQuotationAsync(createdQuotation);
        }

        // 7️⃣ Crear versión inicial si hay productos
        if (dto.Products != null && dto.Products.Any())
        {
            var version = new QuotationVersion
            {
                IdQuotation = createdQuotation.IdQuotation,
                VersionNumber = 1,
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.UtcNow,
                ProductsJson = dto.Products.ToList(),
                Quotation = null,
                QuotationDataJson = null
            };
            await _versionRepository.CreateAsync(version);
        }

        // 8️⃣ Releer la cotización con versiones
        var result = await _quotationRepository.GetByIdAsync(createdQuotation.IdQuotation);

        if (result == null)
            throw new Exception($"Error inesperado: la cotización {createdQuotation.IdQuotation} no pudo ser recuperada después de ser creada.");

        // 9️⃣ SANITIZAR: eliminar referencias circulares antes de devolver
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

        return result;
    }


    public async Task<IEnumerable<Quotation>> GetQuotationsAsync(DateTime? startDate = null, DateTime? endDate = null, QuotationFilterDto? filter = null)
    {
        // Si no se proporciona filtro, crear uno vacío
        filter ??= new QuotationFilterDto();

        var today = DateTime.UtcNow;
        var effectiveStartDate = startDate ?? filter.StartDate;
        var effectiveEndDate = endDate ?? filter.EndDate;

        DateTime rangeStartDate;
        DateTime rangeEndDate;

        // Si se envían ambas fechas, se usa el rango completo.
        // Si no se envía ninguna, se usa el mes actual por defecto.
        if (effectiveStartDate.HasValue && effectiveEndDate.HasValue)
        {
            rangeStartDate = effectiveStartDate.Value.Date;
            rangeEndDate = effectiveEndDate.Value.Date;
        }
        else if (effectiveStartDate.HasValue)
        {
            rangeStartDate = effectiveStartDate.Value.Date;
            rangeEndDate = rangeStartDate;
        }
        else if (effectiveEndDate.HasValue)
        {
            rangeEndDate = effectiveEndDate.Value.Date;
            rangeStartDate = rangeEndDate;
        }
        else
        {
            rangeStartDate = new DateTime(today.Year, today.Month, 1);
            rangeEndDate = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
        }

        if (rangeStartDate > rangeEndDate)
            throw new ArgumentException("StartDate no puede ser mayor que EndDate.");

        IEnumerable<Quotation> quotations;

        // 1️⃣ Si se especifica IdQuotation, buscarlo directamente
        if (filter.IdQuotation.HasValue)
        {
            var q = await _quotationRepository.GetByIdAsync(filter.IdQuotation.Value);
            quotations = q != null ? new[] { q } : Array.Empty<Quotation>();
        }
        // 2️⃣ Si se especifica Folio, buscarlo
        else if (filter.Folio.HasValue)
        {
            var q = await _quotationRepository.GetQuotationByFolioAsync(filter.Folio.Value);
            quotations = q != null ? new[] { q } : Array.Empty<Quotation>();
        }
        // 3️⃣ Buscar por rango de fechas
        else
        {
            quotations = await _quotationRepository.GetQuotationsByDateRangeAsync(rangeStartDate, rangeEndDate);

            // 4️⃣ Filtrar por IdCustomer si se especifica
            if (filter.IdCustomer.HasValue)
            {
                quotations = quotations.Where(q => q.IdCustomer == filter.IdCustomer.Value);
            }

            // 5️⃣ Filtrar por CustomerName si se especifica
            if (!string.IsNullOrWhiteSpace(filter.CustomerName))
            {
                quotations = quotations.Where(q => q.Customer != null &&
                    ((q.Customer.Name?.Contains(filter.CustomerName, StringComparison.OrdinalIgnoreCase) ?? false) ||
                     (q.Customer.Email?.Contains(filter.CustomerName, StringComparison.OrdinalIgnoreCase) ?? false)));
            }

            // 6️⃣ Filtrar por ExternalId si se especifica
            if (filter.ExternalId.HasValue && filter.ExternalId.Value > 0)
            {
                quotations = quotations.Where(q => q.Customer != null && q.Customer.ExternalId == filter.ExternalId.Value);
            }

            // 7Filtrar por SalesExecutive si se especifica
            // EXCEPCIÓN: Si el usuario es administrador, no se aplica este filtro
            if (!string.IsNullOrWhiteSpace(filter.SalesExecutive))
            {
                var salesExecutiveLower = filter.SalesExecutive.ToLowerInvariant();
                bool isAdmin = salesExecutiveLower == "admin" ||
                              salesExecutiveLower == "administrador";

                // Solo aplicar filtro si NO es administrador
                if (!isAdmin)
                {
                    quotations = quotations.Where(q => q.SalesExecutives != null &&
                        q.SalesExecutives.Any(se => se.Contains(filter.SalesExecutive, StringComparison.OrdinalIgnoreCase)));
                }
            }
        }

        // 6️⃣ Limpiar referencias circulares problemáticas pero mantener datos del customer
        foreach (var quotation in quotations)
        {
            // Mantener Customer pero limpiar sus referencias circulares
            if (quotation.Customer != null)
            {
                quotation.Customer.Quotations = new List<Quotation>();
                quotation.Customer.Sales = new List<Sale>();
                quotation.Customer.Projects = new List<AVASphere.ApplicationCore.Projects.Entities.General.Project>();
            }

            // Limpiar ConfigSys si existe
            quotation.ConfigSys = null;

            // Limpiar referencias circulares en Versions
            if (quotation.Versions != null)
            {
                foreach (var version in quotation.Versions)
                {
                    version.Quotation = null;
                }
            }
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

    /// <summary>
    /// Obtiene un IdConfigSys válido. Si no hay ninguno configurado, devuelve 1 por defecto.
    /// </summary>
    /// <returns>Un IdConfigSys válido</returns>
    private async Task<int> GetValidConfigSysIdAsync()
    {
        try
        {
            var configSys = await _configSysRepository.GetAsync();
            if (configSys != null && configSys.IdConfigSys > 0)
            {
                return configSys.IdConfigSys;
            }

            // Si no hay configuración, devolver 1 como valor por defecto
            // (esto asume que siempre habrá al menos un registro con ID 1)
            return 1;
        }
        catch
        {
            // En caso de error, devolver 1 como fallback
            return 1;
        }
    }

}