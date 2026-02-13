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
            Data = new {
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

    [HttpPost("load-default-catalogs")]
    public async Task<IActionResult> LoadDefaultCatalogs([FromQuery] bool overwrite = false)
    {
        try
        {
            // SQL con datos por defecto
            var defaultSql = GetDefaultCatalogsSql();
            
            var result = await _backupService.ImportFromSqlAsync(defaultSql, overwrite);
            
            var response = new ApiResponse
            {
                Success = result.Success,
                Data = result,
                Message = result.Success ? 
                    "Catálogos por defecto cargados exitosamente. " + result.Message :
                    "Error al cargar catálogos por defecto: " + result.Message,
                StatusCode = result.Success ? 200 : 400
            };
            
            return result.Success ? Ok(response) : BadRequest(response);
        }
        catch (Exception ex)
        {
            var errorResponse = new ApiResponse
            {
                Success = false,
                Message = $"Error al cargar catálogos por defecto: {ex.Message}",
                StatusCode = 500
            };
            return StatusCode(500, errorResponse);
        }
    }

    #endregion

    #region Private Methods

    private static string GetDefaultCatalogsSql()
    {
        return @"
-- PROPERTIES
INSERT INTO ""Property""(""Name"",""NormalizedName"") VALUES
('Familia','Family'),
('Clase','Class'),
('Línea','Line');

-- SUPPLIERS
INSERT INTO ""Supplier""(""Name"",""RegistrationDate"") 
VALUES
('LA VIGA EXTRUSIONES','2026-02-04'),
('LA VIGA CUPRUM','2026-02-04'),
('LA VIGA INDALUM','2026-02-04'),
('CASA FERNANDEZ DEL SURESTE, S.A. DE C.V.','2026-02-04'),
('HERRALUM INDUSTRIAL, S.A. DE C.V.','2026-02-04'),
('ASSA ABLOY MEXICO, S.A. DE C.V.','2026-02-04'),
('JACKSON CORPORATION MEXICO, SA DE CV','2026-02-04'),
('CONSORCIO DE ALUMINIO DEL SURESTE','2026-02-04'),
('M INDUSTRIA, S.A. DE C.V.','2026-02-04'),
('TECNO EXTRUSIONES, S.A. DE C.V.','2026-02-04'),
('PLASTICOS ESPECIALES GAREN, S A DE C V','2026-02-04'),
('ACRILICOS PLASTITEC, S.A. DE C.V.','2026-02-04'),
('PERFILETTO ALUMINIO, S.A. DE C.V.','2026-02-04'),
('GUARDIAN INDUSTRIES VP S DE RL DE CV','2026-02-04'),
('BRUKEN INTERNACIONAL SA DE CV','2026-02-04'),
('ROBIN HERRAJES Y ACCESORIOS, S DE RL DE CV','2026-02-04'),
('ACRILICOS NEWTON, S.A. DE C.V.','2026-02-04'),
('CASA GALVAN TODO PARA VIDRIO SA DE CV','2026-02-04'),
('PELICULAS GARCIA','2026-02-04'),
('MASTIQUES MADISON, S.A. DE C.V.','2026-02-04'),
('WURTH MEXICO S,A, DE C,V,','2026-02-04'),
('ARTEFACTOS DE PRECISION, S.A. DE C.V.','2026-02-04'),
('INDUX SA DE CV','2026-02-04'),
('ALUMINIOS Y VIDRIOS ARMANDO Y CIA','2026-02-04'),
('FANAL, S.A. DE C.V.','2026-02-04'),
('DB HERRAJES SA DE CV','2026-02-04'),
('ASESORIA TECNICA PENINSULAR, S.A.','2026-02-04'),
('HERRAJES EUROPEOS','2026-02-04'),
('JR DE MEXICO','2026-02-04'),
('ARBI INDUSTRIAL, S A DE C.V.','2026-02-04'),
('CHAPA INDUSTRIAS S.A. DE C.V.','2026-02-04'),
('VDS DIVISION PENINSULAR DEL MAYAB S.A. DE C.V.','2026-02-04'),
('MERIK, S.A. DE C.V.','2026-02-04'),
('DISTRIBUIDORA MAYORISTA DE TORNILLOS DE YUCATAN S.A. DE C.V.','2026-02-04'),
('ARGENTUM MEXICANA S DE RL DE CV','2026-02-04'),
('CUPRUM CAMPECHE','2026-02-04');

-- PROPERTY VALUES - FAMILIAS (IdProperty = 1)
INSERT INTO ""PropertyValue""(""Value"", ""FatherValue"", ""Type"", ""IdProperty"") 
VALUES
('ACRILICOS', NULL, NULL, 1),
('ALUMINIO BEIGE Y ORO', NULL, NULL, 1),
('ALUMINIO CAFE (18)', NULL, NULL, 1),
('ALUMINIO CAFE OSCURO (58)', NULL, NULL, 1),
('ALUMINIO CHAMPAGNE (4)', NULL, NULL, 1),
('ALUMINIO CORPORATIVO VALSA', NULL, NULL, 1),
('ALUMINIO INDALUM', NULL, NULL, 1),
('ALUMINIO LINEA ESPAÑOLA', NULL, NULL, 1),
('ALUMINIO LINEA ESPECIAL', NULL, NULL, 1),
('ALUMINIO LINEA NACIONAL', NULL, NULL, 1),
('ALUMINIO OUTLET', NULL, NULL, 1),
('BISAGRAS HIDRAULICAS', NULL, NULL, 1),
('CARRETILLAS ASSA ABLOY', NULL, NULL, 1),
('CARRETILLAS HERRALUM', NULL, NULL, 1),
('CARRETILLAS TECNO PROMOCION', NULL, NULL, 1),
('CELOSIAS', NULL, NULL, 1),
('CERRADURA ASSA', NULL, NULL, 1),
('CERRADURAS', NULL, NULL, 1),
('CERRADURAS HERRALUM', NULL, NULL, 1),
('CORREDIZA LIGHT 1 1/2', NULL, NULL, 1),
('CRISTAL POR M2', NULL, NULL, 1),
('CRISTAL TEMPLADOS HERRALUM', NULL, NULL, 1),
('CRISTALES', NULL, NULL, 1),
('CRISTALES ESPECIALES', NULL, NULL, 1),
('CRISTALES MILLET', NULL, NULL, 1),
('ESPECIALES MADERA', NULL, NULL, 1),
('EXTRUSIONES 2 Y 3 BLANCO Y NATURAL', NULL, NULL, 1),
('FELPA HERRALUM', NULL, NULL, 1),
('GENERAL', NULL, NULL, 1),
('HERRAJES', NULL, NULL, 1),
('HERRAJES LINEA ESPAÑOLA', NULL, NULL, 1),
('HERRAJES OUTLET', NULL, NULL, 1),
('HERRALUM LINEAS ESPAÑOLAS', NULL, NULL, 1),
('HERRAMIENTAS', NULL, NULL, 1),
('JALADERAS', NULL, NULL, 1),
('JALADERAS ACERO INOXIDABLE', NULL, NULL, 1),
('KIT RIO BRAVO', NULL, NULL, 1),
('MERIK', NULL, NULL, 1),
('PANEL ART', NULL, NULL, 1),
('PELICULAS', NULL, NULL, 1),
('PIJAS Y TORNILLOS', NULL, NULL, 1),
('PLASTICO HERRALUM', NULL, NULL, 1),
('PLASTICOS', NULL, NULL, 1),
('POLICARBONATO METROS LINEALES', NULL, NULL, 1),
('POLICARBONATOS ROLLOS', NULL, NULL, 1),
('REMACHES', NULL, NULL, 1),
('RETROVISOR', NULL, NULL, 1),
('ROLLO PELICULAS HERRALUM', NULL, NULL, 1),
('SERIES CUPRUM', NULL, NULL, 1),
('SERVICIOS', NULL, NULL, 1),
('SILICONES', NULL, NULL, 1),
('TELA ALUMINIO GRIS Y NEGRA', NULL, NULL, 1),
('TELA ALUMINIO NEGRA', NULL, NULL, 1),
('TELA FIBRA HERRALUM (ROLLO)', NULL, NULL, 1),
('TELA FIBRA METROS', NULL, NULL, 1),
('TRABAJOS', NULL, NULL, 1),
('VARIOS LA VIGA HERRAJES', NULL, NULL, 1),
('VIDRIOS CILINDRADOS', NULL, NULL, 1),
('VIDRIOS FLORENTINO', NULL, NULL, 1),
('VINIL HERRALUM', NULL, NULL, 1),
('VINIL MADISON', NULL, NULL, 1);

-- PROPERTY VALUES - CLASES (IdProperty = 2)
INSERT INTO ""PropertyValue""(""Value"", ""FatherValue"", ""Type"", ""IdProperty"") 
VALUES
('ACRILICOS', NULL, NULL, 2),
('ANGULOS', NULL, NULL, 2),
('BATIENTES', NULL, NULL, 2),
('BAÑOS', NULL, NULL, 2),
('CELOSIAS', NULL, NULL, 2),
('CORREDIZAS', NULL, NULL, 2),
('DUELAS', NULL, NULL, 2),
('FIJOS', NULL, NULL, 2),
('MOSQUITEROS', NULL, NULL, 2),
('PORTAVIDRIOS', NULL, NULL, 2),
('PUERTAS', NULL, NULL, 2),
('TUBOS', NULL, NULL, 2),
('MOLDURAS', NULL, NULL, 2),
('PASAMANOS', NULL, NULL, 2),
('PROYECCION', NULL, NULL, 2),
('ESPAÑOLA', NULL, NULL, 2),
('GENERAL', NULL, NULL, 2),
('VITRINAS', NULL, NULL, 2),
('CANALES', NULL, NULL, 2),
('CUPRUM', NULL, NULL, 2),
('CRISTAL', NULL, NULL, 2),
('HERRAJES', NULL, NULL, 2),
('VIDRIO', NULL, NULL, 2),
('FELPAS Y FELPONES', NULL, NULL, 2),
('INOXIDABLE', NULL, NULL, 2),
('PELICULAS P/CRISTAL', NULL, NULL, 2),
('POLICARBONATOS', NULL, NULL, 2),
('SILICONES', NULL, NULL, 2),
('HERRAMIENTAS', NULL, NULL, 2),
('PORTONES', NULL, NULL, 2),
('PLASTICOS', NULL, NULL, 2),
('REMACHES', NULL, NULL, 2),
('TELAS MOSQUITERAS', NULL, NULL, 2),
('TRABAJOS', NULL, NULL, 2),
('VINILES', NULL, NULL, 2);

-- PROPERTY VALUES - LÍNEAS (IdProperty = 3)
INSERT INTO ""PropertyValue""(""Value"", ""FatherValue"", ""Type"", ""IdProperty"") 
VALUES
('ACRILICOS', NULL, NULL, 3),
('ACRILICOS DECORADOS', NULL, NULL, 3),
('ANGULOS', NULL, NULL, 3),
('BATIENTE 1750', NULL, NULL, 3),
('BAÑOS', NULL, NULL, 3),
('CELOSIAS', NULL, NULL, 3),
('CORREDIZA 1 1/2""', NULL, NULL, 3),
('CORREDIZA 2""', NULL, NULL, 3),
('CORREDIZA 3""', NULL, NULL, 3),
('DUELA LISA', NULL, NULL, 3),
('DUELA ONDULADA', NULL, NULL, 3),
('6000 ESPAÑOLA', NULL, NULL, 3),
('FIJA 2""', NULL, NULL, 3),
('FIJA 3""', NULL, NULL, 3),
('MOLDURAS', NULL, NULL, 3),
('MOSQUITEROS', NULL, NULL, 3),
('PASAMANO REDONDO', NULL, NULL, 3),
('PORTAVIDRIO 1""', NULL, NULL, 3),
('SERIE 35', NULL, NULL, 3),
('TUBO CUADRADO', NULL, NULL, 3),
('TUBO RECTANGULAR', NULL, NULL, 3),
('TUBO REDONDO', NULL, NULL, 3),
('VITRINA', NULL, NULL, 3),
('TEE', NULL, NULL, 3),
('3500 ESPAÑOLA', NULL, NULL, 3),
('3600 ESPAÑOLA', NULL, NULL, 3),
('4500 ESPAÑOLA', NULL, NULL, 3),
('4600 ESPAÑOLA', NULL, NULL, 3),
('CUADRICULA', NULL, NULL, 3),
('VARIOS', NULL, NULL, 3),
('PASAMANO OVALADO', NULL, NULL, 3),
('SERIE 70 EUROVENT', NULL, NULL, 3),
('BISAGRAS', NULL, NULL, 3),
('BISAGRAS ESPECIALES', NULL, NULL, 3),
('CARRETILLAS', NULL, NULL, 3),
('CANCELES DE BAÑO', NULL, NULL, 3),
('CERRADURAS', NULL, NULL, 3),
('FELPA', NULL, NULL, 3),
('GENERAL', NULL, NULL, 3),
('HERRAMIENTAS', NULL, NULL, 3),
('OPERADORES', NULL, NULL, 3),
('APARADORES', NULL, NULL, 3),
('BALANCINES', NULL, NULL, 3),
('BARRA DE EMPUJE', NULL, NULL, 3),
('BARRA DE SEGURIDAD', NULL, NULL, 3),
('BARRAS ANTIPANICOS', NULL, NULL, 3),
('BRAZO DE PROYECCION', NULL, NULL, 3),
('BROCAS', NULL, NULL, 3),
('CIERRA PUERTAS', NULL, NULL, 3),
('CIERRE FINAL', NULL, NULL, 3),
('CLIPOS', NULL, NULL, 3),
('COMENZA', NULL, NULL, 3),
('CONECTORES P/CRISTAL', NULL, NULL, 3),
('CORREDERAS P/MUEBLE', NULL, NULL, 3),
('CREMALLERAS', NULL, NULL, 3),
('CRISTAL TEMPLADO', NULL, NULL, 3),
('CRISTAL FILTRAPLUS', NULL, NULL, 3),
('CRISTAL FILTRASOL', NULL, NULL, 3),
('CRISTAL REFLECTASOL', NULL, NULL, 3),
('CRISTAL SATINADO', NULL, NULL, 3),
('CRISTAL TINTEX', NULL, NULL, 3),
('CRISTAL TRANSPARENTE', NULL, NULL, 3),
('ESPEJOS', NULL, NULL, 3),
('VIDRIO ARTICO', NULL, NULL, 3),
('VIDRIO LABRADO', NULL, NULL, 3),
('ESCUADRAS RESBALONES RETENES Y SEGUROS', NULL, NULL, 3),
('GAVETAS', NULL, NULL, 3),
('HERRAJE P/VIDRIO', NULL, NULL, 3),
('HERRAJES FIJACION', NULL, NULL, 3),
('JALADERAS', NULL, NULL, 3),
('MAQUINARIA', NULL, NULL, 3),
('MENSULAS', NULL, NULL, 3),
('PASADORES', NULL, NULL, 3),
('PIVOTES', NULL, NULL, 3),
('POLICARBONATOS', NULL, NULL, 3),
('RESBALONES', NULL, NULL, 3),
('SELLAPOLVOS', NULL, NULL, 3),
('TAQUETES', NULL, NULL, 3),
('TITANES', NULL, NULL, 3),
('VENTOSAS', NULL, NULL, 3),
('SOPORTES', NULL, NULL, 3),
('PELICULAS DECORATIVAS', NULL, NULL, 3),
('VINIL', NULL, NULL, 3),
('SELLADORES Y SILICONES', NULL, NULL, 3),
('TELA DE MOSQUITERO', NULL, NULL, 3),
('PANEL ART', NULL, NULL, 3),
('TENSORES', NULL, NULL, 3),
('TORNILLOS', NULL, NULL, 3),
('PLASTICOS HERRALUM', NULL, NULL, 3),
('PLASTICOS DECORADOS', NULL, NULL, 3),
('PLASTICOS LABRADOS', NULL, NULL, 3),
('PLASTICOS LISOS', NULL, NULL, 3),
('REMACHES', NULL, NULL, 3),
('CANALES', NULL, NULL, 3),
('SOLERAS', NULL, NULL, 3),
('PORTONES', NULL, NULL, 3),
('TRABAJOS', NULL, NULL, 3),
('RESIDENCIAL SERIE 50', NULL, NULL, 3),
('SOL LITE', NULL, NULL, 3),
('CRISTAL PAVIA', NULL, NULL, 3),
('10000 ESPAÑOLA', NULL, NULL, 3),
('1400 ESPAÑOLA', NULL, NULL, 3);
";
    }

    #endregion
}









