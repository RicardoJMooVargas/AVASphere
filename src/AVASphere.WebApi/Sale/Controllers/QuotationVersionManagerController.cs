using Microsoft.AspNetCore.Mvc;
using AVASphere.ApplicationCore.Sales.Interfaces;
using AVASphere.ApplicationCore.Sales.Entities;
using AVASphere.ApplicationCore.Sales.DTOs;

namespace AVASphere.WebApi.QuotationVersion.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "Sales")]
[Tags("Quotations")]

// Controlador para gestionar las versiones de una cotización.
// Provee endpoints para obtener una versión por id, listar versiones por cotización
// y recuperar la versión más reciente de una cotización.

public class QuotationVersionManagerController : ControllerBase
{
    private readonly IQuotationVersionService _quotationVersionService;

    public QuotationVersionManagerController(IQuotationVersionService quotationVersionService)
    {
        _quotationVersionService = quotationVersionService;
    }

    //  GET api/quotationversion/{id}
    //  Obtiene una versión de cotización por su identificador Devuelve 200 con la versión cuando existe, 404 si no se encuentra.
    [HttpGet("GetQuotationVersionById")]
    public async Task<IActionResult> GetById(int IdQuotationVersion)
    {
        var v = await _quotationVersionService.GetVersionAsync(IdQuotationVersion);
        if (v == null) return NotFound();
        return Ok(v);
    }

    // GET api/quotationversion/by-quotation/{quotationId}
    // Lista todas las versiones asociadas a una cotización.
    // Devuelve 200 con la lista (puede estar vacía si no hay versiones).
    [HttpGet("GetAllQuotationVersions")]
    public async Task<IActionResult> GetByQuotationId(int IdQuotation)
    {
        var list = await _quotationVersionService.ListVersionsAsync(IdQuotation);
        return Ok(list);
    }

    // GET api/quotationversion/latest/{quotationId}
    // Recupera la versión más reciente de una cotización.
    // Devuelve 200 con la versión más reciente o 404 si no existe ninguna.

    [HttpGet("GetLatestQuotationVersion")]
    public async Task<IActionResult> GetLatest(int IdQuotation)
    {
        var latest = await _quotationVersionService.GetLatestVersionAsync(IdQuotation);
        if (latest == null) return NotFound();
        return Ok(latest);
    }


}