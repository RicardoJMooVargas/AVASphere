using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.Enums;
using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.ApplicationCore.Projects.DTOs;
using AVASphere.ApplicationCore.Projects.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AVASphere.WebApi.Common.Controllers;

[ApiController]
[Route("api/common/[controller]")]
[ApiExplorerSettings(GroupName = nameof(SystemModule.Common))]
[Tags("Common - Catalogs")]
public class CatalogsController : ControllerBase
{
    private readonly IAreaService _areaService;
    private readonly IProjectCategoryService _projectCategoryService;
    private readonly ILogger<CatalogsController> _logger;
    
    public CatalogsController(IAreaService areaService,IProjectCategoryService projectCategoryService, ILogger<CatalogsController> logger)
    {
        _areaService = areaService;
        _logger = logger;
        _projectCategoryService = projectCategoryService;
    }
    
    [HttpPost("new-area")]
    public async Task<ActionResult> NewArea([FromBody] AreaRequestDto areaRequest)
    {
        try
        {
            _logger.LogInformation("Creating a new area with name: {AreaName}", areaRequest.Name);
            var createdArea = await _areaService.CreateAsync(areaRequest);
            
            return CreatedAtAction(nameof(GetAreas), new { id = createdArea.IdArea }, 
                new ApiResponse(createdArea, "Area created successfully", 201));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while creating area: {AreaName}", areaRequest.Name);
            return BadRequest(new ApiResponse(ex.Message, 400));  
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating area with name: {AreaName}", areaRequest.Name);
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }
    
    [HttpGet("get-areas")]
    public async Task<ActionResult> GetAreas([FromQuery] int? id, [FromQuery] string? name)
    {
        try
        {
            if (id.HasValue)
            {
                _logger.LogInformation("Retrieving area with ID: {AreaId}", id);
                var area = await _areaService.GetByIdAsync(id.Value);
                if (area == null)
                    return NotFound(new ApiResponse($"Area with ID {id} not found", 404));
            
                return Ok(new ApiResponse(area, "Area retrieved successfully", 200));
            }

            if (!string.IsNullOrEmpty(name))
            {
                _logger.LogInformation("Retrieving area with name: {AreaName}", name);
                var area = await _areaService.GetByNameAsync(name);
                if (area == null)
                    return NotFound(new ApiResponse($"Area with name '{name}' not found", 404));
            
                return Ok(new ApiResponse(area, "Area retrieved successfully", 200));
            }

            _logger.LogInformation("Retrieving all areas");
            var areas = await _areaService.GetAllAsync();
            return Ok(new ApiResponse(areas, "Areas retrieved successfully", 200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving areas");
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }
    
    [HttpPut("edit-areas/{id}")]
    public async Task<ActionResult> UpdateArea(int id, [FromBody] AreaRequestDto areaRequest)
    {
        try
        {
            _logger.LogInformation("Updating area with ID: {AreaId}", id);
            var updatedArea = await _areaService.UpdateAsync(id, areaRequest);
            return Ok(new ApiResponse(updatedArea, "Area updated successfully", 200));
        }
        catch (KeyNotFoundException keyEx)
        {
            _logger.LogWarning(keyEx, "Area not found for update: {AreaId}", id);
            return NotFound(new ApiResponse(keyEx.Message, 404));
        }
        catch (InvalidOperationException opEx)
        {
            _logger.LogWarning(opEx, "Business rule violation while updating area: {AreaId}", id);
            return BadRequest(new ApiResponse(opEx.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating area with ID: {AreaId}", id);
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }
    
    [HttpDelete("delete-areas/{id}")]
    public async Task<ActionResult> DeleteArea(int id)
    {
        try
        {
            _logger.LogInformation("Deleting area with ID: {AreaId}", id);
            var result = await _areaService.DeleteAsync(id);
            
            if (!result)
            {
                return NotFound(new ApiResponse($"Area with ID {id} not found", 404));
            }
            
            return Ok(new ApiResponse(null, "Area deleted successfully", 200));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while deleting area: {AreaId}", id);
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting area with ID: {AreaId}", id);
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }
    
    
    //Project Category Controller
    
    [HttpPost("new-projectCategory")]
    public async Task<ActionResult> NewProjectCategory([FromBody] ProjectCategoryRequestDto projectCategoryRequest)
    {
        try
        {
            _logger.LogInformation("Creating a new category with name: {ProjectCategoryName}", projectCategoryRequest.Name);
            var createdProjectCategory = await _projectCategoryService.CreateAsync(projectCategoryRequest);
            
            return CreatedAtAction(nameof(GetProjectCategories), new { id = createdProjectCategory.IdProjectCategory }, 
                new ApiResponse(createdProjectCategory, "Project Category created successfully", 201));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while creating category: {ProjectCategoryName}", projectCategoryRequest.Name);
            return BadRequest(new ApiResponse(ex.Message, 400));  
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating category with name: {ProjectCategoryName}", projectCategoryRequest.Name);
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }
    
    [HttpGet("get-projectCategory")]
    public async Task<ActionResult> GetProjectCategories([FromQuery] int? id, [FromQuery] string? name)
    {
        try
        {
            if (id.HasValue)
            {
                _logger.LogInformation("Retrieving project category with ID: {ProjectCategoryId}", id);
                var projectCategory = await _projectCategoryService.GetByIdAsync(id.Value);
                if (projectCategory == null)
                    return NotFound(new ApiResponse($"Project category with ID {id} not found", 404));
            
                return Ok(new ApiResponse(projectCategory, "Project category retrieved successfully", 200));
            }

            if (!string.IsNullOrEmpty(name))
            {
                _logger.LogInformation("Retrieving project category with name: {ProjectCategoryName}", name);
                var projectCategory = await _projectCategoryService.GetByNameAsync(name);
                if (projectCategory == null)
                    return NotFound(new ApiResponse($"Project category with name '{name}' not found", 404));
            
                return Ok(new ApiResponse(projectCategory, "Project category retrieved successfully", 200));
            }

            _logger.LogInformation("Retrieving all project categories");
            var projectCategories = await _projectCategoryService.GetAllAsync();
            return Ok(new ApiResponse(projectCategories, "Project categories retrieved successfully", 200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project categories");
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }
    
    [HttpPut("edit-projectCategory/{id}")]
    public async Task<ActionResult> UpdateProjectCategory(int id, [FromBody] ProjectCategoryRequestDto projectCategoryRequest)
    {
        try
        {
            _logger.LogInformation("Updating project category with ID: {ProjectCategoryId}", id);
            var updatedProjectCategory = await _projectCategoryService.UpdateAsync(id, projectCategoryRequest);
            return Ok(new ApiResponse(updatedProjectCategory, "Project Category updated successfully", 200));
        }
        catch (KeyNotFoundException keyEx)
        {
            _logger.LogWarning(keyEx, "Project Category not found for update: {ProyectCategoryId}", id);
            return NotFound(new ApiResponse(keyEx.Message, 404));
        }
        catch (InvalidOperationException opEx)
        {
            _logger.LogWarning(opEx, "Business rule violation while updating project category: {ProjectCategoryId}", id);
            return BadRequest(new ApiResponse(opEx.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project category with ID: {ProjectCategoryId}", id);
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }
    
    /*[HttpDelete("delete-projectCategory/{id}")]
    public async Task<ActionResult> DeleteProjectCategory(int id)
    {
        try
        {
            _logger.LogInformation("Deleting project category with ID: {ProjectCategoryId}", id);
            var result = await _projectCategoryService.DeleteAsync(id);
            
            if (!result)
            {
                return NotFound(new ApiResponse($"Project category with ID {id} not found", 404));
            }
            
            return Ok(new ApiResponse(null, "Project category deleted successfully", 200));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while deleting project category: {ProjectCategoryId}", id);
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project category with ID: {ProjectCategoryId}", id);
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }*/
}