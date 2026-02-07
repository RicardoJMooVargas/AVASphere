namespace AVASphere.ApplicationCore.Inventory.DTOs;

public class StorageStructureRequestDto
{
    public string CodeRack { get; set; } = null!;
    public bool OneSection { get; set; }
    public bool HasLevel { get; set; }
    public bool HasSubLevel { get; set; }
    public int IdWarehouse { get; set; }
    public int? IdArea { get; set; }
}

public class StorageStructureResponseDto
{
    public int IdStorageStructure { get; set; }
    public string CodeRack { get; set; } = null!;
    public bool OneSection { get; set; }
    public bool HasLevel { get; set; }
    public bool HasSubLevel { get; set; }
    public int IdWarehouse { get; set; }
    public string WarehouseName { get; set; } = null!;
    public int? IdArea { get; set; }
    public string? AreaName { get; set; }
    public int LocationDetailsCount { get; set; }
}
