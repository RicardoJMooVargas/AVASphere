namespace AVASphere.ApplicationCore.Sales.DTOs.ImportDTOs;

/// <summary>
/// DTO raíz que representa el JSON completo generado al pre-procesar el archivo <c>PAGADOS.xlsx</c>.
///
/// ESTRUCTURA DEL EXCEL:
/// El archivo PAGADOS.xlsx del sistema InforAVA contiene ventas agrupadas en bloques de ~51 registros
/// cada uno. El pre-procesador (script externo) convierte este Excel a este JSON antes de enviarlo
/// al endpoint <c>POST /api/SaleManager/ImportFromPagados</c>.
///
/// CAMPOS GLOBALES:
/// <list type="table">
///   <item><term>FileName</term><description>Nombre original del archivo procesado (ej. "PAGADOS.xlsx").</description></item>
///   <item><term>ProcessedAt</term><description>Fecha y hora en que el script generó el JSON.</description></item>
///   <item><term>TotalRecords</term><description>Total de filas de datos en todo el archivo.</description></item>
///   <item><term>TotalBlocks</term><description>Número de bloques en que está dividido el archivo.</description></item>
///   <item><term>TotalImporte</term><description>Suma de todos los importes del archivo.</description></item>
///   <item><term>TotalPagado</term><description>Suma de todos los montos pagados del archivo.</description></item>
///   <item><term>TotalSaldo</term><description>Suma de todos los saldos pendientes del archivo.</description></item>
/// </list>
/// </summary>
public class ImportPagadosRequestDto
{
    public string FileName { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
    public int TotalRecords { get; set; }
    public int TotalBlocks { get; set; }
    public decimal TotalImporte { get; set; }
    public decimal TotalPagado { get; set; }
    public decimal TotalSaldo { get; set; }
    public List<ImportPagadosBlockDto> Blocks { get; set; } = new();
}

/// <summary>
/// DTO que representa un bloque de registros del archivo <c>PAGADOS.xlsx</c>.
/// Cada bloque agrupa un rango de filas del Excel (ej. filas 9-59, 68-118, etc.)
/// con sus propios subtotales de importe, pagado y saldo.
/// </summary>
public class ImportPagadosBlockDto
{
    public int BlockNumber { get; set; }
    public int StartRow { get; set; }
    public int EndRow { get; set; }
    public int TotalRecords { get; set; }
    public decimal TotalImporte { get; set; }
    public decimal TotalPagado { get; set; }
    public decimal TotalSaldo { get; set; }
    public List<ImportPagadosRecordDto> Records { get; set; } = new();
}

/// <summary>
/// DTO que representa un registro individual (fila) del archivo <c>PAGADOS.xlsx</c>,
/// correspondiente a una nota de venta del sistema InforAVA.
///
/// CAMPO CRÍTICO - NombreCliente:
/// Viene en el formato <c>"000055 PUBLICO GENERAL"</c> donde:
/// <list type="bullet">
///   <item>Los primeros dígitos (con ceros al frente) = ExternalId del cliente.</item>
///   <item>El resto = nombre del cliente en el sistema externo.</item>
/// </list>
/// Este campo es parseado por <c>SaleService.ParseNombreCliente()</c> para separar ambos valores.
///
/// LÓGICA DE NEGOCIO:
/// <list type="bullet">
///   <item><c>Importe</c>: monto total de la nota de venta.</item>
///   <item><c>ImportePagado</c>: monto efectivamente cobrado (puede ser menor al importe).</item>
///   <item><c>Saldo</c>: diferencia pendiente de cobro (<c>Importe - ImportePagado</c>).</item>
///   <item>Registros con <c>Importe = 0</c> son notas sin cargo (ej. entregas, devoluciones).</item>
/// </list>
/// </summary>
public class ImportPagadosRecordDto
{
    /// <summary>Fecha de la venta en formato dd/MM/yyyy</summary>
    public string Fecha { get; set; } = string.Empty;

    /// <summary>Datos adicionales opcionales</summary>
    public string DatosAdicionales { get; set; } = string.Empty;

    /// <summary>Folio de la nota/venta</summary>
    public string Folio { get; set; } = string.Empty;

    /// <summary>Hora de la venta en formato HH:mm:ss</summary>
    public string Hora { get; set; } = string.Empty;

    /// <summary>Nombre del cliente con formato "000055 PUBLICO GENERAL" (ExternalId + nombre)</summary>
    public string NombreCliente { get; set; } = string.Empty;

    /// <summary>Importe total de la venta</summary>
    public decimal Importe { get; set; }

    /// <summary>Monto efectivamente pagado</summary>
    public decimal ImportePagado { get; set; }

    /// <summary>Saldo pendiente</summary>
    public decimal Saldo { get; set; }
}
