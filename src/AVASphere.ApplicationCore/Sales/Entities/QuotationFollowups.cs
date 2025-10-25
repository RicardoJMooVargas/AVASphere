using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AVASphere.ApplicationCore.Sales.Entities;
// CONVERTIR A JSON DENTRO DE COTIZACIONES
public class QuotationFollowups
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("date")]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    [BsonElement("comment")]
    public string Comment { get; set; } = string.Empty;

    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty; // ID del usuario que hace el seguimiento

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}