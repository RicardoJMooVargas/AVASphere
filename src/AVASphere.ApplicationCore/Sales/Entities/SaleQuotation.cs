using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AVASphere.ApplicationCore.Common.Entities.General;
using AVASphere.ApplicationCore.Common.Entities.Jsons;
using AVASphere.ApplicationCore.Sales.DTOs;

namespace AVASphere.ApplicationCore.Sales.Entities
{
    public class SaleQuotation
    {
        [Key]
        public int IdSaleQuotation { get; set; }
        public int IdQuotation { get; set; }
        public int IdSale { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        [Column(TypeName = "jsonb")]
        public List<SingleProductJson> ProductsJson { get; set; } = new List<SingleProductJson>();
        public PriceSnapshotJson? PriceSnapshot { get; set; } = new PriceSnapshotJson();
        public string? GeneralComment { get; set; }

        [ForeignKey(nameof(IdQuotation))]
        public Quotation? Quotation { get; set; }
        
        [ForeignKey(nameof(IdSale))]
        public Sale? Sale { get; set; }
        
    }
    public class PriceSnapshotJson
    {
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Currency { get; set; }

    }
}