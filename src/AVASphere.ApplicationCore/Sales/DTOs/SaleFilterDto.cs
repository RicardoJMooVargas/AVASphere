using System;

namespace AVASphere.ApplicationCore.Sales.DTOs;

/// <summary>
/// DTO para filtrar ventas externas combinadas.
/// 
/// PROPÓSITO:
/// Centralizar todos los filtros disponibles para el endpoint GET /api/SaleManager/GetSalesExternal.
/// Permite búsquedas complejas sin agregar múltiples parámetros al endpoint.
/// 
/// MOTIVO:
/// - ESCALABILIDAD: Agregar nuevos filtros sin cambiar la firma del endpoint
/// - CLARIDAD: Todos los filtros disponibles están documentados en un lugar
/// - REUTILIZACIÓN: Otros servicios pueden usar este DTO para búsquedas
/// </summary>
public class SaleFilterDto
{
    /// <summary>
    /// Catálogo del sistema externo (ej: "001", "PRINCIPAL", "AVA01").
    /// OBLIGATORIO para búsquedas en sistema externo.
    /// </summary>
    public string? Catalogo { get; set; }

    /// <summary>
    /// Fecha a consultar en formato YYYY-MM-DD.
    /// Si no se especifica, usa la fecha actual.
    /// </summary>
    public DateTime? Fecha { get; set; }

    /// <summary>
    /// Búsqueda especial inteligente.
    /// 
    /// LÓGICA:
    /// - Si contiene SOLO NÚMEROS → Busca por Folio (interno o externo)
    /// - Si contiene TEXTO → Busca por Nombre del Cliente
    /// - Si contiene AMBOS → Busca por texto del cliente (ignora números al inicio/final)
    /// 
    /// EJEMPLOS:
    /// - "12345" → Busca folio "12345"
    /// - "ARMALUM" → Busca cliente "ARMALUM"
    /// - "123 ARMALUM" → Busca cliente "ARMALUM"
    /// - "CLIENTE-001" → Busca cliente "CLIENTE-001"
    /// 
    /// MOTIVO: Permitir búsquedas rápidas sin precisar el campo exacto.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Filtro por nombre del cliente.
    /// Se aplica si Search no está especificado o en adición a otros filtros.
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// Filtro por folio de venta.
    /// Se aplica tanto a folios internos como externos.
    /// </summary>
    public string? Folio { get; set; }

    /// <summary>
    /// Filtro por estado de vinculación.
    /// - null: Todas las ventas (vinculadas y no vinculadas)
    /// - true: Solo ventas vinculadas (existen internamente)
    /// - false: Solo ventas no vinculadas (no existen internamente)
    /// </summary>
    public bool? IsLinked { get; set; }

    /// <summary>
    /// Filtro por rango de montos.
    /// Retorna ventas cuyo Total esté entre MinAmount y MaxAmount.
    /// </summary>
    public decimal? MinAmount { get; set; }

    /// <summary>
    /// Filtro por rango de montos (máximo).
    /// Retorna ventas cuyo Total esté entre MinAmount y MaxAmount.
    /// </summary>
    public decimal? MaxAmount { get; set; }

    /// <summary>
    /// Filtro por nivel de satisfacción (solo para ventas vinculadas).
    /// Valores: 0-5
    /// </summary>
    public int? SatisfactionLevel { get; set; }

    /// <summary>
    /// Número máximo de resultados a retornar.
    /// Si es null o 0, retorna todos los resultados.
    /// </summary>
    public int? Limit { get; set; } = 100;

    /// <summary>
    /// Número de resultados a saltar (para paginación).
    /// Por defecto es 0 (primeros resultados).
    /// </summary>
    public int? Offset { get; set; } = 0;
}
