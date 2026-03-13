using AVASphere.ApplicationCore.Common.Entities.Products;

namespace AVASphere.ApplicationCore.Inventory.Entities.General;

public class WarehouseTransferDetail
{
    public int IdTransferDetail { get; set; }
    public int TransferDate { get; set; }
    public double Quantity { get; set; }
    
    // FK
    public int IdProduct { get; set; }
    public Product Product { get; set; } = null!;
    
    public int IdWarehouseTransfer { get; set; }
    public WarehouseTransfer WarehouseTransfer { get; set; } = null!;
}

