namespace AVASphere.ApplicationCore.System.DTOs;

// DTOs para backup general de tablas
public class BackupTablesRequestDto
{
    public List<string> TableNames { get; set; } = new();
    public bool ExportAllTables { get; set; } = false;
    public string? BackupName { get; set; }
    public string? Description { get; set; }
    public string Format { get; set; } = "SQL"; // SQL, JSON
}

public class BackupTablesResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string BackupId { get; set; } = string.Empty;
    public string BackupName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? Description { get; set; }
    public List<string> ProcessedTables { get; set; } = new();
    public List<string> SkippedTables { get; set; } = new();
    public int TotalRecords { get; set; }
}

public class BackupImportResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool OverwriteMode { get; set; }
    public List<string> ExecutedStatements { get; set; } = new();
    public List<string> SkippedStatements { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}

public class AvailableTablesDto
{
    public List<string> Tables { get; set; } = new();
    public int TotalCount { get; set; }
    public DateTime RetrievedAt { get; set; } = DateTime.UtcNow;
}

// DTOs legacy mantenidos para compatibilidad hacia atrás
public class CatalogBackupRequestDto
{
    public bool IncludeProperties { get; set; } = true;
    public bool IncludeSuppliers { get; set; } = true;
    public bool IncludePropertyValues { get; set; } = true;
    public string? BackupName { get; set; }
    public string? Description { get; set; }
}

public class CatalogBackupResponseDto
{
    public string BackupId { get; set; } = string.Empty;
    public string BackupName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? Description { get; set; }
    public BackupDataDto Data { get; set; } = new();
    public BackupStatsDto Stats { get; set; } = new();
}

public class BackupDataDto
{
    public List<PropertyBackupDto> Properties { get; set; } = new();
    public List<SupplierBackupDto> Suppliers { get; set; } = new();
    public List<PropertyValueBackupDto> PropertyValues { get; set; } = new();
}

public class BackupStatsDto
{
    public int PropertiesCount { get; set; }
    public int SuppliersCount { get; set; }
    public int PropertyValuesCount { get; set; }
    public int TotalRecords { get; set; }
}

public class PropertyBackupDto
{
    public int IdProperty { get; set; }
    public string? Name { get; set; }
    public string? NormalizedName { get; set; }
}

public class SupplierBackupDto
{
    public int IdSupplier { get; set; }
    public string? Name { get; set; }
    public string? CompanyName { get; set; }
    public string? TaxId { get; set; }
    public string? PersonType { get; set; }
    public string? BusinessId { get; set; }
    public string? CurrencyCoin { get; set; }
    public double? DeliveryDays { get; set; }
    public DateOnly RegistrationDate { get; set; }
    public string? Observations { get; set; }
}

public class PropertyValueBackupDto
{
    public int IdPropertyValue { get; set; }
    public string? Value { get; set; }
    public int? FatherValue { get; set; }
    public string? Type { get; set; }
    public int IdProperty { get; set; }
}

// DTOs para restore de catálogos
public class CatalogRestoreRequestDto
{
    public BackupDataDto Data { get; set; } = new();
    public RestoreOptionsDto Options { get; set; } = new();
}

public class RestoreOptionsDto
{
    public bool ClearExistingData { get; set; } = false;
    public bool SkipDuplicates { get; set; } = true;
    public bool UpdateExisting { get; set; } = false;
    public bool RestoreProperties { get; set; } = true;
    public bool RestoreSuppliers { get; set; } = true;
    public bool RestorePropertyValues { get; set; } = true;
}

public class CatalogRestoreResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public RestoreStatsDto Stats { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

public class RestoreStatsDto
{
    public int PropertiesProcessed { get; set; }
    public int PropertiesInserted { get; set; }
    public int PropertiesUpdated { get; set; }
    public int PropertiesSkipped { get; set; }
    
    public int SuppliersProcessed { get; set; }
    public int SuppliersInserted { get; set; }
    public int SuppliersUpdated { get; set; }
    public int SuppliersSkipped { get; set; }
    
    public int PropertyValuesProcessed { get; set; }
    public int PropertyValuesInserted { get; set; }
    public int PropertyValuesUpdated { get; set; }
    public int PropertyValuesSkipped { get; set; }
    
    public int TotalProcessed { get; set; }
    public int TotalInserted { get; set; }
    public int TotalUpdated { get; set; }
    public int TotalSkipped { get; set; }
}
