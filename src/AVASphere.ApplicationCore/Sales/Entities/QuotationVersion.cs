using System.ComponentModel.DataAnnotations.Schema;
using AVASphere.ApplicationCore.Common.Entities.General;
using AVASphere.ApplicationCore.Common.Entities.Jsons;
using System.Linq;

namespace AVASphere.ApplicationCore.Sales.Entities;

public class QuotationVersion
{
    public int QuotationVersionId { get; set; }
    public int VersionNumber { get; set; }
    public decimal? Subtotal { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? GeneralComment { get; set; }

    // NUEVO: Lista simplificada de productos (JSONB) - opcional
    [Column(TypeName = "jsonb")]
    public List<SingleProductJson> Products { get; set; } = new List<SingleProductJson>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public int IdQuotation { get; set; }

    // Versión completa de la cotización en formato JSONB
    [Column(TypeName = "jsonb")]

    public Quotation QuotationData { get; set; } = new Quotation();

    // Navegación (opcional)
    [ForeignKey(nameof(IdQuotation))]
    public Quotation? Quotation { get; set; }
    public IReadOnlyList<SingleProductJson> GetProducts() => Products.AsReadOnly();


    // Añade un producto o actualiza si existe (por ProductId o Description)
    public void AddOrUpdateProduct(SingleProductJson product)
    {
        if (product == null) return;

        var existing = FindProductReference(product);
        if (existing != null)
        {
            // Actualiza propiedades relevantes
            existing.Quantity = product.Quantity;
            existing.UnitPrice = product.UnitPrice;
            existing.Description = product.Description;
            existing.Unit = product.Unit;

            // Recalcula totales de línea (simplemente recalcula TotalPrice)
            existing.TotalPrice = (decimal)existing.Quantity * existing.UnitPrice;
        }
        else
        {
            // Calcula total de línea antes de añadir
            product.TotalPrice = (decimal)product.Quantity * product.UnitPrice;
            Products.Add(product);
        }

        RecalculateTotals();
    }

    public bool RemoveProductById(int productId)
    {
        var existing = Products.FirstOrDefault(p => p.ProductId.HasValue && p.ProductId.Value == productId);
        if (existing == null) return false;
        Products.Remove(existing);
        RecalculateTotals();
        return true;
    }

    // No usamos SKU, por tanto se omite completamente
    public bool RemoveProductByDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description)) return false;
        var existing = Products.FirstOrDefault(p => string.Equals(p.Description, description, StringComparison.OrdinalIgnoreCase));
        if (existing == null) return false;
        Products.Remove(existing);
        RecalculateTotals();
        return true;
    }

    // Busca referencia considerada "igual" para actualización
    private SingleProductJson? FindProductReference(SingleProductJson product)
    {

        if (product.ProductId.HasValue && product.ProductId.Value != 0)
        {
            return Products.FirstOrDefault(p => p.ProductId.HasValue && p.ProductId.Value == product.ProductId.Value);
        }

        if (!string.IsNullOrEmpty(product.Description))
        {
            return Products.FirstOrDefault(p => string.Equals(p.Description, product.Description, StringComparison.OrdinalIgnoreCase));
        }

        return null;
    }

    // Recalcula totales de la cotización a partir de los productos
    public void RecalculateTotals(decimal taxRate = 0m)
    {
        // Subtotal: suma de subtotales de líneas (asumimos TotalPrice incluye impuestos si así lo prefieres).
        // Si deseas separar subtotal / impuesto / total, cambia SingleProductJson para exponer LineSubtotal y LineTax.
        Subtotal = Products.Sum(p => p.TotalPrice);
        Subtotal = Subtotal.HasValue ? Decimal.Round(Subtotal.Value, 2, MidpointRounding.AwayFromZero) : 0m;

        // Si quieres aplicar una tasa global, la puedes calcular así (opcional):
        if (taxRate > 0m)
        {
            var subtotalValue = Products.Sum(p => Decimal.Round((decimal)p.Quantity * (decimal)p.UnitPrice, 2, MidpointRounding.AwayFromZero));
            TaxAmount = Decimal.Round(subtotalValue * taxRate, 2, MidpointRounding.AwayFromZero);
            TotalAmount = Decimal.Round(subtotalValue + (TaxAmount ?? 0m), 2, MidpointRounding.AwayFromZero);
        }
        else
        {
            // Por defecto dejamos TaxAmount = 0 y TotalAmount = Subtotal
            TaxAmount = 0m;
            TotalAmount = Subtotal;
        }
    }

}