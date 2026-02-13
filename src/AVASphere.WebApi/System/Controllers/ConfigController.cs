using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.Entities.General;
using AVASphere.ApplicationCore.Common.Enums;
using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.ApplicationCore.System.DTOs;
using AVASphere.ApplicationCore.Common.Entities.Catalogs;
using AVASphere.Infrastructure;
using AVASphere.Infrastructure.System.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;
using System.Data.Common;
using System.Text;

namespace AVASphere.WebApi.System.Controllers;

[ApiController]
[Route("api/system/[controller]")]
[ApiExplorerSettings(GroupName = "System")]
[Tags("System - Up Tools")]
public class ConfigController : ControllerBase
{
    private readonly DatabaseMigrationService _dbMigrationService;
    private readonly MasterDbContext _dbContext;
    private readonly IEncryptionService _encryptionService;
    private readonly BackupService _backupService;

    public ConfigController(
        DatabaseMigrationService dbMigrationService,
        MasterDbContext dbContext,
        IEncryptionService encryptionService,
        BackupService backupService)
    {
        _dbMigrationService = dbMigrationService;
        _dbContext = dbContext;
        _encryptionService = encryptionService;
        _backupService = backupService;
    }

    [HttpGet("check-initial-config")]
    public async Task<IActionResult> CheckInitialConfig()
    {
        try
        {
            ConfigSys? config = null;
            bool tableExists;
            bool hasConfiguration;

            try
            {
                var canConnect = await _dbContext.Database.CanConnectAsync();
                if (!canConnect)
                {
                    var response = new ApiResponse
                    {
                        Success = false,
                        Data = new
                        {
                            HasConfiguration = false,
                            TableExists = false,
                            RequiresMigration = true
                        },
                        Message = "No se puede conectar a la base de datos",
                        StatusCode = 200
                    };
                    return Ok(response);
                }

                config = await _dbContext.ConfigSys.FirstOrDefaultAsync();
                tableExists = true;
                hasConfiguration = config != null;
            }
            catch (Exception dbEx)
            {
                var errorMessage = dbEx.Message.ToLower();
                var innerMessage = dbEx.InnerException?.Message?.ToLower() ?? string.Empty;

                if (errorMessage.Contains("does not exist") ||
                    errorMessage.Contains("relation") ||
                    errorMessage.Contains("table") ||
                    innerMessage.Contains("does not exist") ||
                    innerMessage.Contains("relation") ||
                    (dbEx is PostgresException pgEx && pgEx.SqlState == "42P01"))
                {
                    tableExists = false;
                    hasConfiguration = false;
                }
                else
                {
                    throw;
                }
            }

            if (!tableExists)
            {
                var response = new ApiResponse
                {
                    Success = false,
                    Data = new
                    {
                        HasConfiguration = false,
                        TableExists = false,
                        RequiresMigration = true
                    },
                    Message = "La tabla ConfigSys no existe. Se requiere ejecutar migraciones.",
                    StatusCode = 200
                };
                return Ok(response);
            }

            if (!hasConfiguration)
            {
                var response = new ApiResponse
                {
                    Success = false,
                    Data = new
                    {
                        HasConfiguration = false,
                        TableExists = true,
                        RequiresMigration = false
                    },
                    Message = "La tabla ConfigSys existe pero no hay configuración inicial del sistema",
                    StatusCode = 200
                };
                return Ok(response);
            }

            var successResponse = new ApiResponse
            {
                Success = true,
                Data = new
                {
                    HasConfiguration = true,
                    TableExists = true,
                    RequiresMigration = false,
                    Configuration = new
                    {
                        config!.IdConfigSys,
                        config.CompanyName,
                        config.BranchName,
                        config.LogoUrl,
                        ColorsCount = config.Colors.Count,
                        NotUseModulesCount = config.NotUseModules.Count,
                        config.CreatedAt
                    }
                },
                Message = "Configuración inicial encontrada y tabla ConfigSys existe",
                StatusCode = 200
            };
            return Ok(successResponse);
        }
        catch (Exception ex)
        {
            var errorResponse = new ApiResponse
            {
                Success = false,
                Data = new
                {
                    HasConfiguration = false,
                    TableExists = false,
                    RequiresMigration = true
                },
                Message = $"Error al verificar la configuración inicial: {ex.Message}",
                StatusCode = 500
            };
            return StatusCode(500, errorResponse);
        }
    }

