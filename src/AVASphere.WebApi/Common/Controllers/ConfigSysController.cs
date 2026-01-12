using AVASphere.ApplicationCore.Common.Entities.General;
using AVASphere.ApplicationCore.Common.Enums;

namespace AVASphere.WebApi.Common.Controllers;

using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

[ApiController]
[Route("api/common/[controller]")]
[ApiExplorerSettings(GroupName = nameof(SystemModule.Common))]
[Tags("Common - ConfigSys")]
public class ConfigSysController : ControllerBase
{
    private readonly IConfigSysService _configSysService;
    private readonly ILogger<ConfigSysController> _logger;
    
    public ConfigSysController(IConfigSysService configSysService, ILogger<ConfigSysController> logger)
    {
        _configSysService = configSysService;
        _logger = logger;
    }
    
    [HttpGet("get")]
    public async Task<ActionResult> GetConfig()
    {
        try
        {
            _logger.LogInformation("Retrieving system configuration");
            var config = await _configSysService.GetConfigAsync();
            
            if (config == null)
            {
                return NotFound(new ApiResponse("System configuration not found", 404));
            }
            
            // Mapear a DTO de respuesta
            var configResponse = new ConfigSysResponseDto
            {
                IdConfigSys = config.IdConfigSys,
                CompanyName = config.CompanyName,
                BranchName = config.BranchName,
                LogoUrl = config.LogoUrl,
                Colors = config.Colors.Select(c => new ColorResponseDto
                {
                    Index = c.Index,
                    NameColor = c.NameColor,
                    ColorCode = c.ColorCode,
                    ColorRgb = c.ColorRgb
                }).ToList(),
                NotUseModules = config.NotUseModules.Select(m => new NotUseModuleResponseDto
                {
                    Index = m.Index,
                    NameModule = m.NameModule
                }).ToList(),
                CreatedAt = config.CreatedAt
            };
            
            return Ok(new ApiResponse(configResponse, "System configuration retrieved successfully", 200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving system configuration");
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpPost("save")]
    public async Task<ActionResult> SaveConfig([FromBody] ConfigSysRequestDto configRequest)
    {
        try
        {
            if (configRequest == null)
            {
                _logger.LogWarning("Attempt to save null configuration");
                return BadRequest(new ApiResponse("Configuration cannot be null", 400));
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for configuration request");
                return BadRequest(new ApiResponse("Invalid model state", 400, ModelState));
            }

            _logger.LogInformation("Saving system configuration");

            // ...existing code...
            var config = new ConfigSys
            {
                CompanyName = configRequest.CompanyName,
                BranchName = configRequest.BranchName,
                LogoUrl = configRequest.LogoUrl,
                Colors = configRequest.Colors.Select(c => new ColorsJson
                {
                    Index = c.Index,
                    NameColor = c.NameColor,
                    ColorCode = c.ColorCode,
                    ColorRgb = c.ColorRgb
                }).ToList(),
                NotUseModules = configRequest.NotUseModules.Select(m => new NotUseModuleJson
                {
                    Index = m.Index,
                    NameModule = m.NameModule
                }).ToList()
            };

            var savedConfig = await _configSysService.SaveConfigAsync(config);
            
            // ...existing code...
            var response = new ConfigSysResponseDto
            {
                IdConfigSys = savedConfig.IdConfigSys,
                CompanyName = savedConfig.CompanyName,
                BranchName = savedConfig.BranchName,
                LogoUrl = savedConfig.LogoUrl,
                Colors = savedConfig.Colors.Select(c => new ColorResponseDto
                {
                    Index = c.Index,
                    NameColor = c.NameColor,
                    ColorCode = c.ColorCode,
                    ColorRgb = c.ColorRgb
                }).ToList(),
                NotUseModules = savedConfig.NotUseModules.Select(m => new NotUseModuleResponseDto
                {
                    Index = m.Index,
                    NameModule = m.NameModule
                }).ToList(),
                CreatedAt = savedConfig.CreatedAt
            };
            
            return Ok(new ApiResponse(response, "System configuration saved successfully", 200));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while saving configuration");
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving system configuration");
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpPut("update")]
    public async Task<ActionResult> UpdateConfig([FromBody] ConfigSysRequestDto configRequest)
    {
        try
        {
            if (configRequest == null)
            {
                _logger.LogWarning("Attempt to update with null configuration");
                return BadRequest(new ApiResponse("Configuration cannot be null", 400));
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for configuration update request");
                return BadRequest(new ApiResponse("Invalid model state", 400, ModelState));
            }

            _logger.LogInformation("Updating system configuration");

            // ...existing code...
            var config = new ConfigSys
            {
                CompanyName = configRequest.CompanyName,
                BranchName = configRequest.BranchName,
                LogoUrl = configRequest.LogoUrl,
                Colors = configRequest.Colors.Select(c => new ColorsJson
                {
                    Index = c.Index,
                    NameColor = c.NameColor,
                    ColorCode = c.ColorCode,
                    ColorRgb = c.ColorRgb
                }).ToList(),
                NotUseModules = configRequest.NotUseModules.Select(m => new NotUseModuleJson
                {
                    Index = m.Index,
                    NameModule = m.NameModule
                }).ToList()
            };

            var updatedConfig = await _configSysService.SaveConfigAsync(config);
            
            // ...existing code...
            var response = new ConfigSysResponseDto
            {
                IdConfigSys = updatedConfig.IdConfigSys,
                CompanyName = updatedConfig.CompanyName,
                BranchName = updatedConfig.BranchName,
                LogoUrl = updatedConfig.LogoUrl,
                Colors = updatedConfig.Colors.Select(c => new ColorResponseDto
                {
                    Index = c.Index,
                    NameColor = c.NameColor,
                    ColorCode = c.ColorCode,
                    ColorRgb = c.ColorRgb
                }).ToList(),
                NotUseModules = updatedConfig.NotUseModules.Select(m => new NotUseModuleResponseDto
                {
                    Index = m.Index,
                    NameModule = m.NameModule
                }).ToList(),
                CreatedAt = updatedConfig.CreatedAt
            };
            
            return Ok(new ApiResponse(response, "System configuration updated successfully", 200));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while updating configuration");
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating system configuration");
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpGet("check-initial")]
    public async Task<ActionResult> CheckInitialConfig()
    {
        try
        {
            var hasConfig = await _configSysService.HasInitialConfigAsync();
            var tablesExist = await _configSysService.TablesExistAsync();

            if (!hasConfig)
            {
                return Ok(new ApiResponse(new { HasConfig = false, TablesExist = tablesExist, Message = "No initial configuration found" }, "No data", 200));
            }

            var config = await _configSysService.GetConfigAsync();
            var configResponse = new ConfigSysResponseDto
            {
                IdConfigSys = config.IdConfigSys,
                CompanyName = config.CompanyName,
                BranchName = config.BranchName,
                LogoUrl = config.LogoUrl,
                Colors = config.Colors.Select(c => new ColorResponseDto
                {
                    Index = c.Index,
                    NameColor = c.NameColor,
                    ColorCode = c.ColorCode,
                    ColorRgb = c.ColorRgb
                }).ToList(),
                NotUseModules = config.NotUseModules.Select(m => new NotUseModuleResponseDto
                {
                    Index = m.Index,
                    NameModule = m.NameModule
                }).ToList(),
                CreatedAt = config.CreatedAt
            };

            return Ok(new ApiResponse(new { HasConfig = true, TablesExist = tablesExist, Config = configResponse }, "Configuration found", 200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking initial configuration");
            return StatusCode(500, new ApiResponse($"Error checking initial configuration: {ex.Message}", 500));
        }
    }
}