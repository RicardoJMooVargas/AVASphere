namespace AVASphere.ApplicationCore.Sales.DTOs.ImportDTOs;

/// <summary>
/// Resultado completo de la importación masiva de ventas del sistema externo InforAVA
/// para un mes completo (generado por <c>ISaleService.ImportSalesForMonthAsync</c>).
///
/// PROPÓSITO:
/// Proporcionar estadísticas detalladas del proceso para:
/// <list type="bullet">
///   <item>Monitoreo y trazabilidad de cada importación.</item>
///   <item>Detección de errores parciales sin bloquear el proceso completo.</item>
///   <item>Auditoría: qué se importó, cuántos clientes se crearon, tiempos de ejecución.</item>
///   <item>Reportes y métricas de rendimiento del sistema de integración.</item>
/// </list>
///
/// FLUJO DE GENERACIÓN:
/// 1. El servicio crea una instancia con <c>StartDate</c>/<c>EndDate</c> del mes.
/// 2. Por cada lote procesado se agrega un <see cref="BatchProcessingSummary"/> a <c>BatchSummaries</c>
///    y se acumulan los contadores <c>TotalSales*</c>.
/// 3. Al finalizar, se calculan las métricas de clientes y tiempo.
/// 4. <c>IsSuccessful</c> es <c>true</c> solo si <c>TotalSalesError == 0</c>.
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
/// Detalle de un error individual ocurrido durante la importación de una venta.
/// Se agrega a <see cref="ImportSalesResult.Errors"/> cuando una venta no puede procesarse.
///
/// Un error individual no cancela el lote ni la importación completa:
/// el proceso continúa con las ventas siguientes.
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
/// Resumen de procesamiento de un lote de días dentro de la importación mensual.
/// Se genera uno por cada rango de fechas procesado en <c>ProcessDateBatchAsync</c>.
///
/// CAMPOS CLAVE:
/// <list type="table">
///   <item><term>BatchNumber</term><description>Número secuencial del lote (1, 2, 3…).</description></item>
///   <item><term>BatchStartDate / BatchEndDate</term><description>Rango de fechas del lote.</description></item>
///   <item><term>SalesProcessed</term><description>Total de ventas encontradas en el sistema externo para el rango.</description></item>
///   <item><term>SalesImported</term><description>Ventas efectivamente insertadas en la BD.</description></item>
///   <item><term>SalesSkipped</term><description>Ventas omitidas porque ya existían (por folio).</description></item>
///   <item><term>SalesError</term><description>Ventas que fallaron al procesar (no detuvieron el lote).</description></item>
///   <item><term>ProcessingTime</term><description>Tiempo real que tomó este lote.</description></item>
///   <item><term>IsSuccessful</term><description><c>true</c> solo si <c>SalesError == 0</c>.</description></item>
/// </list>
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
