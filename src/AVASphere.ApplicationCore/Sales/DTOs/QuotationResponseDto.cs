namespace AVASphere.ApplicationCore.Sales.DTOs;

public class QuotationResponseDto
{
    // En Postgres (EF) QuotationId es int
    public int QuotationId { get; set; }

    public DateTime SaleDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<string> SalesExecutives { get; set; } = new();
    public int Folio { get; set; }

    // CustomerId en la DB es int
    public int CustomerId { get; set; }

    // Información opcional anidada del cliente (si la incluyes en la consulta)
    public CustomerResponseDto? Customer { get; set; }

    public string? GeneralComment { get; set; }

    // Followups vienen como JSONB; en la entidad el Id del followup es string,
    // por eso aquí usamos string para mantener compatibilidad.
    public List<QuotationFollowupResponseDto> Followups { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CustomerResponseDto
{
    // Id numérico para Postgres
    public int CustomerId { get; set; }

    // Puedes mantener un "Code" si lo usas en UI, sino quítalo.
    public string Code { get; set; } = string.Empty;

    // Nombre completo para presentación (puedes construirlo en el mapeo)
    public string FullName { get; set; } = string.Empty;

    public string? Email { get; set; }

    // Phones como lista de strings para presentación
    public List<string>? Phones { get; set; }

    public DateTime CreatedAt { get; set; }

    // Status como booleano (activo/inactivo)
    public bool Status { get; set; }
}

public class QuotationFollowupResponseDto
{
    // Mantengo string para Id porque la entidad QuotationFollowupsJson define Id como string
    public string Id { get; set; } = string.Empty;

    public DateTime Date { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class GetQuotationsQueryDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // Mantengo CustomerName como filtro textual; si filtras por id, añade CustomerId:int
    public string? CustomerName { get; set; }
    public int? Folio { get; set; }
}