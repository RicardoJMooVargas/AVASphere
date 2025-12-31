// ChartResponseDTOs.cs
using System.Collections.Generic;

namespace AVASphere.ApplicationCore.Sales.DTOs.ChartDTOs
{
    public class SalesSummaryResponse
    {
        public decimal TotalAmount { get; set; }
        public int TotalSalesCount { get; set; }
        public List<SalesSummaryDetail> Details { get; set; } = new List<SalesSummaryDetail>();
        public ChartMetadata Metadata { get; set; } = new ChartMetadata();
    }

    public class SalesSummaryDetail
    {
        public string Period { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int SalesCount { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
    }

    public class SalesByAgentResponse
    {
        public List<AgentSalesDetail> Agents { get; set; } = new List<AgentSalesDetail>();
        public decimal TotalAmount { get; set; }
        public ChartMetadata Metadata { get; set; } = new ChartMetadata();
    }

    public class AgentSalesDetail
    {
        public string AgentName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int SalesCount { get; set; }
        public List<CustomerSalesDetail> CustomerDetails { get; set; } = new List<CustomerSalesDetail>();
    }

    public class CustomerSalesDetail
    {
        public string CustomerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int SalesCount { get; set; }
    }

    public class SalesByProductResponse
    {
        public List<ProductSalesDetail> Products { get; set; } = new List<ProductSalesDetail>();
        public ChartMetadata Metadata { get; set; } = new ChartMetadata();
        public List<SalesFrequency> TopFrequencies { get; set; } = new List<SalesFrequency>();
    }

    public class ProductSalesDetail
    {
        public string ProductName { get; set; } = string.Empty;
        public double TotalQuantity { get; set; }
        public decimal TotalAmount { get; set; }
        public int SalesCount { get; set; }
        public List<PeriodQuantity> PeriodQuantities { get; set; } = new List<PeriodQuantity>();
    }

    public class PeriodQuantity
    {
        public string Period { get; set; } = string.Empty;
        public double Quantity { get; set; }
        public decimal Amount { get; set; }
    }

    public class SalesFrequency
    {
        public string Period { get; set; } = string.Empty;
        public int Frequency { get; set; }
        public string ProductName { get; set; } = string.Empty;
    }

    public class ChartMetadata
    {
        public TypeFilter Type { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public DateTime? SpecificDate { get; set; }
        public List<string> AppliedFilters { get; set; } = new List<string>();
    }
}