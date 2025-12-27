﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AVASphere.ApplicationCore.Common.Attributes;
using AVASphere.ApplicationCore.Common.Entities.Jsons;

namespace AVASphere.ApplicationCore.Sales.DTOs;

public class CreateQuotationDto
{
    [Required]
    [JsonPropertyName("folio")]
    public int Folio { get; set; }

    [JsonPropertyName("saleDate")]
    public DateOnly? SaleDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    [JsonPropertyName("status")]
    public StatusEnum? Status { get; set; } = StatusEnum.Pending;

    [JsonPropertyName("generalComment")]
    public string? GeneralComment { get; set; }

    // Id del cliente requerido
    [Required]
    [JsonPropertyName("customerId")]
    public int CustomerId { get; set; }

    //PUEDE SER NULO: SI CUSTOMER TIENE UN ID SE REGISTRA
    //SI NEWCUSTOMER NO ESTA VACIO SE GENERA UN NUEVO CUSTOMER
    [JsonPropertyName("newCustomers")]
    public List<NewCustomerDto>? NewCustomers { get; set; }

    // Lista de ejecutivos de venta (se serializa a JSONB en la entidad)
    [JsonPropertyName("salesExecutives")]
    public List<string>? SalesExecutives { get; set; }

    // FollowupsJson iniciales (se serializa a JSONB)
    [JsonPropertyName("followups")]
    public List<QuotationFollowupDto>? Followups { get; set; }

    // Lista de productos simplificada (se serializa a JSONB)
    [JsonPropertyName("products")]
    public List<SingleProductJson>? Products { get; set; }

    // Configuración del sistema
    [JsonPropertyName("idConfigSys")]
    public int IdConfigSys { get; set; } = 0;
}

public class QuotationFollowupDto
{
    [JsonPropertyName("date")]
    public DateTime? Date { get; set; }

    [Required]
    [JsonPropertyName("comment")]
    public string Comment { get; set; } = string.Empty;

    [JsonPropertyName("userId")]
    public string? UserId { get; set; }
}

public class NewCustomerDto
{
    [JsonPropertyName("codeCustomer")]
    public string? CodeCustomer { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("phoneNumber")]
    public string? PhoneNumber { get; set; }

    [JsonPropertyName("externalId")]
    public string? ExternalId { get; set; }

    [JsonPropertyName("direction")]
    public string? Direction { get; set; }
}
