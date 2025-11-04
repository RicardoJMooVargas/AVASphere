using System.ComponentModel.DataAnnotations.Schema;
using AVASphere.ApplicationCore.Common.Entities.General;
using AVASphere.ApplicationCore.Common.Entities.Jsons;

namespace AVASphere.ApplicationCore.Sales.Entities;

public class Sale
{
    public int SaleId { get; set; }
    public string SalesExecutive { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string Type { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string Folio { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string? DeliveryDriver { get; set; }
    public bool HomeDelivery { get; set; } = false;
    public DateTime? DeliveryDate { get; set; }
    public int SatisfactionLevel { get; set; } = 0;
    public string? SatisfactionReason { get; set; }
    public string? Comment { get; set; }
    public DateTime? AfterSalesFollowupDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // NUEVO: Lista de cotizaciones vinculadas (JSONB)
    [Column(TypeName = "jsonb")]
    public List<QuotationReference> LinkedQuotations { get; set; } = new List<QuotationReference>();

    // NUEVO: Lista simplificada de productos (JSONB) - opcional
    [Column(TypeName = "jsonb")]
    public List<SingleProductJson>? Products { get; set; }

    // NUEVO: Datos de nota externa (JSONB) - opcional
    [Column(TypeName = "jsonb")]
    public AuxNoteDataJson? AuxNoteDataJson { get; set; }

    // FK a ConfigSys (si es necesaria)
    public int IdConfigSys { get; set; }

    // Propiedades de navegación
    public Customer? Customer { get; set; }
    public ConfigSys? ConfigSys { get; set; }

    // Propiedades calculadas (no se mapean a la BD)
    [NotMapped]
    public string? FirstQuotationFolio => LinkedQuotations?.Count > 0 ? LinkedQuotations[0].QuotationFolio.ToString() : null;

    [NotMapped]
    public bool HasLinkedQuotations => LinkedQuotations?.Count > 0;

    [NotMapped]
    public bool HasProducts => Products?.Count > 0;

    [NotMapped]
    public bool HasAuxNoteDataJson => AuxNoteDataJson != null;
}

public class QuotationReference
{
    public int QuotationId { get; set; }
    public int QuotationFolio { get; set; }
    public DateTime LinkedDate { get; set; } = DateTime.UtcNow;
    public string LinkedBy { get; set; } = string.Empty;
}

public class AuxNoteDataJson
{
    public string Cliente { get; set; } = string.Empty;
    public string NombreCliente { get; set; } = string.Empty;
    public string Folio { get; set; } = string.Empty;
    public string Fecha { get; set; } = string.Empty;
    public string Hora { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public string Caja { get; set; } = string.Empty;
    public string Zn { get; set; } = string.Empty;
    public string Nf { get; set; } = string.Empty;
    public string Agente { get; set; } = string.Empty;
    public string DireccionCliente { get; set; } = string.Empty;
    public string PoblacionCliente { get; set; } = string.Empty;
    public string EmailCliente { get; set; } = string.Empty;
    public string TelCliente { get; set; } = string.Empty;
    public decimal Importe { get; set; }
    public decimal Descuento { get; set; }
    public decimal Impuesto { get; set; }
    public decimal Total { get; set; }
    public bool ExisteEnDB { get; set; }
}

