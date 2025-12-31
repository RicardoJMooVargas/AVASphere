using Microsoft.AspNetCore.Mvc;
using AVASphere.ApplicationCore.Sales.Interfaces;
using AVASphere.ApplicationCore.Sales.Entities;
using AVASphere.ApplicationCore.Sales.DTOs;
using AVASphere.ApplicationCore.Sales.DTOs.SaleDTOs;
using AVASphere.ApplicationCore.Sales.DTOs.ImportDTOs;
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
            [FromQuery] string? aux = null,
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
                
                if (aux == "string")
                {
                    aux = null;
                }
                
                // Construir filtro
                var filter = new SaleFilterDto
                {
                    Catalogo = catalogo,
                    Fecha = fecha,
                    Search = search,
                    CustomerName = customerName,
                    Folio = folio,
                    Auxiliar = aux,
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
        
        /// <summary>
        /// POST: api/SaleManager/ImportSalesMonth
        /// 
        /// PROPÓSITO: Importa ventas del sistema externo InforAVA para un mes completo
        /// de forma optimizada, minimizando la carga en la API externa.
        /// 
        /// OPTIMIZACIONES IMPLEMENTADAS:
        /// 1. Procesamiento por lotes de días (configurable, por defecto 5 días)
        /// 2. Cache de clientes para evitar consultas repetitivas
        /// 3. Detección de ventas duplicadas antes de inserción
        /// 4. Pausas entre lotes para no saturar API externa
        /// 5. Manejo granular de errores sin afectar el lote completo
        /// 6. Consulta de detalles de productos para cada venta
        /// 
        /// PROCESO:
        /// 1. Valida parámetros (año/mes válidos, no futuro, máximo 1 mes)
        /// 2. Divide el mes en lotes de días para procesamiento controlado
        /// 3. Para cada día del lote, consulta VENTASPORFECHAV
        /// 4. Filtra ventas ya existentes en BD local
        /// 5. Crea/encuentra clientes necesarios en cache
        /// 6. Para cada venta, consulta DetalleVentaV para obtener productos
        /// 7. Inserta ventas con toda la información enriquecida
        /// 8. Retorna estadísticas detalladas del proceso
        /// 
        /// LIMITACIONES:
        /// - Máximo 1 mes por importación
        /// - Timeout de 10 minutos por importación
        /// - Máximo 1000 ventas procesadas por seguridad
        /// </summary>
        /// <param name="year">Año a importar (2020-presente)</param>
        /// <param name="month">Mes a importar (1-12)</param>
        /// <param name="idConfigSys">ID del sistema de configuración (requerido)</param>
        /// <param name="batchSize">Tamaño del lote en días (opcional, por defecto 5)</param>
        /// <returns>Estadísticas detalladas de la importación</returns>
        [HttpPost("ImportSalesMonth")]
        public async Task<IActionResult> ImportSalesMonth(
            [FromQuery] int year,
            [FromQuery] int month,
            [FromQuery] int idConfigSys,
            [FromQuery] int batchSize = 5)
        {
            try
            {
                // 🔍 VALIDACIONES INICIALES
                if (year < 2020 || year > DateTime.UtcNow.Year)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = $"El año {year} no es válido. Debe estar entre 2020 y {DateTime.UtcNow.Year}.",
                        Error = "INVALID_YEAR"
                    });
                }

                if (month < 1 || month > 12)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = $"El mes {month} no es válido. Debe estar entre 1 y 12.",
                        Error = "INVALID_MONTH"
                    });
                }

                if (idConfigSys <= 0)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "El idConfigSys es requerido y debe ser mayor a 0.",
                        Error = "INVALID_CONFIG_SYS"
                    });
                }

                var importDate = new DateTime(year, month, 1);
                if (importDate > DateTime.UtcNow.Date)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "No se puede importar datos de fechas futuras.",
                        Error = "FUTURE_DATE"
                    });
                }

                if (batchSize < 1 || batchSize > 15)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "El tamaño del lote debe estar entre 1 y 15 días.",
                        Error = "INVALID_BATCH_SIZE"
                    });
                }

                // 📊 EJECUTAR IMPORTACIÓN OPTIMIZADA
                var createdByUserId = "system"; // O obtener del contexto de usuario actual

                var importResult = await _saleService.ImportSalesForMonthAsync(
                    year, 
                    month, 
                    idConfigSys, 
                    createdByUserId, 
                    batchSize);

                // 📈 PREPARAR RESPUESTA CON ESTADÍSTICAS DETALLADAS
                var response = new
                {
                    Success = importResult.IsSuccessful,
                    Message = importResult.Message,
                    ImportPeriod = new
                    {
                        Year = year,
                        Month = month,
                        StartDate = importResult.StartDate.ToString("yyyy-MM-dd"),
                        EndDate = importResult.EndDate.ToString("yyyy-MM-dd"),
                        TotalDays = (importResult.EndDate - importResult.StartDate).Days + 1
                    },
                    Statistics = new
                    {
                        TotalSalesFound = importResult.TotalSalesFound,
                        TotalSalesImported = importResult.TotalSalesImported,
                        TotalSalesSkipped = importResult.TotalSalesSkipped,
                        TotalSalesError = importResult.TotalSalesError,
                        SuccessRate = importResult.TotalSalesFound > 0 
                            ? Math.Round((double)importResult.TotalSalesImported / importResult.TotalSalesFound * 100, 2) 
                            : 0
                    },
                    Customers = new
                    {
                        CustomersFound = importResult.CustomersFound,
                        CustomersCreated = importResult.CustomersCreated,
                        CustomersReused = importResult.CustomersReused
                    },
                    Performance = new
                    {
                        TotalProcessingTime = importResult.TotalProcessingTime.ToString(@"hh\:mm\:ss"),
                        BatchesProcessed = importResult.BatchesProcessed,
                        AverageTimePerBatch = $"{importResult.AverageTimePerBatch:F2} seconds",
                        SalesPerMinute = importResult.TotalProcessingTime.TotalMinutes > 0 
                            ? Math.Round(importResult.TotalSalesImported / importResult.TotalProcessingTime.TotalMinutes, 2) 
                            : 0
                    },
                    BatchDetails = importResult.BatchSummaries.Select(b => new
                    {
                        BatchNumber = b.BatchNumber,
                        Period = $"{b.BatchStartDate:yyyy-MM-dd} to {b.BatchEndDate:yyyy-MM-dd}",
                        SalesProcessed = b.SalesProcessed,
                        SalesImported = b.SalesImported,
                        SalesSkipped = b.SalesSkipped,
                        SalesError = b.SalesError,
                        ProcessingTime = b.ProcessingTime.ToString(@"mm\:ss"),
                        IsSuccessful = b.IsSuccessful,
                        Message = b.Message
                    }).ToList(),
                    Errors = importResult.Errors.Select(e => new
                    {
                        SaleFolio = e.SaleFolio,
                        CustomerName = e.CustomerName,
                        SaleDate = e.SaleDate?.ToString("yyyy-MM-dd"),
                        ErrorType = e.ErrorType,
                        ErrorMessage = e.ErrorMessage,
                        Timestamp = e.ErrorTimestamp.ToString("yyyy-MM-dd HH:mm:ss")
                    }).ToList(),
                    SkippedSales = importResult.SkippedSales
                };

                // 🎯 DETERMINAR CÓDIGO DE RESPUESTA HTTP
                if (importResult.IsSuccessful)
                {
                    return Ok(response);
                }
                else if (importResult.TotalSalesImported > 0)
                {
                    // Importación parcial exitosa
                    return StatusCode(207, response); // Multi-Status
                }
                else
                {
                    // Importación falló completamente
                    return StatusCode(422, response); // Unprocessable Entity
                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message,
                    Error = "VALIDATION_ERROR"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error interno del servidor durante la importación.",
                    Error = "INTERNAL_SERVER_ERROR",
                    Details = ex.Message
                });
            }
        }
    }
}

