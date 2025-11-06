using Microsoft.AspNetCore.Mvc;
using AVASphere.Infrastructure.System.Services;
using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.ApplicationCore.Common.Entities.General;
using AVASphere.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
    
    [HttpDelete("drop-tables")]
    public async Task<IActionResult> DropTables()
    {
        var result = await _dbTools.DropTablesAsync();
        return Ok(new { Result = result });
    }

    
}
