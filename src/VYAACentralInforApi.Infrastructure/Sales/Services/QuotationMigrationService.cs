using MongoDB.Driver;
using VYAACentralInforApi.ApplicationCore.Sales.Entities;
using VYAACentralInforApi.Infrastructure.Sales.Data;
using MongoDB.Bson;

namespace VYAACentralInforApi.Infrastructure.Sales.Services;
/*
public class QuotationMigrationService
{
    private readonly IMongoCollection<BsonDocument> _quotationsCollection;
    private readonly IMongoCollection<Customer> _customersCollection;

    public QuotationMigrationService(SalesMongoDbContext context)
    {
        // Usar BsonDocument para poder acceder a campos que ya no están en el modelo
        _quotationsCollection = context.Database.GetCollection<BsonDocument>("quotations");
        _customersCollection = context.Customers;
    }

    public async Task MigrateQuotationsAsync()
    {
        Console.WriteLine("Iniciando migración de cotizaciones...");

        // Obtener todas las cotizaciones que tienen el campo 'customer' embebido
        var filter = Builders<BsonDocument>.Filter.Exists("customer");
        var quotationsWithEmbeddedCustomer = await _quotationsCollection.Find(filter).ToListAsync();

        Console.WriteLine($"Encontradas {quotationsWithEmbeddedCustomer.Count} cotizaciones para migrar...");

        foreach (var quotationDoc in quotationsWithEmbeddedCustomer)
        {
            try
            {
                var quotationId = quotationDoc["_id"].AsObjectId.ToString();
                Console.WriteLine($"Migrando cotización: {quotationId}");

                // Extraer los datos del customer embebido
                var embeddedCustomer = quotationDoc["customer"].AsBsonDocument;
                var customerData = new Customer
                {
                    CustomerId = embeddedCustomer.GetValue("customerId", "").AsString,
                    Code = embeddedCustomer.GetValue("code", "").AsString,
                    FullName = embeddedCustomer.GetValue("fullName", "").AsString,
                    Email = embeddedCustomer.GetValue("email", BsonNull.Value).IsBsonNull ? null : embeddedCustomer["email"].AsString,
                    Phones = embeddedCustomer.GetValue("phones", new BsonArray()).AsBsonArray.Select(p => p.AsString).ToList(),
                    CreatedAt = embeddedCustomer.GetValue("createdAt", DateTime.UtcNow).ToUniversalTime(),
                    Status = embeddedCustomer.GetValue("status", true).AsBoolean
                };

                // Si el CustomerId está vacío, generar uno nuevo
                if (string.IsNullOrEmpty(customerData.CustomerId))
                {
                    customerData.CustomerId = ObjectId.GenerateNewId().ToString();
                }

                // Verificar si el cliente ya existe en la colección de customers
                var existingCustomer = await _customersCollection
                    .Find(c => c.CustomerId == customerData.CustomerId)
                    .FirstOrDefaultAsync();

                if (existingCustomer == null)
                {
                    // Insertar el cliente en la colección de customers si no existe
                    await _customersCollection.InsertOneAsync(customerData);
                    Console.WriteLine($"Cliente creado: {customerData.CustomerId} - {customerData.FullName}");
                }
                else
                {
                    Console.WriteLine($"Cliente ya existe: {customerData.CustomerId} - {customerData.FullName}");
                }

                // Actualizar la cotización para usar solo el customerId y remover el campo customer embebido
                var quotationFilter = Builders<BsonDocument>.Filter.Eq("_id", quotationDoc["_id"].AsObjectId);
                var update = Builders<BsonDocument>.Update
                    .Set("customerId", customerData.CustomerId)
                    .Unset("customer") // Remover el campo customer embebido
                    .Set("updatedAt", DateTime.UtcNow);

                await _quotationsCollection.UpdateOneAsync(quotationFilter, update);
                Console.WriteLine($"Cotización actualizada: {quotationId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error migrando cotización {quotationDoc["_id"]}: {ex.Message}");
            }
        }

        Console.WriteLine("Migración completada.");
    }
}
*/