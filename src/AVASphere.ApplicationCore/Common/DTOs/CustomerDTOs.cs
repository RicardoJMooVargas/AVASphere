﻿using AVASphere.ApplicationCore.Common.Entities.Jsons;
using AVASphere.ApplicationCore.Common.Entities.General;

namespace AVASphere.ApplicationCore.Common.DTOs;

// Filtros de búsqueda para Customer (todos opcionales)
public class CustomerFilterDto
{
    public int? IdCustomer { get; set; }
    public string? LastName { get; set; }
    public int? ExternalId { get; set; }
}

// DTO para búsqueda inteligente de Customer
public class CustomerSearchDto
{
    public string SearchText { get; set; } = string.Empty;
}

// DTOs específicos para requests (sin Index)
public class CustomerSettingsDto
{
    public string? Route { get; set; }
    public string Type { get; set; } = "General";
    public double Discount { get; set; } = 0.0;
}

public class CustomerDirectionDto
{
    public string? InteriorNumber { get; set; }
    public string? ExteriorNumber { get; set; }
    public string? NeighboringStreet { get; set; }
    public string? NeighboringStreet2 { get; set; }
    public string? Colony { get; set; }
    public string? City { get; set; }
    public string? Municipality { get; set; }
}

public class CustomerPaymentMethodDto
{
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Bank { get; set; }
    public int AccountNumber { get; set; }
    public string? ReferencePayment { get; set; }
    public string? Currency { get; set; }
}

public class CustomerPaymentTermsDto
{
    public string? PaymentType { get; set; }
    public DateTime ExpirationDate { get; set; }
    public string? TypeOfCurrency { get; set; }
}

// DTO de creación de Customer
public class CustomerCreateRequest
{
    public int ExternalId { get; set; }
    public string? Name { get; set; }
    public string? LastName { get; set; }
    public string PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? TaxId { get; set; }

    // JSON: se pueden enviar vacíos (null) - el Index se asigna automáticamente
    public CustomerSettingsDto? Settings { get; set; }
    public CustomerDirectionDto? Direction { get; set; }
    public CustomerPaymentMethodDto? PaymentMethod { get; set; }
    public CustomerPaymentTermsDto? PaymentTerms { get; set; }
}

// DTO de actualización parcial/múltiple de Customer
public class CustomerUpdateRequest
{
    // Identificador obligatorio para actualizar
    public int IdCustomer { get; set; }

    // Campos opcionales: solo se actualizan los que NO son null
    public int? ExternalId { get; set; }
    public string? Name { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? TaxId { get; set; }

    // JSON opcionales: si no son null, se reemplazan completamente - el Index se maneja automáticamente
    public CustomerSettingsDto? Settings { get; set; }
    public CustomerDirectionDto? Direction { get; set; }
    public CustomerPaymentMethodDto? PaymentMethod { get; set; }
    public CustomerPaymentTermsDto? PaymentTerms { get; set; }
}

// DTO de respuesta de Customer
public class CustomerDto
{
    public int IdCustomer { get; set; }
    public int ExternalId { get; set; }
    public string? Name { get; set; }
    public string? LastName { get; set; }
    public string PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? TaxId { get; set; }

    public SettingsCustomerJson? SettingsCustomerJson { get; set; }
    public DirectionJson? DirectionJson { get; set; }
    public PaymentMethodsJson? PaymentMethodsJson { get; set; }
    public PaymentTermsJson? PaymentTermsJson { get; set; }
}
