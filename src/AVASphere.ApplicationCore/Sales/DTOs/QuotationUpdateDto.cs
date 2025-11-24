using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AVASphere.ApplicationCore.Sales.DTOs
{
    public class QuotationUpdateDto
    {
        public int Folio { get; set; }
        public DateOnly SaleDate { get; set; }
        public StatusEnum? Status { get; set; }
        public string? GeneralComment { get; set; }
        public int CustomerId { get; set; }
        public List<string> SalesExecutives { get; set; } = new();
        public List<QuotationsFollowupDto>? Followups { get; set; }
        public List<QuotationProductDto>? Products { get; set; }
        public int IdConfigSys { get; set; }
    }

    public class QuotationsFollowupDto
    {
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string Comment { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    public class QuotationProductDto
    {
        public int ProductId { get; set; }
        public double Quantity { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string Unit { get; set; } = string.Empty;
    }
}