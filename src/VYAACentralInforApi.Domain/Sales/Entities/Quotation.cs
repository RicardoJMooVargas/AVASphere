using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VYAACentralInforApi.Domain.Sales.Entities;

public class Quotation
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string QuotationId { get; set; } = string.Empty;

    [BsonElement("saleDate")]
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;

    [BsonElement("status")]
    public string Status { get; set; } = "PENDIENTE"; // Pending, Accepted, Rejected

    [BsonElement("salesExecutives")]
    public List<string> SalesExecutives { get; set; } = new List<string>(); // User IDs, first one is the creator/owner

    [BsonElement("folio")]
    public int Folio { get; set; }

    [BsonElement("customerId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string CustomerId { get; set; } = string.Empty;

    [BsonElement("customer")]
    public Customer? Customer { get; set; }

    [BsonElement("generalComment")]
    public string? GeneralComment { get; set; }

    [BsonElement("followups")]
    public List<QuotationFollowups> Followups { get; set; } = new List<QuotationFollowups>();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}