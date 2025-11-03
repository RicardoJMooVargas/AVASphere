using System;
using System.Collections.Generic;

namespace AVASphere.ApplicationCore.Sales.Entities;

public class Quotation
{
    public int QuotationId { get; set; }
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "PENDIENTE";
    // Lista de IDs de ejecutivos de venta; si tus user ids son ints, cambia a List<int>
    public List<string> SalesExecutives { get; set; } = new List<string>();
    public int Folio { get; set; }
    // FK al cliente; usando int según tu decisión
    public int CustomerId { get; set; }
    public string? GeneralComment { get; set; }
    // Campo que se mantendrá como JSONB en Postgres (serialización de la clase QuotationFollowupsJson)
    public List<QuotationFollowupsJson> Followups { get; set; } = new List<QuotationFollowupsJson>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    // Propiedad de navegación opcional para incluir datos del cliente (si lo necesitas)
    public Customer? Customer { get; set; }
}

public class QuotationFollowupsJson
{
    public string Id { get; set; } = string.Empty;

    public DateTime Date { get; set; } = DateTime.UtcNow;

    public string Comment { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty; // ID del usuario que hace el seguimiento

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}