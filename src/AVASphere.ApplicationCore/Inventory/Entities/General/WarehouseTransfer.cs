//ACTUALIZADO A LA VERSION 0.2 DE LA DB
namespace AVASphere.ApplicationCore.Inventory.Entities.General;

public class WarehouseTransfer
{
    public int IdWarehouseTransfer { get; set; }
    public int TransferDate { get; set; }
    public string Status { get; set; } = null!; // Pending, Completed, Cancelled
    public string? Observations { get; set; }
    public double IdWarehouseFrom { get; set; }
    public double IdWarehouseTo { get; set; }
    
    // RELACIONES
    public ICollection<WarehouseTransferDetail> WarehouseTransferDetails { get; set; } = new List<WarehouseTransferDetail>();
}

