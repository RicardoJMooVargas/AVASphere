using VYAACentralInforApi.ApplicationCore.Sales.Interfaces;
using VYAACentralInforApi.ApplicationCore.Sales.Entities;
using MongoDB.Bson;

namespace VYAACentralInforApi.ApplicationCore.Sales.Services;

public class QuotationService : IQuotationService
{
    private readonly IQuotationRepository _quotationRepository;
    private readonly ICustomerRepository _customerRepository;

    public QuotationService(
        IQuotationRepository quotationRepository,
        ICustomerRepository customerRepository)
    {
        _quotationRepository = quotationRepository;
        _customerRepository = customerRepository;
    }

    public async Task<Quotation> CreateQuotationAsync(Quotation quotation, string createdByUserId)
    {
        // Validar que el usuario creador no esté vacío
        if (string.IsNullOrEmpty(createdByUserId))
        {
            throw new ArgumentException("El ID del usuario creador es requerido.", nameof(createdByUserId));
        }

        // Validar que el folio no esté vacío
        if (quotation.Folio <= 0)
        {
            throw new ArgumentException("El folio debe ser mayor a 0.", nameof(quotation.Folio));
        }

        // Verificar si el folio ya existe
        if (await _quotationRepository.QuotationExistsByFolioAsync(quotation.Folio))
        {
            throw new InvalidOperationException($"Ya existe una cotización con el folio {quotation.Folio}.");
        }

        // Si no se proporciona SaleDate o es null, usar la fecha actual
        if (quotation.SaleDate == default(DateTime))
        {
            quotation.SaleDate = DateTime.UtcNow.Date;
        }

        // Verificar que el customer existe si se proporciona CustomerId
        if (!string.IsNullOrEmpty(quotation.CustomerId))
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(quotation.CustomerId);
            if (customer == null)
            {
                throw new InvalidOperationException($"No existe un cliente con el ID {quotation.CustomerId}.");
            }
        }

        // Configurar campos automáticos
        quotation.QuotationId = ObjectId.GenerateNewId().ToString();
        quotation.Status = "PENDIENTE";
        quotation.CreatedAt = DateTime.UtcNow;
        quotation.UpdatedAt = DateTime.UtcNow;

        // Agregar el usuario creador a SalesExecutives si no está vacío
        if (quotation.SalesExecutives == null)
        {
            quotation.SalesExecutives = new List<string>();
        }
        
        if (!quotation.SalesExecutives.Contains(createdByUserId))
        {
            quotation.SalesExecutives.Insert(0, createdByUserId); // El creador va primero
        }

        // Crear followup si se proporcionan datos
        if (quotation.Followups != null && quotation.Followups.Any())
        {
            foreach (var followup in quotation.Followups)
            {
                followup.Id = ObjectId.GenerateNewId().ToString();
                followup.Date = DateTime.UtcNow;
                followup.CreatedAt = DateTime.UtcNow;
                if (string.IsNullOrEmpty(followup.UserId))
                {
                    followup.UserId = createdByUserId;
                }
            }
        }

        var createdQuotation = await _quotationRepository.CreateQuotationAsync(quotation);

        // Cargar los datos del cliente después de crear la cotización
        if (!string.IsNullOrEmpty(createdQuotation.CustomerId))
        {
            createdQuotation.Customer = await _customerRepository.GetCustomerByIdAsync(createdQuotation.CustomerId);
        }