    [HttpGet("diagnose-migrations")]
    public async Task<IActionResult> DiagnoseMigrations()
    {
        var result = await _dbMigrationService.DiagnoseMigrationsAsync();
        var response = new ApiResponse
        {
            Success = true,
            Data = new
            {
                diagnosis = result,
                info = "Diagnóstico detallado del estado de las migraciones"
            },
            Message = "Diagnóstico de migraciones completado",
            StatusCode = 200
        };
        return Ok(response);
    }

    [HttpPost("configure-system")]
    public async Task<IActionResult> ConfigureSystem([FromBody] SystemConfigRequestDto request)
    {
        try
        {
            // Verificar si ya existe una configuración
            var existingConfig = await _dbContext.ConfigSys.FirstOrDefaultAsync();
            if (existingConfig != null)
            {
                var badResponse = new ApiResponse
                {
                    Success = false,
                    Message = "Ya existe una configuración del sistema. Use el endpoint de actualización.",
                    StatusCode = 400
                };
                return BadRequest(badResponse);
            }

            // Crear los módulos no utilizados basados en el enum
            var notUseModules = new List<NotUseModuleJson>();
            if (request.NotUseModules != null && request.NotUseModules.Any())
            {
                foreach (var moduleValue in request.NotUseModules)
                {
                    if (Enum.IsDefined(typeof(SystemModule), moduleValue))
                    {
                        var moduleName = ((SystemModule)moduleValue).ToString();
                        notUseModules.Add(new NotUseModuleJson
                        {
                            Index = moduleValue,
                            NameModule = moduleName
                        });
                    }
                }
            }

            // Crear la configuración del sistema
            var config = new ConfigSys
            {
                CompanyName = request.CompanyName,
                BranchName = request.BranchName,
                LogoUrl = request.LogoUrl,
                Colors = request.Colors?.Select(c => new ColorsJson
                {
                    Index = c.Index,
                    NameColor = c.NameColor,
                    ColorCode = c.ColorCode,
                    ColorRgb = c.ColorRgb
                }).ToList() ?? new List<ColorsJson>(),
                NotUseModules = notUseModules,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.ConfigSys.Add(config);
            await _dbContext.SaveChangesAsync();

            var response = new ApiResponse
            {
                Success = true,
                Data = new
                {
                    config.IdConfigSys,
                    config.CompanyName,
                    config.BranchName,
                    config.LogoUrl,
                    ColorsCount = config.Colors.Count,
                    NotUseModulesCount = config.NotUseModules.Count,
                    config.CreatedAt
                },
                Message = "Configuración del sistema creada exitosamente",
                StatusCode = 200
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            var errorResponse = new ApiResponse
            {
                Success = false,
                Message = $"Error al configurar el sistema: {ex.Message}",
                StatusCode = 500
            };
            return StatusCode(500, errorResponse);
        }
    }

    [HttpPost("configure-admin")]
    public async Task<IActionResult> ConfigureAdmin([FromBody] AdminUserRequestDto request)
    {
        try
        {
            // Verificar si existe la configuración del sistema
            var configSys = await _dbContext.ConfigSys.FirstOrDefaultAsync();
            if (configSys == null)
            {
                var badResponse = new ApiResponse
                {
                    Success = false,
                    Message = "Debe configurar el sistema primero antes de crear el usuario administrador.",
                    StatusCode = 400
                };
                return BadRequest(badResponse);
            }

            // Verificar si ya existe un usuario administrador
            var existingAdmin = await _dbContext.Users
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Rol.NormalizedName == "admin");

            if (existingAdmin != null)
            {
                var badResponse = new ApiResponse
                {
                    Success = false,
                    Message = "Ya existe un usuario administrador configurado.",
                    StatusCode = 400
                };
                return BadRequest(badResponse);
            }

            // Crear o obtener el área de Sistema
            var areaSystem = await _dbContext.Areas
                .FirstOrDefaultAsync(a => a.NormalizedName == "SYSTEM");

            if (areaSystem == null)
            {
                areaSystem = new Area
                {
                    Name = "Sistema",
                    NormalizedName = "SYSTEM"
                };
                _dbContext.Areas.Add(areaSystem);
                await _dbContext.SaveChangesAsync();
            }

            // Crear el rol de Administrador con módulo específico y permisos vacíos
            var adminRole = new Rol
            {
                Name = "Administrador",
                NormalizedName = "admin",
                IdArea = areaSystem.IdArea,
                Permissions = new List<Permission>(), // ✅ ARREGLO VACÍO según solicitud
                Modules = new List<Module>           // ✅ SOLO MÓDULO 'Total' según solicitud
                {
                    new Module
                    {
                        Name = "Total",
                        Index = 0,
                        Normalized = "/all"
                    }
                }
            };

            _dbContext.Rols.Add(adminRole);
            await _dbContext.SaveChangesAsync();

            // Crear el hash de la contraseña
            var hashPassword = _encryptionService.HashPassword(request.Password);

            // Crear el usuario administrador
            var adminUser = new User
            {
                UserName = request.UserName,
                Name = "Administrador",
                LastName = "Sistema",
                HashPassword = hashPassword,
                Status = "active",
                Verified = true,
                CreateAt = DateOnly.FromDateTime(DateTime.UtcNow),
                IdRol = adminRole.IdRol,
                IdConfigSys = configSys.IdConfigSys
            };

            _dbContext.Users.Add(adminUser);
            await _dbContext.SaveChangesAsync();

            var response = new ApiResponse
            {
                Success = true,
                Data = new
                {
                    adminUser.IdUser,
                    adminUser.UserName,
                    Role = new
                    {
                        adminRole.IdRol,
                        adminRole.Name,
                        adminRole.NormalizedName,
                        PermissionsCount = adminRole.Permissions?.Count ?? 0,
                        ModulesCount = adminRole.Modules?.Count ?? 0
                    },
                    Area = new
                    {
                        areaSystem.IdArea,
                        areaSystem.Name,
                        areaSystem.NormalizedName
                    }
                },
                Message = "Usuario administrador creado exitosamente",
                StatusCode = 200
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            var errorResponse = new ApiResponse
            {
                Success = false,
                Data = new { Details = ex.InnerException?.Message },
                Message = $"Error al crear el usuario administrador: {ex.Message}",
                StatusCode = 500
            };
            return StatusCode(500, errorResponse);
        }
    }

    #region Backup Service Endpoints

    [HttpGet("available-tables")]
    public async Task<IActionResult> GetAvailableTables()
    {
        try
        {
            var tables = await _backupService.GetAvailableTablesAsync();

            var response = new ApiResponse
            {
                Success = true,
                Data = new AvailableTablesDto
                {
                    Tables = tables,
                    TotalCount = tables.Count,
                    RetrievedAt = DateTime.UtcNow
                },
                Message = $"Se encontraron {tables.Count} tablas disponibles",
                StatusCode = 200
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            var errorResponse = new ApiResponse
            {
                Success = false,
                Message = $"Error al obtener tablas disponibles: {ex.Message}",
                StatusCode = 500
            };
            return StatusCode(500, errorResponse);
        }
    }

    [HttpPost("export-tables")]
    public async Task<IActionResult> ExportTables([FromBody] BackupTablesRequestDto request)
    {
        try
        {
            string sqlBackup;
            var backupName = request.BackupName ?? $"Backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}";

            if (request.ExportAllTables)
            {
                sqlBackup = await _backupService.ExportAllTablesAsSqlAsync();
            }
            else
            {
                if (!request.TableNames.Any())
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Debe especificar al menos una tabla o activar 'ExportAllTables'",
                        StatusCode = 400
                    });
                }
                sqlBackup = await _backupService.ExportSelectedTablesAsSqlAsync(request.TableNames);
            }

            var fileName = $"{backupName}.sql";
            var fileBytes = Encoding.UTF8.GetBytes(sqlBackup);

            return File(fileBytes, "application/sql", fileName);
        }
        catch (Exception ex)
        {
            var errorResponse = new ApiResponse
            {
                Success = false,
                Message = $"Error al exportar tablas: {ex.Message}",
                StatusCode = 500
            };
            return StatusCode(500, errorResponse);
        }
    }

