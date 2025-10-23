using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AVASphere.ApplicationCore.System
{
    public class Users
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string IdUsers { get; set; } = string.Empty;

        [BsonElement("userName")]
        public string UserName { get; set; } = string.Empty;

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("lastName")]
        public string LastName { get; set; } = string.Empty;

        [BsonElement("rol")]
        public string Rol { get; set; } = string.Empty;

        [BsonElement("password")]
        public string Password { get; set; } = string.Empty;

        [BsonElement("hashPassword")]
        public string HashPassword { get; set; } = string.Empty;

        [BsonElement("status")]
        public bool Status { get; set; } = true;

        [BsonElement("aux")]
        public string Aux { get; set; } = string.Empty;

        [BsonElement("createAt")]
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        [BsonElement("verified")]
        public bool Verified { get; set; } = false;
    }
}