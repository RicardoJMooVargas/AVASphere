using Microsoft.AspNetCore.Mvc;
using AVASphere.ApplicationCore.Common.DTOs;
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
        var response = new ApiResponse
        {
            Success = true,
            Data = new
            {
                IsConnected = isConnected,
                HasData = hasData
            },
            Message = message,
            StatusCode = 200
        };
        return Ok(response);
    }
    
    [HttpDelete("drop-tables")]
    public async Task<IActionResult> DropTables()
    {
        var result = await _dbTools.DropTablesAsync();
        var response = new ApiResponse
        {
            Success = true,
            Data = new { Result = result },
            Message = "Operación de eliminación de tablas completada",
            StatusCode = 200
        };
        return Ok(response);
    }
}
