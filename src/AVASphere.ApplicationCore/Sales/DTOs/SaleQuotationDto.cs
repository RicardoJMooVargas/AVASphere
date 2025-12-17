using System;
using System.Collections.Generic;
using AVASphere.ApplicationCore.Sales.Entities;
using AVASphere.ApplicationCore.Common.Entities.Jsons;

namespace AVASphere.ApplicationCore.Sales.DTOs
{
    /// <summary>
    /// DTO para la entidad SaleQuotation
    /// Mapea la relación N:N entre Sales y Quotations
    /// </summary>
    public class SaleQuotationDto
    {
        public int IdSaleQuotation { get; set; }
        public int IdQuotation { get; set; }
        public int IdSale { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Indica si esta es la cotización principal del sale
        /// No requiere migración de BD - solo uso en aplicación
        /// </summary>
        public bool IsPrimary { get; set; } = false;

        public List<SingleProductJson> ProductsJson { get; set; } = new List<SingleProductJson>();
        public PriceSnapshotJson? PriceSnapshot { get; set; } = new PriceSnapshotJson();
        public string? GeneralComment { get; set; }

        // Propiedades de navegación opcionales
        public string? QuotationNumber { get; set; }
        public string? SaleFolio { get; set; }
    }

    /// <summary>
    /// DTO simplificado para listados de SaleQuotation
    /// </summary>
    public class SaleQuotationSummaryDto
    {
        public int IdSaleQuotation { get; set; }
        public int IdQuotation { get; set; }
        public int IdSale { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public bool IsPrimary { get; set; }
        public string? GeneralComment { get; set; }
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// DTO para respuesta de creación de SaleQuotation
    /// </summary>
    public class SaleQuotationResponseDto
    {
        public int IdSaleQuotation { get; set; }
        public int IdQuotation { get; set; }
        public int IdSale { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? Message { get; set; }
        public bool Success { get; set; }
        public PriceSnapshotJson? PriceSnapshot { get; set; }
    }
}
