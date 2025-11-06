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
        return Ok(new { message = result });
    }

    [HttpPost("apply-migrations-force")]
    public async Task<IActionResult> ApplyMigrationsForce()
    {
        var result = await _dbMigrationService.ApplyMigrationsForceAsync();
        return Ok(new { 
            message = result,
            info = "Aplica migraciones suprimiendo PendingModelChangesWarning - Para inicialización de DB"
        });
    }
}
