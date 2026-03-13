using AVASphere.ApplicationCore.Common.Entities.Jsons;
using AVASphere.ApplicationCore.Sales.Entities;

namespace AVASphere.ApplicationCore.Sales.DTOs;

/// <summary>
/// DTO para actualizar cotizaciones con campos opcionales.
/// Solo se actualizan los campos que se especifiquen (no-null).
/// IMPORTANTE: Todos los campos deben ser opcionales porque es una actualización parcial.
/// </summary>
public class QuotationUpdateDto
{
    /// <summary>
    /// Folio de la cotización (opcional).
    /// Cambio: string? en lugar de int para mayor flexibilidad y ser opcional.
    /// </summary>
    public string? Folio { get; set; }

    /// <summary>
    /// Fecha de venta (opcional).
    /// Cambio: DateOnly? en lugar de DateOnly para que sea opcional.
    /// </summary>
    public DateOnly? SaleDate { get; set; }

    /// <summary>
    /// Estado de la cotización (opcional).
    /// </summary>
    public StatusEnum? Status { get; set; }

    /// <summary>
    /// Comentario general de la cotización (opcional).
    /// </summary>
    public string? GeneralComment { get; set; }

    /// <summary>
    /// Ejecutivos de ventas asignados (opcional).
    /// Cambio: Removido "= new()" porque eso hace que siempre esté presente,
    /// impidiendo distinguir entre "no envió nada" y "envió una lista vacía".
    /// </summary>
    public List<string>? SalesExecutives { get; set; }

    /// <summary>
    /// ID de configuración del sistema (opcional).
    /// Cambio: int? en lugar de int para ser opcional.
    /// </summary>
    public int? IdConfigSys { get; set; }

    /// <summary>
    /// Productos de la cotización (opcional).
    /// Cambio: SingleProductJson en lugar de QuotationProductDto
    /// porque esa es la estructura que usa la entidad Quotation.
    /// </summary>
    public List<SingleProductJson>? Products { get; set; }

    /// <summary>
    /// Nuevos seguimientos a agregar (opcional).
    /// Cambio: Usa QuotationFollowupCreateDto que NO incluye el campo Id
    /// porque se genera automáticamente en el servidor.
    /// </summary>
    public List<QuotationFollowupCreateDto>? FollowupsToAdd { get; set; }

    /// <summary>
    /// Seguimientos existentes a editar (opcional).
    /// Usa QuotationFollowupsJson porque incluye el campo Id necesario
    /// para identificar cuál seguimiento específico se debe actualizar.
    /// </summary>
    public List<QuotationFollowupsJson>? FollowupsToEdit { get; set; }

    /// <summary>
    /// IDs de seguimientos a eliminar (opcional).
    /// Cambio: Nuevo campo necesario para poder eliminar followups específicos
    /// sin confundirlos con los que se están agregando.
    /// </summary>
    public List<int>? FollowupsToDelete { get; set; }
}