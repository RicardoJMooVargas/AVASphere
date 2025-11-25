using Microsoft.AspNetCore.Mvc;
using AVASphere.ApplicationCore.Sales.Interfaces;
using AVASphere.ApplicationCore.Sales.Entities;
using AVASphere.ApplicationCore.Sales.DTOs;
using AVASphere.ApplicationCore.Common.Entities.Jsons;
using System.Text.Json;
using SaleEntity = AVASphere.ApplicationCore.Sales.Entities.Sale;


namespace AVASphere.WebApi.Sale.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "Sales")]
    [Tags("Sales")]
    public class SaleManagerController : ControllerBase
    {
        private readonly ISaleService _saleService;
        private readonly HttpClient _httpClient;
        public SaleManagerController(ISaleService saleService, HttpClient httpClient)
        {
            _saleService = saleService;
            _httpClient = httpClient;

        }

        [HttpPost("CreateSale")]
        public async Task<IActionResult> Create(SaleExternalDto saleDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // ⚙️ Datos que vendrán desde Swagger (o puedes poner valores por defecto)
            int customerId = int.TryParse(saleDto.CodeClient, out var cId) ? cId : 0;
            string createdByUserId = User?.Identity?.Name ?? "system";
            string salesExecutive = saleDto.Agente ?? "No asignado";

            // ✅ Llamada correcta al servicio con todos los parámetros requeridos
            var created = await _saleService.CreateSaleAsync(saleDto, createdByUserId, customerId, salesExecutive);

            return CreatedAtAction(nameof(GetById), new { id = created.IdSale }, created);
        }

        // GET: api/Sale/{id}
        [HttpGet("GetByIdSale")]
        public async Task<ActionResult> GetById(int IdSale)
        {
            var sale = await _saleService.GetSaleByIdAsync(IdSale);
            if (sale == null) return NotFound();
            return Ok(sale);
        }

        // GET: api/Sale/folio/{folio}
        [HttpGet("GetByfolio")]
        public async Task<ActionResult> GetByFolio(string folio)
        {
            var sale = await _saleService.GetSaleByFolioAsync(folio);
            if (sale == null) return NotFound();
            return Ok(sale);
        }

        // DELETE: api/Sale/{id}
        [HttpDelete("DeleteIdSale")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _saleService.DeleteSaleAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        // POST: api/Sale/from-quotations
        [HttpPost("CreateFromQuotations")]
        public async Task<IActionResult> CreateFromQuotations(CreateSaleFromQuotationsDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var sale = new global::AVASphere.ApplicationCore.Sales.Entities.Sale
            {
                SalesExecutive = dto.SalesExecutive,
                SaleDate = dto.Date,
                Type = dto.Type,
                IdCustomer = dto.CustomerId,
                Folio = dto.Folio,
                TotalAmount = dto.TotalAmount,
                DeliveryDriver = dto.DeliveryDriver,
                HomeDelivery = dto.HomeDelivery,
                DeliveryDate = dto.DeliveryDate,
                SatisfactionLevel = dto.SatisfactionLevel,
                SatisfactionReason = dto.SatisfactionReason,
                Comment = dto.Comment,
                AfterSalesFollowupDate = dto.AfterSalesFollowupDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IdConfigSys = dto.IdConfigSys
            };

            var created = await _saleService.CreateSaleFromQuotationsAsync(dto.QuotationIds, sale, User?.Identity?.Name ?? "system");
            return CreatedAtAction(nameof(GetById), new { id = created.IdSale }, created);
        }
        // Obtiene las ventas por fecha desde la API externa (APIVAA) y verifica si existen en la base de datos.

        [HttpGet("GetSalesExternal")]
        public async Task<IActionResult> ObtenerVentasPorFecha(
            [FromQuery] string fecha,
            [FromQuery] string? folio,
            [FromQuery] string? nombreCliente,
            [FromQuery] string? cliente)
        {
            if (string.IsNullOrEmpty(fecha))
                return BadRequest("El parámetro 'fecha' es requerido.");

            const string catalogo = "AVA01";
            var url = $"http://apivaa.ddns.net:8080/api/rest/tsm/VENTASPORFECHAV?CATALOGO={catalogo}&FECHA={fecha}";

            try
            {
                // Llamada HTTP a la API externa
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, $"Error al obtener datos: {response.ReasonPhrase}");

                var content = await response.Content.ReadAsStringAsync();

                // Deserializar JSON en lista de SaleJson
                List<SaleJson>? ventas;
                try
                {
                    var result = JsonSerializer.Deserialize<VentasResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    ventas = result?.Ventas;
                }
                catch (JsonException ex)
                {
                    return StatusCode(500, $"Error al deserializar la respuesta: {ex.Message}");
                }


                if (ventas == null || ventas.Count == 0)
                    return NotFound("No se encontraron ventas para la fecha especificada.");

                // Filtros opcionales
                var ventasFiltradas = ventas.Where(v =>
                    (string.IsNullOrEmpty(folio) || v.Folio.Equals(folio, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrEmpty(nombreCliente) || v.NombreCliente.Contains(nombreCliente, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrEmpty(cliente) || v.Cliente.Equals(cliente, StringComparison.OrdinalIgnoreCase))
                ).ToList();

                return Ok(ventasFiltradas);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error en la solicitud HTTP: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado: {ex.Message}");
            }
        }

        [HttpGet("External-Detail")]
        public async Task<IActionResult> ObtenerVentaDetalleExterno(
            [FromQuery] string NF,
            [FromQuery] string caja,
            [FromQuery] string serie,
            [FromQuery] string folio)
        {
            if (string.IsNullOrEmpty(NF) || string.IsNullOrEmpty(caja) ||
                string.IsNullOrEmpty(serie) || string.IsNullOrEmpty(folio))
                return BadRequest("Los parámetros 'NF', 'caja', 'serie' y 'folio' son requeridos.");

            // ✅ Valor por defecto
            string catalogo = "AVA01";

            // ✅ Construcción de la URL con los parámetros ingresados
            string url = $"http://apivaa.ddns.net:8080/api/rest/tsm/DetalleVentaV?CATALOGO={catalogo}&NF={NF}&CAJA={caja}&SERIE={serie}&FOLIO={folio}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, $"Error al consultar la venta externa: {response.ReasonPhrase}");

                var content = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(content))
                    return BadRequest("La respuesta del servicio externo está vacía.");

                using var document = JsonDocument.Parse(content);
                var root = document.RootElement;

                // 🔍 Buscar el arreglo de productos
                JsonElement movimientos;
                if (root.TryGetProperty("Registros", out var movProp))
                {
                    movimientos = movProp;
                }
                else if (root.ValueKind == JsonValueKind.Array)
                {
                    movimientos = root;
                }
                else
                {
                    return BadRequest(new
                    {
                        Mensaje = "La respuesta no contiene un arreglo de productos válido.",
                        Ejemplo = content
                    });
                }

                // ✅ Mapear los productos al formato solicitado
                var productos = new List<object>();

                foreach (var item in movimientos.EnumerateArray())
                {
                    productos.Add(new
                    {
                        Mov = item.TryGetProperty("Mov", out var mov) ? mov.GetInt32() : 0,
                        Codigo = item.TryGetProperty("Codigo", out var cod) ? cod.GetString() ?? string.Empty : string.Empty,
                        Descripcion = item.TryGetProperty("Descripcion", out var desc) ? desc.GetString() ?? string.Empty : string.Empty,
                        Unidad = item.TryGetProperty("Unidad", out var uni) ? uni.GetString() ?? string.Empty : string.Empty,
                        Cantidad = item.TryGetProperty("Cantidad", out var cant) ? cant.GetDouble() : 0,
                        Tprc = item.TryGetProperty("Tprc", out var tprc) ? tprc.GetInt32() : 0,
                        Precio = item.TryGetProperty("Precio", out var prec) ? prec.GetDouble() : 0,
                        Dcto = item.TryGetProperty("Dcto", out var dcto) ? dcto.GetDouble() : 0,
                        Impto = item.TryGetProperty("Impto", out var impto) ? impto.GetDouble() : 0,
                        Importe = item.TryGetProperty("Importe", out var imp) ? imp.GetDouble() : 0,
                        Descuento = item.TryGetProperty("Descuento", out var des) ? des.GetDouble() : 0,
                        Impuesto = item.TryGetProperty("Impuesto", out var impu) ? impu.GetDouble() : 0,
                        Total = item.TryGetProperty("Total", out var tot) ? tot.GetDouble() : 0
                    });
                }

                return Ok(productos);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error en la solicitud HTTP: {ex.Message}");
            }
            catch (JsonException ex)
            {
                return StatusCode(500, $"Error al analizar el JSON: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado: {ex.Message}");
            }
        }



    }
}