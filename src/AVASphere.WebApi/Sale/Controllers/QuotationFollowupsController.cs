using Microsoft.AspNetCore.Mvc;

namespace AVASphere.WebApi.Sale.Controllers;

// OBSOLETO: Este controlador ya no es necesario.
// Los endpoints de followups ahora están integrados directamente 
// en QuotationManagerController como parte de las cotizaciones.
[Obsolete("Los endpoints de followups ahora están en QuotationManagerController")]
[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "Sales")]
[Tags("Quotation Followups - OBSOLETO")]
public class QuotationFollowupsController : ControllerBase
{
    // Este controlador puede ser eliminado
    // Los endpoints han sido movidos a QuotationManagerController:
    // - POST /api/QuotationManager/{quotationId}/followups
    // - PUT /api/QuotationManager/{quotationId}/followups/{followupId}  
    // - DELETE /api/QuotationManager/{quotationId}/followups/{followupId}
}