using VYAACentralInforApi.Application.Sales.Interfaces;
using VYAACentralInforApi.Domain.Sales.Entities;
using VYAACentralInforApi.Domain.Sales.Interfaces;
using MongoDB.Bson;

namespace VYAACentralInforApi.Application.Sales.Services;

public class QuotationService : IQuotationService
{
    private readonly IQuotationRepository _quotationRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IQuotationFollowupsRepository _followupsRepository;

    public QuotationService(
        IQuotationRepository quotationRepository,
        ICustomerRepository customerRepository,
        IQuotationFollowupsRepository followupsRepository)
    {
        _quotationRepository = quotationRepository;
        _customerRepository = customerRepository;
        _followupsRepository = followupsRepository;
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
            quotation.Customer = customer;
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

        return await _quotationRepository.CreateQuotationAsync(quotation);
    }

    public async Task<IEnumerable<Quotation>> GetQuotationsAsync(DateTime? startDate = null, DateTime? endDate = null, string? customerName = null, int? folio = null)
    {
        // Si se proporciona un folio específico, buscar solo por folio
        if (folio.HasValue)
        {
            var quotationByFolio = await _quotationRepository.GetQuotationByFolioAsync(folio.Value);
            return quotationByFolio != null ? new List<Quotation> { quotationByFolio } : new List<Quotation>();
        }

        // Si no se proporcionan fechas, usar el día actual
        if (!startDate.HasValue || !endDate.HasValue)
        {
            var today = DateTime.UtcNow.Date;
            startDate = today;
            endDate = today.AddDays(1).AddTicks(-1); // Hasta el final del día
        }

        // Obtener cotizaciones por rango de fechas
        var quotations = await _quotationRepository.GetQuotationsByDateRangeAsync(startDate.Value, endDate.Value);

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

        return await _quotationRepository.GetQuotationByIdAsync(id);
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
            quotation.Customer = customer;
        }

        // Mantener algunos campos originales
        quotation.CreatedAt = existingQuotation.CreatedAt;
        quotation.UpdatedAt = DateTime.UtcNow;

        return await _quotationRepository.UpdateQuotationAsync(quotation);
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
}