    /// <summary>
    /// Importa datos desde un archivo SQL
    /// </summary>
    /// <param name="sqlFile">Archivo SQL a importar</param>
    /// <param name="overwrite">Si debe sobrescribir datos existentes</param>
    /// <returns>Resultado de la importación</returns>
    [HttpPost("import-sql")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
    public async Task<IActionResult> ImportFromSql(IFormFile sqlFile, bool overwrite = false)
    {
        try
        {
            if (sqlFile.Length == 0)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "No se proporcionó un archivo SQL válido",
                    StatusCode = 400
                });
            }

            string sqlContent;
            using (var reader = new StreamReader(sqlFile.OpenReadStream(), Encoding.UTF8))
            {
                sqlContent = await reader.ReadToEndAsync();
            }

            var result = await _backupService.ImportFromSqlAsync(sqlContent, overwrite);

            var response = new ApiResponse
            {
                Success = result.Success,
                Data = result,
                Message = result.Message,
                StatusCode = result.Success ? 200 : 400
            };

            return result.Success ? Ok(response) : BadRequest(response);
        }
        catch (Exception ex)
        {
            var errorResponse = new ApiResponse
            {
                Success = false,
                Message = $"Error al importar archivo SQL: {ex.Message}",
                StatusCode = 500
            };
            return StatusCode(500, errorResponse);
        }
    }

    #endregion

    #region Legacy Catalog Backup Endpoints (Deprecated - usar nuevos endpoints)

    [HttpPost("backup-catalogs")]
    [Obsolete("Usar /export-tables con tablas específicas")]
    public IActionResult BackupCatalogs([FromBody] CatalogBackupRequestDto request)
    {
        try
        {
            var tableNames = new List<string>();
            if (request.IncludeProperties) tableNames.Add("Property");
            if (request.IncludeSuppliers) tableNames.Add("Supplier");
            if (request.IncludePropertyValues) tableNames.Add("PropertyValue");

            if (!tableNames.Any())
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Debe seleccionar al menos un tipo de catálogo",
                    StatusCode = 400
                });
            }

            // Crear un backup response simplificado para compatibilidad
            var response = new ApiResponse
            {
                Success = true,
                Data = new CatalogBackupResponseDto
                {
                    BackupId = Guid.NewGuid().ToString(),
                    BackupName = request.BackupName ?? $"Catalogs_Backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}",
                    CreatedAt = DateTime.UtcNow,
                    Description = request.Description,
                    // Nota: No se incluyen datos detallados para mantener compatibilidad
                },
                Message = "Backup de catálogos creado exitosamente (usar nuevos endpoints para funcionalidad completa)",
                StatusCode = 200
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            var errorResponse = new ApiResponse
            {
                Success = false,
                Message = $"Error al crear backup de catálogos: {ex.Message}",
                StatusCode = 500
            };
            return StatusCode(500, errorResponse);
        }
    }

    /// <summary>
    /// Importa los catálogos por defecto del sistema (Property, Supplier, PropertyValue)
    /// </summary>
    /// <param name="overwrite">Si es true, sobrescribe datos existentes. Si es false, solo inserta nuevos.</param>
    /// <returns>Resultado de la importación con estadísticas detalladas</returns>
    /// <remarks>
    /// Este endpoint importa:
    /// - 3 Properties (Familia, Clase, Línea)
    /// - 36 Suppliers (La Viga, Casa Fernández, Herralum, etc.)
    /// - 150+ PropertyValues organizados por categorías
    /// </remarks>
    [HttpPost("load-default-catalogs")]
    public async Task<IActionResult> LoadDefaultCatalogs([FromQuery] bool overwrite = false)
    {
        try
        {
            // SQL con datos por defecto desde BackupService
            var defaultSql = _backupService.GetDefaultCatalogsSql();

            var result = await _backupService.ImportFromSqlAsync(defaultSql, overwrite);

            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = $"Error al cargar catálogos por defecto: {ex.Message}"
            });
        }
    }

    #endregion

    #region Delete Catalog Data

    /// <summary>
    /// Elimina todos los datos de las 6 tablas de catálogos base y todas sus dependencias, reiniciando las secuencias
    /// </summary>
    /// <returns>Resultado de la operación</returns>

    [HttpDelete("delete-catalog-data")]
    public async Task<IActionResult> DeleteCatalogData()
    {
        try
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // Eliminar TODAS las tablas dependientes primero, luego las 6 tablas base
                var deleteCommands = new[]
                {
                    // ========================================
                    // NIVEL 1: Detalles y tablas más dependientes
                    // ========================================
                    "DELETE FROM \"PhysicalInventoryDetail\";",
                    "ALTER SEQUENCE IF EXISTS \"PhysicalInventoryDetail_IdPhysicalInventoryDetail_seq\" RESTART WITH 1;",

                    "DELETE FROM \"WarehouseTransferDetail\";",
                    "ALTER SEQUENCE IF EXISTS \"WarehouseTransferDetail_IdWarehouseTransferDetail_seq\" RESTART WITH 1;",
                    
                    // ========================================
                    // NIVEL 2: Tablas de inventario que dependen de Product/Warehouse
                    // ========================================
                    "DELETE FROM \"PhysicalInventory\";",
                    "ALTER SEQUENCE IF EXISTS \"PhysicalInventory_IdPhysicalInventory_seq\" RESTART WITH 1;",

                    "DELETE FROM \"WarehouseTransfer\";",
                    "ALTER SEQUENCE IF EXISTS \"WarehouseTransfer_IdWarehouseTransfer_seq\" RESTART WITH 1;",

                    "DELETE FROM \"StockMovement\";",
                    "ALTER SEQUENCE IF EXISTS \"StockMovement_IdStockMovement_seq\" RESTART WITH 1;",

                    "DELETE FROM \"Inventory\";",
                    "ALTER SEQUENCE IF EXISTS \"Inventory_IdInventory_seq\" RESTART WITH 1;",
                    
                    // ========================================
                    // NIVEL 3: Ubicaciones y estructuras de almacén
                    // ========================================
                    "DELETE FROM \"LocationDetails\";",
                    "ALTER SEQUENCE IF EXISTS \"LocationDetails_IdLocationDetails_seq\" RESTART WITH 1;",

                    "DELETE FROM \"StorageStructure\";",
                    "ALTER SEQUENCE IF EXISTS \"StorageStructure_IdStorageStructure_seq\" RESTART WITH 1;",
                    
                    // ========================================
                    // NIVEL 4: LAS 6 TABLAS DE CATÁLOGOS BASE
                    // ========================================
                    
                    // ProductProperties (depende de Product y Property)
                    "DELETE FROM \"ProductProperties\";",
                    "ALTER SEQUENCE IF EXISTS \"ProductProperties_IdProductProperties_seq\" RESTART WITH 1;",
                    
                    // PropertyValue (depende de Property)
                    "DELETE FROM \"PropertyValue\";",
                    "ALTER SEQUENCE IF EXISTS \"PropertyValue_IdPropertyValue_seq\" RESTART WITH 1;",
                    
                    // Product (tabla principal)
                    "DELETE FROM \"Product\";",
                    "ALTER SEQUENCE IF EXISTS \"Product_IdProduct_seq\" RESTART WITH 1;",
                    
                    // Property (tabla base)
                    "DELETE FROM \"Property\";",
                    "ALTER SEQUENCE IF EXISTS \"Property_IdProperty_seq\" RESTART WITH 1;",
                    
                    // Supplier (tabla independiente)
                    "DELETE FROM \"Supplier\";",
                    "ALTER SEQUENCE IF EXISTS \"Supplier_IdSupplier_seq\" RESTART WITH 1;",
                    
                    // Warehouse (tabla independiente)
                    "DELETE FROM \"Warehouse\";",
                    "ALTER SEQUENCE IF EXISTS \"Warehouse_IdWarehouse_seq\" RESTART WITH 1;"
                };

                foreach (var command in deleteCommands)
                {
                    await _dbContext.Database.ExecuteSqlRawAsync(command);
                }

                await transaction.CommitAsync();

                var response = new ApiResponse
                {
                    Success = true,
                    Message = "Datos de catálogos e inventario eliminados exitosamente",
                    StatusCode = 200,
                    Data = new
                    {
                        InventoryTablesCleared = new[]
                        {
                            "PhysicalInventoryDetail",
                            "PhysicalInventory",
                            "WarehouseTransferDetail",
                            "WarehouseTransfer",
                            "StockMovement",
                            "Inventory",
                            "LocationDetails",
                            "StorageStructure"
                        },
                        CatalogTablesCleared = new[]
                        {
                            "ProductProperties",
                            "PropertyValue",
                            "Product",
                            "Property",
                            "Supplier",
                            "Warehouse"
                        },
                        TotalTablesCleared = 14,
                        SequencesReset = true
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            var response = new ApiResponse
            {
                Success = false,
                Message = $"Error al eliminar datos de catálogos: {ex.Message}",
                StatusCode = 500,
                Data = new
                {
                    Error = ex.Message
                }
            };

            return StatusCode(500, response);
        }
    }

    #endregion
}









