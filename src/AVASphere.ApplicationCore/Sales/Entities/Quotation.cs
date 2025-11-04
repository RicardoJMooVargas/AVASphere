using System.ComponentModel.DataAnnotations.Schema;
using AVASphere.ApplicationCore.Common.Entities.General;
using AVASphere.ApplicationCore.Common.Entities.Jsons;

namespace AVASphere.ApplicationCore.Sales.Entities;

public class Quotation
{
    public int QuotationId { get; set; }
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "PENDIENTE";
    public List<string> SalesExecutives { get; set; } = new List<string>();
    public int Folio { get; set; }
    public int CustomerId { get; set; }
    public string? GeneralComment { get; set; }
    
    [Column(TypeName = "jsonb")]
    public List<QuotationFollowupsJson> Followups { get; set; } = new List<QuotationFollowupsJson>();
    
    // NUEVO: Lista simplificada de productos (JSONB) - opcional
    [Column(TypeName = "jsonb")]
    public List<SingleProductJson>? Products { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // NUEVO: Referencia a la venta vinculada (opcional)
    public string? LinkedSaleId { get; set; }
    public string? LinkedSaleFolio { get; set; }

    // FK a ConfigSys (si es necesaria)
    public int IdConfigSys { get; set; }

    // Propiedades de navegación
    public Customer? Customer { get; set; }
    public ConfigSys? ConfigSys { get; set; }

    // Propiedad calculada para saber si está vinculada a una venta
    [NotMapped]
    public bool IsLinkedToSale => !string.IsNullOrEmpty(LinkedSaleId);

    [NotMapped]
    public bool HasProducts => Products?.Count > 0;
}

public class QuotationFollowupsJson
{
    public string Id { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string Comment { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}