        return createdQuotation;
    }

    public async Task<IEnumerable<Quotation>> GetQuotationsAsync(DateTime? startDate = null, DateTime? endDate = null, string? customerName = null, int? folio = null)
    {
        IEnumerable<Quotation> quotations;

        // Si se proporciona un folio específico, buscar solo por folio
        if (folio.HasValue)
        {
            var quotationByFolio = await _quotationRepository.GetQuotationByFolioAsync(folio.Value);
            quotations = quotationByFolio != null ? new List<Quotation> { quotationByFolio } : new List<Quotation>();
        }
        else
        {
            // Si no se proporcionan fechas, usar el día actual
            if (!startDate.HasValue || !endDate.HasValue)
            {
                var today = DateTime.UtcNow.Date;
                startDate = today;
                endDate = today.AddDays(1).AddTicks(-1); // Hasta el final del día
            }

            // Obtener cotizaciones por rango de fechas
            quotations = await _quotationRepository.GetQuotationsByDateRangeAsync(startDate.Value, endDate.Value);
        }

        // Cargar los datos del cliente para cada cotización
        foreach (var quotation in quotations)
        {
            if (!string.IsNullOrEmpty(quotation.CustomerId))
            {
                quotation.Customer = await _customerRepository.GetCustomerByIdAsync(quotation.CustomerId);
            }
        }

        // Filtrar por nombre de cliente si se proporciona
        if (!string.IsNullOrEmpty(customerName))
        {
            quotations = quotations.Where(q => q.Customer != null && 
                q.Customer.FullName.Contains(customerName, StringComparison.OrdinalIgnoreCase));
        }

        return quotations.OrderByDescending(q => q.CreatedAt);
    }

    public async Task<Quotation?> GetQuotationByIdAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("El ID de la cotización es requerido.", nameof(id));
        }

        var quotation = await _quotationRepository.GetQuotationByIdAsync(id);
        
        // Cargar los datos del cliente si existe
        if (quotation != null && !string.IsNullOrEmpty(quotation.CustomerId))
        {
            quotation.Customer = await _customerRepository.GetCustomerByIdAsync(quotation.CustomerId);
        }

        return quotation;
    }

    public async Task<Quotation> UpdateQuotationAsync(Quotation quotation)
    {
        if (quotation == null)
        {
            throw new ArgumentNullException(nameof(quotation));
        }

        if (string.IsNullOrEmpty(quotation.QuotationId))
        {
            throw new ArgumentException("El ID de la cotización es requerido para actualizar.", nameof(quotation.QuotationId));
        }

        // Verificar que la cotización existe
        var existingQuotation = await _quotationRepository.GetQuotationByIdAsync(quotation.QuotationId);
        if (existingQuotation == null)
        {
            throw new InvalidOperationException($"No se encontró la cotización con ID {quotation.QuotationId}.");
        }

        // Verificar que el customer existe si se proporciona CustomerId
        if (!string.IsNullOrEmpty(quotation.CustomerId))
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(quotation.CustomerId);
            if (customer == null)
            {
                throw new InvalidOperationException($"No existe un cliente con el ID {quotation.CustomerId}.");
            }
        }

        // Mantener algunos campos originales
        quotation.CreatedAt = existingQuotation.CreatedAt;
        quotation.UpdatedAt = DateTime.UtcNow;

        var updatedQuotation = await _quotationRepository.UpdateQuotationAsync(quotation);

        // Cargar los datos del cliente después de actualizar
        if (!string.IsNullOrEmpty(updatedQuotation.CustomerId))
        {
            updatedQuotation.Customer = await _customerRepository.GetCustomerByIdAsync(updatedQuotation.CustomerId);
        }

        return updatedQuotation;
    }

    public async Task<bool> DeleteQuotationAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("El ID de la cotización es requerido.", nameof(id));
        }

        // Verificar que la cotización existe antes de eliminar
        var existingQuotation = await _quotationRepository.GetQuotationByIdAsync(id);
        if (existingQuotation == null)
        {
            throw new InvalidOperationException($"No se encontró la cotización con ID {id}.");
        }

        return await _quotationRepository.DeleteQuotationAsync(id);
    }

    public async Task<QuotationFollowups> AddFollowupToQuotationAsync(string quotationId, QuotationFollowups followup, string userId)
    {
        if (string.IsNullOrEmpty(quotationId))
        {
            throw new ArgumentException("El ID de la cotización es requerido.", nameof(quotationId));
        }

        if (followup == null)
        {
            throw new ArgumentNullException(nameof(followup));
        }

        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("El ID del usuario es requerido.", nameof(userId));
        }

        // Verificar que la cotización existe
        var quotation = await _quotationRepository.GetQuotationByIdAsync(quotationId);
        if (quotation == null)
        {
            throw new InvalidOperationException($"No se encontró la cotización con ID {quotationId}.");
        }

        // Configurar el followup
        followup.Id = ObjectId.GenerateNewId().ToString();
        followup.Date = DateTime.UtcNow;
        followup.CreatedAt = DateTime.UtcNow;
        followup.UserId = userId;

        // Agregar el followup a la lista
        quotation.Followups.Add(followup);
        quotation.UpdatedAt = DateTime.UtcNow;

        // Actualizar la cotización
        await _quotationRepository.UpdateQuotationAsync(quotation);

        return followup;
    }

    public async Task<QuotationFollowups> UpdateFollowupInQuotationAsync(string quotationId, string followupId, QuotationFollowups updatedFollowup)
    {
        if (string.IsNullOrEmpty(quotationId))
        {
            throw new ArgumentException("El ID de la cotización es requerido.", nameof(quotationId));
        }

        if (string.IsNullOrEmpty(followupId))
        {
            throw new ArgumentException("El ID del followup es requerido.", nameof(followupId));
        }

        if (updatedFollowup == null)
        {
            throw new ArgumentNullException(nameof(updatedFollowup));
        }

        // Verificar que la cotización existe
        var quotation = await _quotationRepository.GetQuotationByIdAsync(quotationId);
        if (quotation == null)
        {
            throw new InvalidOperationException($"No se encontró la cotización con ID {quotationId}.");
        }

        // Buscar el followup específico
        var existingFollowup = quotation.Followups.FirstOrDefault(f => f.Id == followupId);
        if (existingFollowup == null)
        {
            throw new InvalidOperationException($"No se encontró el followup con ID {followupId} en la cotización {quotationId}.");
        }

        // Actualizar los campos del followup (mantener ID original, Date y CreatedAt)
        existingFollowup.Comment = updatedFollowup.Comment;
        existingFollowup.Date = updatedFollowup.Date;

        // Actualizar la fecha de modificación de la cotización
        quotation.UpdatedAt = DateTime.UtcNow;

        // Actualizar la cotización
        await _quotationRepository.UpdateQuotationAsync(quotation);

        return existingFollowup;
    }

    public async Task<bool> DeleteFollowupFromQuotationAsync(string quotationId, string followupId)
    {
        if (string.IsNullOrEmpty(quotationId))
        {
            throw new ArgumentException("El ID de la cotización es requerido.", nameof(quotationId));
        }

        if (string.IsNullOrEmpty(followupId))
        {
            throw new ArgumentException("El ID del followup es requerido.", nameof(followupId));
        }

        // Verificar que la cotización existe
        var quotation = await _quotationRepository.GetQuotationByIdAsync(quotationId);
        if (quotation == null)
        {
            throw new InvalidOperationException($"No se encontró la cotización con ID {quotationId}.");
        }

        // Buscar y eliminar el followup
        var followupToRemove = quotation.Followups.FirstOrDefault(f => f.Id == followupId);
        if (followupToRemove == null)
        {
            return false;
        }

        quotation.Followups.Remove(followupToRemove);
        quotation.UpdatedAt = DateTime.UtcNow;

        // Actualizar la cotización
        await _quotationRepository.UpdateQuotationAsync(quotation);

        return true;
    }
}