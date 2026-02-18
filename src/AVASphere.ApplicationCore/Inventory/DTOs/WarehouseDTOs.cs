namespace AVASphere.ApplicationCore.Inventory.DTOs;

public class WarehouseRequestDto
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Location { get; set; }
    public double IsMain { get; set; }
    public double Active { get; set; }
}

public class WarehouseResponseDto
{
    public int IdWarehouse { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Location { get; set; }
    public double IsMain { get; set; }
    public double Active { get; set; }
}