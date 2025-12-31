using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AVASphere.ApplicationCore.Sales.DTOs.SaleDTOs
{
    public class CreateSaleFromQuotationsDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one quotationId is required.")]
        public List<int> QuotationIds { get; set; } = new();

        [Required]
        public string SalesExecutive { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.UtcNow;

        public string Type { get; set; } = string.Empty;

        [Required]
        public int CustomerId { get; set; }

        // Sale.Folio is a string in the entity
        public string Folio { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        public string? DeliveryDriver { get; set; }

        public bool HomeDelivery { get; set; } = false;

        public DateTime? DeliveryDate { get; set; }

        public int SatisfactionLevel { get; set; } = 0;

        public string? SatisfactionReason { get; set; }

        public string? Comment { get; set; }

        public DateTime? AfterSalesFollowupDate { get; set; }

        // Config system id (FK)
        public int IdConfigSys { get; set; } = 0;
    }
}