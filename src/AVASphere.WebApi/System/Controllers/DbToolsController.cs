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
    private readonly IConfigSysService _configSysService;
    private readonly MasterDbContext _dbContext;

    public DbToolsController(DbToolsServices dbTools, IConfigSysService configSysService, MasterDbContext dbContext)
    {
        _dbTools = dbTools;
        _configSysService = configSysService;
        _dbContext = dbContext;
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

    [HttpGet("check-initial-config")]
    public async Task<IActionResult> CheckInitialConfig()
    {
        try
        {
            // Usar DbContext directamente para evitar problemas de conexión cerrada
            ConfigSys? config = null;
            bool tableExists = false;
            bool hasConfiguration = false;
            
            try
            {
                // Verificar si podemos conectar a la base de datos
                var canConnect = await _dbContext.Database.CanConnectAsync();
                if (!canConnect)
                {
                    return Ok(new
                    {
                        HasConfiguration = false,
                        TableExists = false,
                        RequiresMigration = true,
                        Message = "No se puede conectar a la base de datos",
                        Data = (object?)null
                    });
                }

                // Intentar consultar directamente usando Entity Framework
                config = await _dbContext.ConfigSys.FirstOrDefaultAsync();
                tableExists = true; // Si no hay excepción, la tabla existe
                hasConfiguration = config != null;
            }
            catch (Exception dbEx)
            {
                // Verificar si el error es específicamente porque la tabla no existe
                var errorMessage = dbEx.Message.ToLower();
                var innerMessage = dbEx.InnerException?.Message?.ToLower() ?? "";
                
                if (errorMessage.Contains("does not exist") || 
                    errorMessage.Contains("relation") ||
                    errorMessage.Contains("table") ||
                    innerMessage.Contains("does not exist") ||
                    innerMessage.Contains("relation") ||
                    (dbEx is PostgresException pgEx && pgEx.SqlState == "42P01")) // PostgreSQL: undefined_table
                {
                    tableExists = false;
                    hasConfiguration = false;
                }
                else
                {
                    // Re-lanzar otros errores
                    throw;
                }
            }

            // Determinar respuesta basada en los resultados
            if (!tableExists)
            {
                return Ok(new
                {
                    HasConfiguration = false,
                    TableExists = false,
                    RequiresMigration = true,
                    Message = "La tabla ConfigSys no existe. Se requiere ejecutar migraciones.",
                    Data = (object?)null
                });
            }

            if (!hasConfiguration)
            {
                return Ok(new
                {
                    HasConfiguration = false,
                    TableExists = true,
                    RequiresMigration = false,
                    Message = "La tabla ConfigSys existe pero no hay configuración inicial del sistema",
                    Data = (object?)null
                });
            }

            return Ok(new
            {
                HasConfiguration = true,
                TableExists = true,
                RequiresMigration = false,
                Message = "Configuración inicial encontrada y tabla ConfigSys existe",
                Data = new
                {
                    config!.IdConfigSys,
                    config.CompanyName,
                    config.BranchName,
                    config.LogoUrl,
                    ColorsCount = config.Colors?.Count ?? 0,
                    NotUseModulesCount = config.NotUseModules?.Count ?? 0,
                    config.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                HasConfiguration = false,
                TableExists = false,
                RequiresMigration = true,
                Message = $"Error al verificar la configuración inicial: {ex.Message}",
                Data = (object?)null
            });
        }
    }
}
