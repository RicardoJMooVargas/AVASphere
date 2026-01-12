using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AVASphere.ApplicationCore.Sales.DTOs;
using AVASphere.ApplicationCore.Sales.Interfaces;

namespace AVASphere.Infrastructure.Sales.Repositories;

/// <summary>
/// Implementación del repositorio para consultas al sistema externo InforAVA.
/// 
/// MOTIVO DE ESTA CLASE:
/// Centralizar toda la comunicación HTTP con el servicio externo en un único lugar.
/// Esto permite:
/// - Reutilizar la lógica de conexión
/// - Manejar reintentos y timeouts de forma consistente
/// - Cambiar fácilmente el endpoint base sin afectar la aplicación
/// </summary>
public class ExternalSalesRepository : IExternalSalesRepository
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// URL base del servicio externo InforAVA.
    /// </summary>
    private readonly string _externalApiBaseUrl = "http://apivaa.ddns.net:8080/api/rest/tsm";

    public ExternalSalesRepository(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Consulta ventas del sistema externo InforAVA para una fecha y catálogo específicos.
    /// 
    /// PROCESO:
    /// 1. Validar parámetros de entrada
    /// 2. Construir URL del endpoint externo
    /// 3. Realizar solicitud HTTP GET
    /// 4. Parsear respuesta JSON
    /// 5. Mapear datos externos a nuestro DTO
    /// 6. Manejar errores de conectividad y parsing
    /// </summary>
    public async Task<IEnumerable<ExternalSalesDto>> GetSalesByDateAndCatalogAsync(string catalogo, DateTime fecha)
    {
        try
        {
            // Validar entrada
            if (string.IsNullOrWhiteSpace(catalogo))
                throw new ArgumentException("Catálogo no puede estar vacío.", nameof(catalogo));

            // Construir URL: VENTASPORFECHAV?CATALOGO={catalogo}&FECHA={fecha}
            // Formato de fecha: YYYY-MM-DD
            string fechaFormato = fecha.ToString("yyyy-MM-dd");
            string url = $"{_externalApiBaseUrl}/VENTASPORFECHAV?CATALOGO={catalogo}&FECHA={fechaFormato}";

            // Realizar petición HTTP GET
            using (var response = await _httpClient.GetAsync(url))
            {
                // Manejar respuestas no exitosas
                if (!response.IsSuccessStatusCode)
                {
                    return Array.Empty<ExternalSalesDto>();
                }

                // Leer contenido como string
                var content = await response.Content.ReadAsStringAsync();

                // Parsear JSON
                using (var jsonDoc = JsonDocument.Parse(content))
                {
                    var root = jsonDoc.RootElement;
                    var salesList = new List<ExternalSalesDto>();

                    // Navegar la estructura JSON y mapear a nuestro DTO
                    // Estructura esperada: { "Ventas": [...] }
                    if (root.TryGetProperty("Ventas", out var ventasElement))
                    {
                        foreach (var item in ventasElement.EnumerateArray())
                        {
                            var sale = MapJsonToDto(item);
                            if (sale != null)
                                salesList.Add(sale);
                        }
                    }

                    return salesList;
                }
            }
        }
        catch (HttpRequestException ex)
        {
            // Error de conectividad con el servicio externo
            throw new InvalidOperationException(
                "No se pudo conectar con el sistema externo InforAVA. Verifique la conectividad.", ex);
        }
        catch (JsonException ex)
        {
            // Error al parsear JSON
            throw new InvalidOperationException(
                "El sistema externo InforAVA retornó datos inválidos.", ex);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Obtiene una venta específica del sistema externo por su folio/ID.
    /// Implementación pendiente según endpoint que proporcione la API externa.
    /// </summary>
    public async Task<ExternalSalesDto?> GetSaleByFolioAsync(string folioExterno)
    {
        await Task.CompletedTask;
        return null;
    }

    /// <summary>
    /// Verifica si el servicio externo está disponible (health check).
    /// </summary>
    public async Task<bool> IsExternalServiceAvailableAsync()
    {
        try
        {
            using (var response = await _httpClient.SendAsync(
                new HttpRequestMessage(HttpMethod.Head, _externalApiBaseUrl)))
            {
                return response.IsSuccessStatusCode;
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Obtiene los detalles de productos/movimientos de una venta específica en el sistema externo.
    /// 
    /// PROCESO:
    /// 1. Validar parámetros de entrada (NF, Caja, Serie, Folio)
    /// 2. Construir URL del endpoint externo (DetalleVentaV)
    /// 3. Realizar solicitud HTTP GET
    /// 4. Parsear respuesta JSON (arreglo de productos)
    /// 5. Mapear datos a ExternalSaleDetailDto
    /// 6. Manejar errores de conectividad y parsing
    /// </summary>
    public async Task<IEnumerable<ExternalSaleDetailDto>> GetSaleDetailAsync(string nf, string caja, string serie, string folio)
    {
        try
        {
            // Validar entrada
            if (string.IsNullOrWhiteSpace(nf) || string.IsNullOrWhiteSpace(caja) ||
                string.IsNullOrWhiteSpace(serie) || string.IsNullOrWhiteSpace(folio))
                throw new ArgumentException("Los parámetros NF, Caja, Serie y Folio no pueden estar vacíos.");

            // Valor por defecto para catálogo (puede ser parametrizado en el futuro)
            const string catalogo = "AVA01";

            // Construir URL: DetalleVentaV?CATALOGO={catalogo}&NF={nf}&CAJA={caja}&SERIE={serie}&FOLIO={folio}
            string url = $"{_externalApiBaseUrl}/DetalleVentaV?CATALOGO={catalogo}&NF={nf}&CAJA={caja}&SERIE={serie}&FOLIO={folio}";

            // Realizar petición HTTP GET
            using (var response = await _httpClient.GetAsync(url))
            {
                // Manejar respuestas no exitosas
                if (!response.IsSuccessStatusCode)
                {
                    return Array.Empty<ExternalSaleDetailDto>();
                }

                // Leer contenido como string
                var content = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(content))
                {
                    return Array.Empty<ExternalSaleDetailDto>();
                }

                // Parsear JSON
                using (var jsonDoc = JsonDocument.Parse(content))
                {
                    var root = jsonDoc.RootElement;
                    var detailsList = new List<ExternalSaleDetailDto>();

                    // Navegar la estructura JSON
                    // Puede ser un arreglo directo o dentro de una propiedad "Registros"
                    JsonElement itemsArray = root;

                    if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("Registros", out var registros))
                    {
                        itemsArray = registros;
                    }

                    // Enumerar productos y mapear a DTO
                    if (itemsArray.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in itemsArray.EnumerateArray())
                        {
                            var detail = MapJsonToDetailDto(item);
                            if (detail != null)
                                detailsList.Add(detail);
                        }
                    }

                    return detailsList;
                }
            }
        }
        catch (HttpRequestException ex)
        {
            // Error de conectividad con el servicio externo
            throw new InvalidOperationException(
                "No se pudo conectar con el sistema externo InforAVA. Verifique la conectividad.", ex);
        }
        catch (JsonException ex)
        {
            // Error al parsear JSON
            throw new InvalidOperationException(
                "El sistema externo InforAVA retornó datos inválidos.", ex);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Mapea un elemento JSON del sistema externo a nuestro DTO ExternalSalesDto.
    /// Utiliza los campos reales devueltos por la API de InforAVA.
    /// </summary>
    private ExternalSalesDto? MapJsonToDto(JsonElement item)
    {
        try
        {
            var sale = new ExternalSalesDto
            {
                NF = item.TryGetProperty("NF", out var nf) ? nf.GetString() : null,
                Caja = item.TryGetProperty("Caja", out var caja) ? caja.GetString() : null,
                Serie = item.TryGetProperty("Serie", out var serie) ? serie.GetString() : null,
                Folio = item.TryGetProperty("Folio", out var folio) ? folio.GetString() : null,
                Fecha = item.TryGetProperty("Fecha", out var fecha) ? fecha.GetString() : null,
                Hora = item.TryGetProperty("Hora", out var hora) ? hora.GetString() : null,
                ZN = item.TryGetProperty("ZN", out var zn) ? zn.GetString() : null,
                Agente = item.TryGetProperty("Agente", out var agente) ? agente.GetString() : null,
                Cliente = item.TryGetProperty("Cliente", out var cliente) ? cliente.GetString() : null,
                NombreCliente = item.TryGetProperty("NombreCliente", out var nombreCliente) ? nombreCliente.GetString() : null,
                TelCliente = item.TryGetProperty("TelCliente", out var telCliente) ? telCliente.GetString() : null,
                EmailCliente = item.TryGetProperty("EmailCliente", out var emailCliente) ? emailCliente.GetString() : null,
                DireccionCliente = item.TryGetProperty("DireccionCliente", out var direccionCliente) ? direccionCliente.GetString() : null,
                PoblacionCliente = item.TryGetProperty("PoblacionCliente", out var poblacionCliente) ? poblacionCliente.GetString() : null,
                Movs = item.TryGetProperty("Movs", out var movs) ? movs.GetInt32() : 0,
                Importe = item.TryGetProperty("Importe", out var importe) && decimal.TryParse(importe.GetRawText(), out var parsedImporte) ? parsedImporte : 0,
                Descuento = item.TryGetProperty("Descuento", out var descuento) && decimal.TryParse(descuento.GetRawText(), out var parsedDescuento) ? parsedDescuento : 0,
                Impuesto = item.TryGetProperty("Impuesto", out var impuesto) && decimal.TryParse(impuesto.GetRawText(), out var parsedImpuesto) ? parsedImpuesto : 0,
                Total = item.TryGetProperty("Total", out var total) && decimal.TryParse(total.GetRawText(), out var parsedTotal) ? parsedTotal : 0
            };

            return sale;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Mapea un elemento JSON del sistema externo a nuestro DTO ExternalSaleDetailDto.
    /// Utiliza los campos reales devueltos por el endpoint DetalleVentaV.
    /// </summary>
    private ExternalSaleDetailDto? MapJsonToDetailDto(JsonElement item)
    {
        try
        {
            var detail = new ExternalSaleDetailDto
            {
                Mov = item.TryGetProperty("Mov", out var mov) ? mov.GetInt32() : 0,
                Codigo = item.TryGetProperty("Codigo", out var codigo) ? codigo.GetString() : null,
                Descripcion = item.TryGetProperty("Descripcion", out var descripcion) ? descripcion.GetString() : null,
                Unidad = item.TryGetProperty("Unidad", out var unidad) ? unidad.GetString() : null,
                Cantidad = item.TryGetProperty("Cantidad", out var cantidad) && decimal.TryParse(cantidad.GetRawText(), out var parsedCantidad) ? parsedCantidad : 0,
                Tprc = item.TryGetProperty("Tprc", out var tprc) ? tprc.GetInt32() : 0,
                Precio = item.TryGetProperty("Precio", out var precio) && decimal.TryParse(precio.GetRawText(), out var parsedPrecio) ? parsedPrecio : 0,
                Dcto = item.TryGetProperty("Dcto", out var dcto) && decimal.TryParse(dcto.GetRawText(), out var parsedDcto) ? parsedDcto : 0,
                Impto = item.TryGetProperty("Impto", out var impto) && decimal.TryParse(impto.GetRawText(), out var parsedImpto) ? parsedImpto : 0,
                Importe = item.TryGetProperty("Importe", out var importe) && decimal.TryParse(importe.GetRawText(), out var parsedImporte) ? parsedImporte : 0,
                Descuento = item.TryGetProperty("Descuento", out var descuento) && decimal.TryParse(descuento.GetRawText(), out var parsedDescuento) ? parsedDescuento : 0,
                Impuesto = item.TryGetProperty("Impuesto", out var impuesto) && decimal.TryParse(impuesto.GetRawText(), out var parsedImpuesto) ? parsedImpuesto : 0,
                Total = item.TryGetProperty("Total", out var total) && decimal.TryParse(total.GetRawText(), out var parsedTotal) ? parsedTotal : 0
            };

            return detail;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
