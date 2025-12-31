namespace AVASphere.ApplicationCore.Sales.DTOs.ImportDTOs;

/// <summary>
/// Resultado de la importación masiva de ventas del sistema externo.
/// 
/// PROPÓSITO:
/// Proporcionar estadísticas detalladas del proceso de importación para:
/// - Monitoreo del progreso
/// - Detección de errores
/// - Auditoría de importaciones
/// - Reportes de rendimiento
/// </summary>
public class ImportSalesResult
{
    /// <summary>
    /// Período importado.
    /// </summary>
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// Estadísticas generales.
    /// </summary>
    public int TotalSalesFound { get; set; } = 0;
    public int TotalSalesImported { get; set; } = 0;
    public int TotalSalesSkipped { get; set; } = 0;
    public int TotalSalesError { get; set; } = 0;
    
    /// <summary>
    /// Estadísticas de clientes.
    /// </summary>
    public int CustomersFound { get; set; } = 0;
    public int CustomersCreated { get; set; } = 0;
    public int CustomersReused { get; set; } = 0;
    
    /// <summary>
    /// Información de rendimiento.
    /// </summary>
    public TimeSpan TotalProcessingTime { get; set; }
    public int BatchesProcessed { get; set; } = 0;
    public double AverageTimePerBatch { get; set; } = 0;
    
    /// <summary>
    /// Estado de la importación.
    /// </summary>
    public bool IsSuccessful { get; set; } = false;
    public string? Message { get; set; }
    
    /// <summary>
    /// Errores encontrados durante la importación.
    /// </summary>
    public List<ImportErrorDetail> Errors { get; set; } = new List<ImportErrorDetail>();
    
    /// <summary>
    /// Ventas que fueron omitidas (ya existían).
    /// </summary>
    public List<string> SkippedSales { get; set; } = new List<string>();
    
    /// <summary>
    /// Resumen por lotes procesados.
    /// </summary>
    public List<BatchProcessingSummary> BatchSummaries { get; set; } = new List<BatchProcessingSummary>();
}

/// <summary>
/// Detalle de errores durante la importación.
/// </summary>
public class ImportErrorDetail
{
    public string? SaleFolio { get; set; }
    public DateTime? SaleDate { get; set; }
    public string? CustomerName { get; set; }
    public string ErrorType { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime ErrorTimestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Resumen de procesamiento por lote.
/// </summary>
public class BatchProcessingSummary
{
    public int BatchNumber { get; set; }
    public DateTime BatchStartDate { get; set; }
    public DateTime BatchEndDate { get; set; }
    public int SalesProcessed { get; set; } = 0;
    public int SalesImported { get; set; } = 0;
    public int SalesSkipped { get; set; } = 0;
    public int SalesError { get; set; } = 0;
    public TimeSpan ProcessingTime { get; set; }
    public bool IsSuccessful { get; set; } = false;
    public string? Message { get; set; }
}
