using Microsoft.AspNetCore.Mvc;
using AVASphere.Infrastructure.System.Services;
namespace AVASphere.WebApi.System.Controllers;


[ApiController]
[Route("api/system/[controller]")]
[ApiExplorerSettings(GroupName = "System")]
[Tags("System - Database Tools")]
public class DbToolsController : ControllerBase
{
    private readonly DbToolsServices _dbTools;

    public DbToolsController(DbToolsServices dbTools)
    {
        _dbTools = dbTools;
    }

    [HttpGet("check")]
    public async Task<IActionResult> CheckConnection()
    {
        var (isConnected, hasData, message) = await _dbTools.CheckConnectionAsync();
        return Ok(new
        {
            IsConnected = isConnected,
            HasData = hasData,
            Message = message
        });
    }

    [HttpPost("create-migration")]
    public async Task<IActionResult> CreateMigration([FromQuery] string name)
    {
        var result = await _dbTools.CreateMigrationAsync(name);
        return Ok(new { Result = result });
    }

    [HttpPost("apply-migration")]
    public async Task<IActionResult> ApplyMigration()
    {
        var result = await _dbTools.ApplyMigrationAsync();
        return Ok(new { Result = result });
    }

    [HttpDelete("drop")]
    public async Task<IActionResult> DropDatabase()
    {
        var result = await _dbTools.DropDatabaseAsync();
        return Ok(new { Result = result });
    }
}
