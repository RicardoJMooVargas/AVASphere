using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.Infrastructure.System.Services;
using Microsoft.AspNetCore.Mvc;

namespace AVASphere.WebApi.System.Controllers;

[ApiController]
[Route("api/system/[controller]")]
[ApiExplorerSettings(GroupName = "System")]
[Tags("System - Up Tools")]
public class ToolsController : ControllerBase
{
    private readonly DatabaseMigrationService _dbMigrationService;

    public ToolsController(DatabaseMigrationService dbMigrationService)
    {
        _dbMigrationService = dbMigrationService;
    }

    [HttpPost("apply-migrations")]
    public async Task<IActionResult> ApplyMigrations()
    {
        var result = await _dbMigrationService.ApplyMigrationsAsync();
        var response = new ApiResponse
        {
            Success = true,
            Data = new { message = result },
            Message = "Migraciones aplicadas",
            StatusCode = 200
        };
        return Ok(response);
    }

    [HttpPost("apply-migrations-force")]
    public async Task<IActionResult> ApplyMigrationsForce()
    {
        var result = await _dbMigrationService.ApplyMigrationsForceAsync();
        var response = new ApiResponse
        {
            Success = true,
            Data = new { 
                message = result,
                info = "Aplica migraciones suprimiendo PendingModelChangesWarning - Para inicialización de DB"
            },
            Message = "Migraciones forzadas aplicadas",
            StatusCode = 200
        };
        return Ok(response);
    }
}
