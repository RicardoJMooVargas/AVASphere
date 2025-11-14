﻿using System.Globalization;
using AVASphere.ApplicationCore.Common.Entities.Jsons;
using AVASphere.ApplicationCore.Sales.Entities;
namespace AVASphere.ApplicationCore.Common.Entities.General;


public class Customer
{
    public int IdCustomer { get; set; }
    public int ExternalId { get; set; }
    public string? Name { get; set; }
    public string? LastName { get; set; }
    public int PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? TaxId { get; set; }
    // JSON
    public SettingsCustomerJson? SettingsCustomerJson { get; set; } = null!;
    public DirectionJson DirectionJson { get; set; } = null!;
    public PaymentMethodsJson? PaymentMethodsJson { get; set; } = null!;
    public PaymentTermsJson? PaymentTermsJson { get; set; } = null!;
    // FALTARIAN CFDI Y REGIMEN FISCAL ??

    // Relaciones
    public List<Quotation> Quotations { get; set; } = new List<Quotation>();
    public List<Sale> Sales { get; set; } = new List<Sale>();



    // Devuelve ExternalId como string.
    /* Si se necesita un formato o proveedor lo puedes pasar en la sobrecarga.

    public string GetExternalIdAsString()
    {
        return ExternalId.ToString(CultureInfo.InvariantCulture);
    }
     Devuelve ExternalId formateado con el formato especificado (por ejemplo "D6") y proveedor opcional.

    public string GetExternalIdAsString(string format, IFormatProvider? provider = null)
    {
        if (string.IsNullOrEmpty(format))
            return GetExternalIdAsString();

        return ExternalId.ToString(format, provider ?? CultureInfo.InvariantCulture);
    }*/

    // Devuelve ExternalId como string rellenado a totalWidth con paddingChar (ej. "0000123").

    public string GetExternalIdPadded(int totalWidth, char paddingChar = '0')
    {
        return ExternalId.ToString(CultureInfo.InvariantCulture).PadLeft(totalWidth, paddingChar);
    }
}

public class SettingsCustomerJson
{
    public int Index { get; set; }
    public string? Route { get; set; }
    public string Type { get; set; } = "General"; // aluminiero , general, etc.
    public double Discount { get; set; } = 0.0;
}

