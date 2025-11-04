using AVASphere.ApplicationCore.Common.Entities.General;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AVASphere.ApplicationCore.Sales.Entities;

public class Sale
{
    public string SaleId { get; set; } = string.Empty;
    public string SalesExecutive { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string Type { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public Customer? Customer { get; set; }
    public string Folio { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string? DeliveryDriver { get; set; }
    public bool HomeDelivery { get; set; } = false;
    public DateTime? DeliveryDate { get; set; }
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