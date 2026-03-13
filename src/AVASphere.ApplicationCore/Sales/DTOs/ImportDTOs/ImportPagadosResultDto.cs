namespace AVASphere.ApplicationCore.Sales.DTOs.ImportDTOs;

/// <summary>
/// Resultado de la importación del archivo <c>PAGADOS.xlsx</c>,
/// generado por <c>ISaleService.ImportFromPagadosAsync</c>.
///
/// POSIBLES ACCIONES POR REGISTRO:
/// <list type="table">
///   <item>
///     <term>Updated</term>
///     <description>
///       Se encontró una venta existente con el mismo Folio, Fecha e IdConfigSys
///       y con un cliente cuyo ExternalId coincide. Se actualizaron
///       <c>AuxNoteDataJson.ImportePagado</c> y <c>AuxNoteDataJson.Saldo</c>.
///     </description>
///   </item>
///   <item>
///     <term>Created</term>
///     <description>
///       No se encontró coincidencia; se creó una nueva venta con <c>Type="Imported-Pagados"</c>
///       y datos parciales (sin Serie, Caja, Agente, etc.).
///     </description>
///   </item>
///   <item>
///     <term>Skipped</term>
///     <description>
///       Se omitió el registro por fecha inválida o por una excepción al procesarlo.
///     </description>
///   </item>
/// </list>
/// </summary>
public class ImportPagadosResultDto
{
    /// <summary>Total de registros procesados</summary>
    public int TotalProcessed { get; set; }

    /// <summary>Registros que se actualizaron (se encontró coincidencia de Folio + Fecha + Cliente)</summary>
    public int TotalUpdated { get; set; }

    /// <summary>Registros que se crearon como nuevas ventas (no se encontró coincidencia)</summary>
    public int TotalCreated { get; set; }

    /// <summary>Registros omitidos por errores de validación o excepciones</summary>
    public int TotalSkipped { get; set; }

    /// <summary>Lista de errores ocurridos durante la importación</summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>Detalle de cada registro procesado</summary>
    public List<ImportPagadosDetailDto> Details { get; set; } = new();
}

/// <summary>
/// Detalle del resultado de procesamiento de un registro individual del archivo PAGADOS.xlsx.
/// Incluido en <see cref="ImportPagadosResultDto.Details"/> para trazabilidad completa.
/// </summary>
public class ImportPagadosDetailDto
{
    public string Folio { get; set; } = string.Empty;
    public string Fecha { get; set; } = string.Empty;
    public string NombreCliente { get; set; } = string.Empty;

    /// <summary>Acción tomada: "Updated", "Created" o "Skipped"</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Mensaje descriptivo de lo ocurrido</summary>
    public string? Message { get; set; }
}
