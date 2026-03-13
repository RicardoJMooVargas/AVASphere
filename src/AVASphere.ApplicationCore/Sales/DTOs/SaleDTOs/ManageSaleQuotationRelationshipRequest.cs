using System.ComponentModel.DataAnnotations;

namespace AVASphere.ApplicationCore.Sales.DTOs;

/// <summary>
/// DTO para gestionar relaciones complejas entre ventas y cotizaciones.
/// 
/// Permite realizar operaciones avanzadas:
/// 1. REASSIGN: Reasignar una cotización de una venta a otra
/// 2. DELETE: Eliminar la relación (venta permanece)
/// 3. DELETE_WITH_SALE: Eliminar relación y la venta asociada (cascada)
/// 
/// CASOS DE USO:
/// - Transferir cotización entre ventas
/// - Limpiar relaciones incorrectas
/// - Eliminar ventas externas/importadas completas
/// </summary>
public class ManageSaleQuotationRelationshipRequest
{
    /// <summary>
    /// ID de la venta actual (origen).
    /// </summary>
   
    public int IdSale { get; set; }

    /// <summary>
    /// ID de la cotización a gestionar.
    /// </summary>
    public int IdQuotation { get; set; }

    /// <summary>
    /// Tipo de operación a realizar.
    /// 
    /// Valores permitidos:
    /// - "DELETE": Elimina solo la relación (venta persiste)
    /// - "DELETE_WITH_SALE": Elimina la relación Y la venta asociada (cascada)
    /// - "REASSIGN": Reasigna la cotización a otra venta (requiere IdNewSale)
    /// </summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// ID de la venta destino (solo requerido si Operation = "REASSIGN").
    /// Será la nueva venta a la que se vinculará la cotización.
    /// </summary>
    public int? IdNewSale { get; set; }

    /// <summary>
    /// Comentario sobre el motivo de la operación (para auditoría).
    /// Útil para documentar por qué se reasignó, eliminó o cambió la relación.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Flag para confirmar operaciones destructivas (DELETE_WITH_SALE).
    /// Evita eliminaciones accidentales de datos.
    /// </summary>
    public bool ConfirmDeletionWithSale { get; set; } = false;
}

/// <summary>
/// DTO de respuesta para operaciones de gestión de relaciones.
/// </summary>
public class ManageSaleQuotationRelationshipResponse
{
    /// <summary>
    /// Indica si la operación fue exitosa.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Tipo de operación que se realizó.
    /// </summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Mensaje descriptivo del resultado.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// ID de la venta original.
    /// </summary>
    public int IdSale { get; set; }

    /// <summary>
    /// ID de la cotización.
    /// </summary>
    public int IdQuotation { get; set; }

    /// <summary>
    /// ID de la nueva venta (si fue reasignada).
    /// </summary>
    public int? IdNewSale { get; set; }

    /// <summary>
    /// Información adicional sobre lo que fue eliminado o modificado.
    /// </summary>
    public DeletedItemsInfo? DeletedInfo { get; set; }

    /// <summary>
    /// Timestamp de cuándo se realizó la operación.
    /// </summary>
    public DateTime OperationDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Información sobre los items eliminados durante una operación.
/// </summary>
public class DeletedItemsInfo
{
    /// <summary>
    /// Flag indicando si la venta fue eliminada.
    /// </summary>
    public bool SaleDeleted { get; set; }

    /// <summary>
    /// Folio de la venta eliminada (para referencia).
    /// </summary>
    public string? DeletedSaleFolio { get; set; }

    /// <summary>
    /// Cantidad de productos que fueron eliminados junto con la venta.
    /// </summary>
    public int DeletedProductCount { get; set; }

    /// <summary>
    /// Monto total de la venta eliminada.
    /// </summary>
    public decimal DeletedSaleAmount { get; set; }
}
