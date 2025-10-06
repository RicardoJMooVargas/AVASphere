using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VYAACentralInforApi.ApplicationCore.Sales.Entities;

public class Sale
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string SaleId { get; set; } = string.Empty;

    [BsonElement("salesExecutive")]
    public string SalesExecutive { get; set; } = string.Empty;

    [BsonElement("date")]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    [BsonElement("type")]
    public string Type { get; set; } = string.Empty;

    [BsonElement("customerId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string CustomerId { get; set; } = string.Empty;

    [BsonElement("customer")]
    public Customer? Customer { get; set; }

    [BsonElement("folio")]
    public string Folio { get; set; } = string.Empty;

    [BsonElement("totalAmount")]
    public decimal TotalAmount { get; set; }

    [BsonElement("deliveryDriver")]
    public string? DeliveryDriver { get; set; }

    [BsonElement("homeDelivery")]
    public bool HomeDelivery { get; set; } = false;

    [BsonElement("deliveryDate")]
    public DateTime? DeliveryDate { get; set; }

    [BsonElement("satisfactionLevel")]
    public int SatisfactionLevel { get; set; } = 0; // 1-5 stars

    [BsonElement("satisfactionReason")]
    public string? SatisfactionReason { get; set; }

    [BsonElement("comment")]
    public string? Comment { get; set; }

    [BsonElement("afterSalesFollowupDate")]
    public DateTime? AfterSalesFollowupDate { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}