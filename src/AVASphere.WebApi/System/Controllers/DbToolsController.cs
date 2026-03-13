﻿using Microsoft.AspNetCore.Mvc;
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

    /// <summary>
    /// Verifica la conexión y estado de la base de datos
    /// </summary>
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

    /// <summary>
    /// Verifica si existen archivos de migración
    /// </summary>
    [HttpGet("check-migrations")]
    public async Task<IActionResult> CheckMigrations()
    {
        var (hasMigrations, count, files) = await _dbTools.CheckMigrationsAsync();
        var response = new ApiResponse
        {
            Success = true,
            Data = new
            {
                HasMigrations = hasMigrations,
                MigrationCount = count,
                MigrationFiles = files
            },
            Message = hasMigrations ? $"Se encontraron {count} archivos de migración" : "No hay archivos de migración",
            StatusCode = 200
        };
        return Ok(response);
    }

    /// <summary>
    /// Elimina todos los archivos de migración existentes
    /// </summary>
    [HttpDelete("delete-migrations")]
    public async Task<IActionResult> DeleteMigrations()
    {
        var result = await _dbTools.DeleteMigrationsAsync();
        var response = new ApiResponse
        {
            Success = true,
            Data = new { Result = result },
            Message = "Operación de eliminación de migraciones completada",
            StatusCode = 200
        };
        return Ok(response);
    }

    /// <summary>
    /// 🚀 PROCESO COMPLETO AUTOMATIZADO: Elimina tablas, elimina migraciones antiguas, crea nueva migración y la aplica
    /// </summary>
    /// <param name="name">Nombre de la migración (por defecto: "Initial")</param>
    [HttpPost("full-migration")]
    public async Task<IActionResult> FullMigration([FromQuery] string name = "Initial")
    {
        var result = await _dbTools.FullMigrationAsync(name);
        var response = new ApiResponse
        {
            Success = !result.Contains("❌"),
            Data = new { Result = result },
            Message = !result.Contains("❌") ? "Proceso completo de migración ejecutado exitosamente" : "Error en proceso de migración",
            StatusCode = !result.Contains("❌") ? 200 : 400
        };
        return !result.Contains("❌") ? Ok(response) : BadRequest(response);
    }

    /// <summary>
    /// Crea una nueva migración (requiere base de datos vacía)
    /// </summary>
    /// <param name="name">Nombre de la migración</param>
    [HttpPost("create-migration")]
    public async Task<IActionResult> CreateMigration([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "El nombre de la migración es requerido",
                StatusCode = 400
            });
        }

        var result = await _dbTools.CreateMigrationAsync(name);
        var response = new ApiResponse
        {
            Success = !result.Contains("❌"),
            Data = new { Result = result },
            Message = !result.Contains("❌") ? "Migración creada correctamente" : "Error creando migración",
            StatusCode = !result.Contains("❌") ? 200 : 400
        };
        return !result.Contains("❌") ? Ok(response) : BadRequest(response);
    }

    /// <summary>
    /// Aplica migraciones pendientes a la base de datos
    /// </summary>
    [HttpPost("apply-migration")]
    public async Task<IActionResult> ApplyMigration()
    {
        var result = await _dbTools.ApplyMigrationAsync();
        var response = new ApiResponse
        {
            Success = !result.Contains("❌"),
            Data = new { Result = result },
            Message = !result.Contains("❌") ? "Migraciones aplicadas correctamente" : "Error aplicando migraciones",
            StatusCode = !result.Contains("❌") ? 200 : 400
        };
        return !result.Contains("❌") ? Ok(response) : BadRequest(response);
    }
    
    /// <summary>
    /// Elimina todas las tablas de la base de datos (mantiene historial de migraciones)
    /// </summary>
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

    /// <summary>
    /// Elimina completamente la base de datos
    /// </summary>
    [HttpDelete("drop")]
    public async Task<IActionResult> DropDatabase()
    {
        var result = await _dbTools.DropDatabaseAsync();
        var response = new ApiResponse
        {
            Success = true,
            Data = new { Result = result },
            Message = "Operación de eliminación de base de datos completada",
            StatusCode = 200
        };
        return Ok(response);
    }

    /// <summary>
    /// Proceso simplificado: elimina tablas, crea migración y la aplica (sin eliminar archivos de migración)
    /// </summary>
    /// <param name="name">Nombre de la migración (por defecto: "Initial")</param>
    [HttpPost("recreate-database")]
    public async Task<IActionResult> RecreateDatabase([FromQuery] string name = "Initial")
    {
        var result = await _dbTools.RecreateDatabaseAsync(name);
        var response = new ApiResponse
        {
            Success = !result.Contains("❌"),
            Data = new { Result = result },
            Message = !result.Contains("❌") ? "Base de datos recreada exitosamente" : "Error recreando base de datos",
            StatusCode = !result.Contains("❌") ? 200 : 400
        };
        return !result.Contains("❌") ? Ok(response) : BadRequest(response);
    }
}
