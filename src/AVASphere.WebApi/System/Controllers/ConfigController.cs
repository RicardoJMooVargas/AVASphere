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

    public ConfigController(
        DatabaseMigrationService dbMigrationService, 
        MasterDbContext dbContext,
        IEncryptionService encryptionService)
    {
        _dbMigrationService = dbMigrationService;
        _dbContext = dbContext;
        _encryptionService = encryptionService;
    }

    [HttpGet("check-initial-config")]
    public async Task<IActionResult> CheckInitialConfig()
    {
        try
        {
            ConfigSys? config = null;
            bool tableExists = false;
            bool hasConfiguration = false;

            try
            {
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

                config = await _dbContext.ConfigSys.FirstOrDefaultAsync();
                tableExists = true;
                hasConfiguration = config != null;
            }
            catch (Exception dbEx)
            {
                var errorMessage = dbEx.Message.ToLower();
                var innerMessage = dbEx.InnerException?.Message?.ToLower() ?? "";

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

    [HttpGet("diagnose-migrations")]
    public async Task<IActionResult> DiagnoseMigrations()
    {
        var result = await _dbMigrationService.DiagnoseMigrationsAsync();
        return Ok(new {
            diagnosis = result,
            info = "Diagnóstico detallado del estado de las migraciones"
        });
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
                return BadRequest(new
                {
                    Success = false,
                    Message = "Ya existe una configuración del sistema. Use el endpoint de actualización."
                });
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

            return Ok(new
            {
                Success = true,
                Message = "Configuración del sistema creada exitosamente",
                Data = new
                {
                    config.IdConfigSys,
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
                Success = false,
                Message = $"Error al configurar el sistema: {ex.Message}"
            });
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
                return BadRequest(new
                {
                    Success = false,
                    Message = "Debe configurar el sistema primero antes de crear el usuario administrador."
                });
            }

            // Verificar si ya existe un usuario administrador
            var existingAdmin = await _dbContext.Users
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Rol.NormalizedName == "admin");
            
            if (existingAdmin != null)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Ya existe un usuario administrador configurado."
                });
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

            // Crear el rol de Administrador con todos los permisos y módulos
            var adminRole = new Rol
            {
                Name = "Administrador",
                NormalizedName = "admin",
                IdArea = areaSystem.IdArea,
                Permissions = new List<Permission>
                {
                    new Permission
                    {
                        Index = 0,
                        Name = "Acceso Total",
                        Normalized = "FULL_ACCESS",
                        Type = "SUPER_ADMIN",
                        Status = "active"
                    }
                },
                Modules = Enum.GetValues(typeof(SystemModule))
                    .Cast<SystemModule>()
                    .Select(module => new Module
                    {
                        Index = (int)module,
                        Name = module.ToString()
                    })
                    .ToList()
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
                Verified = "true",
                CreateAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                IdRol = adminRole.IdRol,
                IdConfigSys = configSys.IdConfigSys
            };

            _dbContext.Users.Add(adminUser);
            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                Success = true,
                Message = "Usuario administrador creado exitosamente",
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
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Success = false,
                Message = $"Error al crear el usuario administrador: {ex.Message}",
                Details = ex.InnerException?.Message
            });
        }
    }
    
    
}