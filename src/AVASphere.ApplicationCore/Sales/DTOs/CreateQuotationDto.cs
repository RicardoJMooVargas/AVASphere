﻿using System.ComponentModel.DataAnnotations;
using AVASphere.ApplicationCore.Common.Attributes;
using AVASphere.ApplicationCore.Common.Entities.Jsons;

namespace AVASphere.ApplicationCore.Sales.DTOs;

public class CreateQuotationDto
{
    [Required]
    public int Folio { get; set; }

    public DateOnly? SaleDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public StatusEnum? Status { get; set; } = StatusEnum.Pending;

    public string? GeneralComment { get; set; }

    // Id del cliente requerido
    [Required]
    public int CustomerId { get; set; }

    //PUEDE SER NULO: SI CUSTOMER TIENE UN ID SE REGISTRA
    //SI NEWCUSTOMER NO ESTA VACIO SE GENERA UN NUEVO CUSTOMER
    public List<NewCustomerDto>? NewCustomers { get; set; }

    // Lista de ejecutivos de venta (se serializa a JSONB en la entidad)
    public List<string>? SalesExecutives { get; set; }

    // FollowupsJson iniciales (se serializa a JSONB)
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

public class NewCustomerDto
{
    public int CustomerId { get; set; }
    public string? CodeCustomer { get; set; }
    public string? Name { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ExternalId { get; set; }
    public string? Direction { get; set; }

}
