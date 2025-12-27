using Microsoft.AspNetCore.Mvc;
using AVASphere.ApplicationCore.Sales.Interfaces;
using AVASphere.ApplicationCore.Sales.Entities;
using AVASphere.ApplicationCore.Sales.DTOs;
using AVASphere.ApplicationCore.Sales.DTOs.SaleDTOs;
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
        private readonly IExternalSalesService _externalSalesService;
        private readonly HttpClient _httpClient;

        public SaleManagerController(
            ISaleService saleService,
            IExternalSalesService externalSalesService,
            HttpClient httpClient)
        {
            _saleService = saleService;
            _externalSalesService = externalSalesService ?? throw new ArgumentNullException(nameof(externalSalesService));
            _httpClient = httpClient;
        }

        /* <summary>
        /// GET: api/SaleManager/GetSalesExternal
        /// 
        /// PROPÓSITO: Endpoint principal que consulta ventas del sistema externo InforAVA,
        /// las combina con información interna y permite búsquedas/filtrados avanzados.
        /// 
        /// MOTIVO:
        /// 1. INTEGRACIÓN CON SISTEMAS EXTERNOS: Ver qué ventas están en InforAVA
        /// 2. IDENTIFICAR BRECHAS: Mostrar ventas no importadas internamente
        /// 3. BÚSQUEDA INTELIGENTE: Search automático (números → folio; texto → cliente)
        /// 4. FILTRADOS COMPLEJOS: Por monto, satisfacción, estado de vinculación, etc.
        /// </summary>
        /// <returns>Lista filtrada y paginada de ventas combinadas.</returns>*/
        [HttpGet("GetSalesExternal")]
        public async Task<IActionResult> GetSalesExternal(
            [FromQuery] DateTime? fecha = null,
            [FromQuery] string? search = null,
            [FromQuery] string? customerName = null,
            [FromQuery] string? folio = null,
            [FromQuery] bool? isLinked = null,
            [FromQuery] decimal? minAmount = null,
            [FromQuery] decimal? maxAmount = null,
            [FromQuery] int? satisfactionLevel = null,
            [FromQuery] int? limit = 100,
            [FromQuery] int? offset = 0)
        {
            try
            {
                // Catálogo fijo automático
                const string catalogo = "AVA01";

                // Construir filtro
                var filter = new SaleFilterDto
                {
                    Catalogo = catalogo,
                    Fecha = fecha,
                    Search = search,
                    CustomerName = customerName,
                    Folio = folio,
                    IsLinked = isLinked,
                    MinAmount = minAmount,
                    MaxAmount = maxAmount,
                    SatisfactionLevel = satisfactionLevel,
                    Limit = limit,
                    Offset = offset
                };

                // Llamar al servicio con filtros
                var combinedSales = await _externalSalesService.GetExternalSalesWithInternalDataAsync(filter);
                var salesList = combinedSales.ToList();

                // Retornar respuesta estructurada
                return Ok(new
                {
                    success = true,
                    catalogo = filter.Catalogo,
                    fecha = (filter.Fecha ?? DateTime.UtcNow).ToString("yyyy-MM-dd"),
                    filtrosAplicados = new
                    {
                        search = filter.Search,
                        customerName = filter.CustomerName,
                        folio = filter.Folio,
                        isLinked = filter.IsLinked,
                        montoMin = filter.MinAmount,
                        montoMax = filter.MaxAmount,
                        satisfactionLevel = filter.SatisfactionLevel
                    },
                    paginacion = new
                    {
                        limit = filter.Limit ?? 100,
                        offset = filter.Offset ?? 0,
                        totalResultados = salesList.Count
                    },
                    resumen = new
                    {
                        totalVentas = salesList.Count,
                        ventasVinculadas = salesList.Count(c => c.IsLinked),
                        ventasNoVinculadas = salesList.Count(c => !c.IsLinked)
                    },
                    datos = salesList
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(502, new
                {
                    success = false,
                    error = "El sistema externo InforAVA no está disponible.",
                    detalles = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = "Error al procesar la solicitud.",
                    detalles = ex.Message
                });
            }
        }


        // GET: api/SaleManager/VerifyExternalConnection
        [HttpGet("VerifyExternalConnection")]
        public async Task<IActionResult> VerifyExternalConnection()
        {
            try
            {
                var isAvailable = await _externalSalesService.VerifyExternalConnectionAsync();

                return Ok(new
                {
                    success = true,
                    available = isAvailable,
                    mensaje = isAvailable
                        ? "Sistema externo InforAVA disponible."
                        : "Sistema externo InforAVA no disponible."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(502, new
                {
                    success = false,
                    available = false,
                    mensaje = "No se pudo verificar la conectividad.",
                    detalles = ex.Message
                });
            }
        }

        [HttpPost("CreateSale")]
        public async Task<IActionResult> Create(SaleExternalDto saleDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int customerId = int.TryParse(saleDto.CodeClient, out var cId) ? cId : 0;
            string createdByUserId = User?.Identity?.Name ?? "system";
            string salesExecutive = saleDto.Agente ?? "No asignado";

            var created = await _saleService.CreateSaleAsync(saleDto, createdByUserId, customerId, salesExecutive);

            return CreatedAtAction(nameof(GetById), new { id = created.IdSale }, created);
        }

        // GET: api/Sale/{id}
        private async Task<ActionResult> GetById(int IdSale)
        {
            var sale = await _saleService.GetSaleByIdAsync(IdSale);
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

        [HttpGet("ObtenerVentasPorFecha")]
        public async Task<IActionResult> ObtenerVentasPorFecha([FromQuery] string fecha, [FromQuery] string? folio, [FromQuery] string? nombreCliente, [FromQuery] string? cliente)
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
        public async Task<IActionResult> ObtenerVentaDetalleExterno([FromQuery] string nf, [FromQuery] string caja, [FromQuery] string serie, [FromQuery] string folio)
        {
            try
            {
                // Validar parámetros requeridos
                if (string.IsNullOrWhiteSpace(nf) || string.IsNullOrWhiteSpace(caja) ||
                    string.IsNullOrWhiteSpace(serie) || string.IsNullOrWhiteSpace(folio))
                    return BadRequest(new
                    {
                        success = false,
                        error = "Los parámetros 'NF', 'Caja', 'Serie' y 'Folio' son obligatorios."
                    });

                // Obtener detalles a través del repositorio centralizado
                var detalles = await _externalSalesService.GetSaleDetailAsync(nf, caja, serie, folio);
                var detallesList = detalles.ToList();

                // Retornar respuesta estructurada
                return Ok(new
                {
                    success = true,
                    identificadores = new { nf, caja, serie, folio },
                    totalProductos = detallesList.Count,
                    datos = detallesList
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    error = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(502, new
                {
                    success = false,
                    error = "El sistema externo InforAVA no está disponible.",
                    detalles = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = "Error al obtener los detalles de la venta.",
                    detalles = ex.Message
                });
            }
        }

    }
}