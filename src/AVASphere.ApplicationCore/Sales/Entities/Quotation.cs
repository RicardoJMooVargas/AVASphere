using System.ComponentModel.DataAnnotations.Schema;
using AVASphere.ApplicationCore.Common.Entities.General;
using AVASphere.ApplicationCore.Common.Entities.Jsons;

namespace AVASphere.ApplicationCore.Sales.Entities;

public class Quotation
{
    public int IdQuotation { get; set; }
    public int IdCustomer { get; set; }
    public DateOnly SaleDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public StatusEnum Status { get; set; } = StatusEnum.Pending;
    public List<string> SalesExecutives { get; set; } = new List<string>();
    public int Folio { get; set; }
    public string? GeneralComment { get; set; }

    [Column(TypeName = "jsonb")]
    public List<QuotationFollowupsJson> FollowupsJson { get; set; } = new List<QuotationFollowupsJson>();

    // NUEVO: Lista simplificada de productos (JSONB) - opcional
    [Column(TypeName = "jsonb")]
    public List<SingleProductJson>? ProductsJson { get; set; }

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
    public ICollection<SaleQuotation> SaleQuotations { get; set; } = new List<SaleQuotation>();

    // Propiedad calculada para saber si está vinculada a una venta
    [NotMapped]
    public bool IsLinkedToSale => !string.IsNullOrEmpty(LinkedSaleId);

    [NotMapped]
    public bool HasProducts => ProductsJson?.Count > 0;

    public ICollection<QuotationVersion> Versions { get; set; } = new List<QuotationVersion>();
}

public class QuotationFollowupsJson
{
    public int Id { get; set; } = 0;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string Comment { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}