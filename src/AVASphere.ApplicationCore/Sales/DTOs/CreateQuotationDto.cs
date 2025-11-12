using System.ComponentModel.DataAnnotations;
using AVASphere.ApplicationCore.Common.Attributes;
using AVASphere.ApplicationCore.Common.Entities.Jsons;

namespace AVASphere.ApplicationCore.Sales.DTOs;

public class CreateQuotationDto
{
    [Required]
    public int Folio { get; set; }

    public DateTime? SaleDate { get; set; } = DateTime.UtcNow;

    public string? Status { get; set; } = "PENDIENTE";

    public string? GeneralComment { get; set; }

    // Id del cliente requerido
    [Required]
    public int CustomerId { get; set; }

    // Lista de ejecutivos de venta (se serializa a JSONB en la entidad)
    public List<string>? SalesExecutives { get; set; }

    // Followups iniciales (se serializa a JSONB)
    public List<QuotationFollowupDto>? Followups { get; set; }

    // Lista de productos simplificada (se serializa a JSONB)
    public List<SingleProductJson>? Products { get; set; }

    // Configuración del sistema (si aplica)
    public int IdConfigSys { get; set; } = 0;
}

public class QuotationFollowupDto
{
    public DateTime? Date { get; set; }

    [Required]
    public string Comment { get; set; } = string.Empty;

    public string? UserId { get; set; }
}
