using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.Infrastructure.System.Services;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Microsoft.Extensions.Configuration;

namespace AVASphere.WebApi.System.Controllers;

[ApiController]
[Route("api/system/[controller]")]
[ApiExplorerSettings(GroupName = "System")]
[Tags("System - Up Tools")]
public class ToolsController : ControllerBase
{
    private readonly DatabaseMigrationService _dbMigrationService;
    private readonly IConfiguration _configuration;

    public ToolsController(DatabaseMigrationService dbMigrationService, IConfiguration configuration)
    {
        _dbMigrationService = dbMigrationService;
        _configuration = configuration;
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

    [HttpPost("cleanup-duplicate-fields")]
    public async Task<IActionResult> CleanupDuplicateFields()
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection") ?? 
                                 _configuration["DbSettings:ConnectionString"];
            
            if (string.IsNullOrEmpty(connectionString))
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Connection string not found",
                    StatusCode = 400
                });
            }

            var cleanupScript = GetCleanupScript();
            var results = new List<string>();

            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(cleanupScript, connection);
            command.CommandTimeout = 120; // 2 minutos timeout

            await using var reader = await command.ExecuteReaderAsync();
            
            // Capturar mensajes de RAISE NOTICE
            connection.Notice += (sender, args) => {
                results.Add(args.Notice.MessageText);
            };

            var response = new ApiResponse
            {
                Success = true,
                Data = new { 
                    results = results,
                    message = "Campos duplicados específicos eliminados exitosamente",
                    info = "Se eliminaron: StockMovement.WarehouseIdWarehouse y StorageStructure.AreaIdArea"
                },
                Message = "Limpieza de campos duplicados completada",
                StatusCode = 200
            };
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = $"Error durante limpieza: {ex.Message}",
                StatusCode = 400
            });
        }
    }

    [HttpPost("cleanup-and-migrate")]
    public async Task<IActionResult> CleanupAndMigrate()
    {
        try
        {
            // Paso 1: Limpiar campos duplicados
            var cleanupResult = await CleanupDuplicateFields();
            if (cleanupResult is not OkObjectResult)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Falló la limpieza de campos duplicados",
                    StatusCode = 400
                });
            }

            // Paso 2: Aplicar migraciones
            var migrationResult = await _dbMigrationService.ApplyMigrationsForceAsync();

            var response = new ApiResponse
            {
                Success = true,
                Data = new { 
                    cleanupCompleted = true,
                    migrationResult = migrationResult,
                    message = "Limpieza y migraciones aplicadas exitosamente"
                },
                Message = "Proceso completo finalizado",
                StatusCode = 200
            };
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = $"Error durante el proceso: {ex.Message}",
                StatusCode = 400
            });
        }
    }

    [HttpPost("synchronize-migration-history")]
    public async Task<IActionResult> SynchronizeMigrationHistory()
    {
        var result = await _dbMigrationService.SynchronizeMigrationHistoryAsync();
        var response = new ApiResponse
        {
            Success = true,
            Data = new { message = result },
            Message = "Historial de migraciones sincronizado",
            StatusCode = 200
        };
        return Ok(response);
    }

    [HttpGet("check-database-tables")]
    public async Task<IActionResult> CheckDatabaseTables()
    {
        var result = await _dbMigrationService.CheckDatabaseTablesAsync();
        var response = new ApiResponse
        {
            Success = true,
            Data = new { message = result },
            Message = "Verificación de tablas completada",
            StatusCode = 200
        };
        return Ok(response);
    }

    private static string GetCleanupScript()
    {
        return @"
        -- Script para limpiar campos duplicados específicos
        DO $$ 
        BEGIN
            -- TABLA: StockMovement - Eliminar WarehouseIdWarehouse
            IF EXISTS (SELECT 1 FROM information_schema.table_constraints 
                      WHERE constraint_name = 'FK_StockMovement_Warehouse_WarehouseIdWarehouse' 
                      AND table_name = 'StockMovement') THEN
                ALTER TABLE ""StockMovement"" DROP CONSTRAINT ""FK_StockMovement_Warehouse_WarehouseIdWarehouse"";
                RAISE NOTICE '✓ Eliminada FK_StockMovement_Warehouse_WarehouseIdWarehouse';
            END IF;

            IF EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_StockMovement_WarehouseIdWarehouse') THEN
                DROP INDEX ""IX_StockMovement_WarehouseIdWarehouse"";
                RAISE NOTICE '✓ Eliminado índice IX_StockMovement_WarehouseIdWarehouse';
            END IF;

            IF EXISTS (SELECT 1 FROM information_schema.columns 
                      WHERE table_name = 'StockMovement' AND column_name = 'WarehouseIdWarehouse') THEN
                ALTER TABLE ""StockMovement"" DROP COLUMN ""WarehouseIdWarehouse"";
                RAISE NOTICE '✓ Eliminada columna StockMovement.WarehouseIdWarehouse';
            END IF;

            -- TABLA: StorageStructure - Eliminar AreaIdArea
            IF EXISTS (SELECT 1 FROM information_schema.table_constraints 
                      WHERE constraint_name = 'FK_StorageStructure_Area_AreaIdArea' 
                      AND table_name = 'StorageStructure') THEN
                ALTER TABLE ""StorageStructure"" DROP CONSTRAINT ""FK_StorageStructure_Area_AreaIdArea"";
                RAISE NOTICE '✓ Eliminada FK_StorageStructure_Area_AreaIdArea';
            END IF;

            IF EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_StorageStructure_AreaIdArea') THEN
                DROP INDEX ""IX_StorageStructure_AreaIdArea"";
                RAISE NOTICE '✓ Eliminado índice IX_StorageStructure_AreaIdArea';
            END IF;

            IF EXISTS (SELECT 1 FROM information_schema.columns 
                      WHERE table_name = 'StorageStructure' AND column_name = 'AreaIdArea') THEN
                ALTER TABLE ""StorageStructure"" DROP COLUMN ""AreaIdArea"";
                RAISE NOTICE '✓ Eliminada columna StorageStructure.AreaIdArea';
            END IF;

            -- Verificación final
            DECLARE
                remaining_duplicates INTEGER := 0;
            BEGIN
                SELECT COUNT(*) INTO remaining_duplicates
                FROM information_schema.columns 
                WHERE table_schema = 'public' 
                AND ((table_name = 'StockMovement' AND column_name = 'WarehouseIdWarehouse') OR
                     (table_name = 'StorageStructure' AND column_name = 'AreaIdArea'));
                
                IF remaining_duplicates = 0 THEN
                    RAISE NOTICE '🎉 SUCCESS: Todos los campos duplicados específicos han sido eliminados!';
                    RAISE NOTICE 'StockMovement ahora solo tiene: IdWarehouse (✓)';
                    RAISE NOTICE 'StorageStructure ahora solo tiene: IdArea (✓)';
                ELSE
                    RAISE NOTICE '⚠️  WARNING: Aún quedan % campos duplicados por limpiar', remaining_duplicates;
                END IF;
            END;

            RAISE NOTICE '🚀 Limpieza específica completada!';
        END $$;
        ";
    }
}
